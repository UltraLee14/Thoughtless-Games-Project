using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField, InspectorName("Player Stats Object")]
    PlayerStats playerStatsObject;

    [SerializeField, InspectorName("Camera")]
    Transform cameraTransform;

    [SerializeField, InspectorName("Player Root")]
    Transform playerRoot;

    [SerializeField, InspectorName("Sound Trigger Sphere")]
    SphereCollider soundTriggerSphere;

    [SerializeField, InspectorName("Sonar Trigger Sphere")]
    SphereCollider sonarTriggerSphere;

    [SerializeField, InspectorName("Sonar Charge Display Text")]
    TMP_Text sonarChargeDisplayText;

    [SerializeField, InspectorName("Collected Gold Display Text")]
    TMP_Text collectedGoldDisplayText;

    [SerializeField] float walkMoveSpeed;
    [SerializeField] float sneakMoveSpeed;
    [SerializeField] float sprintMoveSpeed;

    [SerializeField] float jumpForce;

    [SerializeField] float lookSpeed;

    [SerializeField] float soundTriggerMultiplier;

    [SerializeField] float sonarRadius;
    [SerializeField] int sonarCharges;

    [SerializeField] KeyCode moveForwardKey;
    [SerializeField] KeyCode moveBackwardKey;
    [SerializeField] KeyCode moveLeftKey;
    [SerializeField] KeyCode moveRightKey;
    [SerializeField] KeyCode sprintKey;
    [SerializeField] KeyCode sneakKey;
    [SerializeField] KeyCode jumpKey;
    [SerializeField] KeyCode sonarPulseKey;

    [SerializeField, InspectorName("Current Speed")]
    float currentSpeed;

    [SerializeField, InspectorName("Sound Trigger Radius")]
    float soundTriggerRadius;

    CharacterController cc;
    bool statsLoaded;

    float pitch;
    float verticalVelocity;

    const float gravity = -9.81f;
    const float groundedStick = -2f;

    Coroutine sonarRoutine;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (playerRoot == null) playerRoot = transform;

        if (sonarTriggerSphere != null)
            sonarTriggerSphere.enabled = false;
    }

    void Start()
    {
        if (playerStatsObject != null)
            playerStatsObject.pendingGoldBalance = 0;

        ApplyPlayerStats();
        ApplyControlValues();
        ApplyLookSpeed();
        CacheInitialPitch();
        statsLoaded = true;

        UpdateSonarUI();
        UpdateCollectedGoldUI();
    }

    void CacheInitialPitch()
    {
        if (cameraTransform == null) return;

        float x = cameraTransform.localEulerAngles.x;
        if (x > 180f) x -= 360f;
        pitch = Mathf.Clamp(x, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void ApplyLookSpeed()
    {
        if (playerStatsObject == null) return;
        lookSpeed = playerStatsObject.lookSpeed;
    }

    void ApplyPlayerStats()
    {
        if (playerStatsObject == null) return;

        playerStatsObject.RecalculateLoadValues();

        var stats = playerStatsObject.statValues;
        if (stats == null) return;

        var t = GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        for (int i = 0; i < stats.Length; i++)
        {
            var s = stats[i];
            if (string.IsNullOrWhiteSpace(s.elementName)) continue;

            string targetName = s.elementName.Trim();
            string raw = s.GetLoadValue();

            FieldInfo f = t.GetField(targetName, flags);
            if (f != null)
            {
                object converted = ConvertToTargetType(raw, f.FieldType);
                if (converted != null) f.SetValue(this, converted);
                continue;
            }

            PropertyInfo p = t.GetProperty(targetName, flags);
            if (p != null && p.CanWrite)
            {
                object converted = ConvertToTargetType(raw, p.PropertyType);
                if (converted != null) p.SetValue(this, converted);
            }
        }
    }

    void ApplyControlValues()
    {
        if (playerStatsObject == null) return;
        if (playerStatsObject.controlValues == null) return;

        moveForwardKey = GetBoundKey("Move Forward");
        moveBackwardKey = GetBoundKey("Move Backward");
        moveLeftKey = GetBoundKey("Move Left");
        moveRightKey = GetBoundKey("Move Right");
        sprintKey = GetBoundKey("Sprint");
        sneakKey = GetBoundKey("Sneak");
        jumpKey = GetBoundKey("Jump");
        sonarPulseKey = GetBoundKey("Echo Locate");
    }

    KeyCode GetBoundKey(string actionName)
    {
        var arr = playerStatsObject.controlValues;
        for (int i = 0; i < arr.Length; i++)
        {
            var cv = arr[i];
            if (cv == null) continue;
            if (!cv.isKeybind) continue;

            if (string.Equals(cv.actionName, actionName, StringComparison.OrdinalIgnoreCase))
                return cv.boundKey;
        }

        return KeyCode.None;
    }

    object ConvertToTargetType(string raw, Type targetType)
    {
        if (targetType == typeof(string)) return raw ?? string.Empty;

        if (targetType == typeof(int))
        {
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int iv)) return iv;

            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float fv))
                return Mathf.RoundToInt(fv);

            return 0;
        }

        if (targetType == typeof(float))
        {
            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float fv)) return fv;

            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int iv))
                return (float)iv;

            return 0f;
        }

        if (targetType == typeof(bool))
        {
            if (bool.TryParse(raw, out bool bv)) return bv;

            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int iv))
                return iv != 0;

            return false;
        }

        try
        {
            return Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    void Update()
    {
        HandleLook();
        HandleMove();
        UpdateCurrentSpeed();
        UpdateSoundDetection();
        UpdateSonarSphere();
        HandleSonarPulse();
        UpdateSonarUI();
        UpdateCollectedGoldUI();
    }

    void HandleLook()
    {
        if (cameraTransform == null) return;
        if (playerRoot == null) playerRoot = transform;

        float mx = Input.GetAxisRaw("Mouse X");
        float my = Input.GetAxisRaw("Mouse Y");

        float yawDelta = mx * lookSpeed;
        playerRoot.Rotate(0f, yawDelta, 0f, Space.Self);

        pitch -= my * lookSpeed;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void HandleMove()
    {
        float x = 0f;
        float z = 0f;

        if (moveLeftKey != KeyCode.None && Input.GetKey(moveLeftKey)) x -= 1f;
        if (moveRightKey != KeyCode.None && Input.GetKey(moveRightKey)) x += 1f;
        if (moveBackwardKey != KeyCode.None && Input.GetKey(moveBackwardKey)) z -= 1f;
        if (moveForwardKey != KeyCode.None && Input.GetKey(moveForwardKey)) z += 1f;

        Vector3 input = new Vector3(x, 0f, z);
        if (input.sqrMagnitude > 1f) input.Normalize();

        float speed = walkMoveSpeed;

        bool sneak = sneakKey != KeyCode.None && Input.GetKey(sneakKey);
        bool sprint = sprintKey != KeyCode.None && Input.GetKey(sprintKey);

        if (sneak) speed = sneakMoveSpeed;
        else if (sprint) speed = sprintMoveSpeed;

        bool grounded = cc.isGrounded;

        if (grounded && verticalVelocity < 0f)
            verticalVelocity = groundedStick;

        if (grounded && jumpKey != KeyCode.None && Input.GetKeyDown(jumpKey))
            verticalVelocity = jumpForce;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 horizontalVelocity = playerRoot.TransformDirection(input) * speed;
        Vector3 displacement = (horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime;

        cc.Move(displacement);
    }

    void UpdateCurrentSpeed()
    {
        Vector3 v = cc.velocity;
        currentSpeed = new Vector3(v.x, 0f, v.z).magnitude;
    }

    void UpdateSoundDetection()
    {
        soundTriggerRadius = currentSpeed * soundTriggerMultiplier;

        if (soundTriggerSphere != null)
            soundTriggerSphere.radius = soundTriggerRadius;
    }

    void UpdateSonarSphere()
    {
        if (sonarTriggerSphere != null)
            sonarTriggerSphere.radius = sonarRadius;
    }

    void HandleSonarPulse()
    {
        if (sonarPulseKey == KeyCode.None) return;
        if (!Input.GetKeyDown(sonarPulseKey)) return;
        if (sonarCharges <= 0) return;
        if (sonarTriggerSphere == null) return;

        sonarCharges--;

        if (sonarRoutine != null) StopCoroutine(sonarRoutine);
        sonarRoutine = StartCoroutine(SonarPulseRoutine());
    }

    IEnumerator SonarPulseRoutine()
    {
        sonarTriggerSphere.enabled = true;
        yield return new WaitForSeconds(0.25f);
        sonarTriggerSphere.enabled = false;
    }

    void UpdateSonarUI()
    {
        if (sonarChargeDisplayText == null) return;
        sonarChargeDisplayText.text = $"Sonar Remaining: {sonarCharges}";
    }

    void UpdateCollectedGoldUI()
    {
        if (collectedGoldDisplayText == null) return;
        if (playerStatsObject == null) return;
        collectedGoldDisplayText.text = $"Gold Collected: {playerStatsObject.pendingGoldBalance}";
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PlayerController))]
    class PlayerControllerEditor : Editor
    {
        bool showDataObjects = true;
        bool showObjectReferences = true;
        bool showUIObjectReferences = true;
        bool showPlayerStats = true;

        bool showMovementSpeeds = true;
        bool showJumpSettings = true;
        bool showSoundDetection = true;
        bool showSonarPulse = true;
        bool showControlValues = true;

        SerializedProperty playerStatsObjectProp;

        SerializedProperty cameraTransformProp;
        SerializedProperty playerRootProp;
        SerializedProperty soundTriggerSphereProp;
        SerializedProperty sonarTriggerSphereProp;

        SerializedProperty sonarChargeDisplayTextProp;
        SerializedProperty collectedGoldDisplayTextProp;

        SerializedProperty walkMoveSpeedProp;
        SerializedProperty sneakMoveSpeedProp;
        SerializedProperty sprintMoveSpeedProp;

        SerializedProperty jumpForceProp;
        SerializedProperty lookSpeedProp;

        SerializedProperty soundTriggerMultiplierProp;
        SerializedProperty soundTriggerRadiusProp;

        SerializedProperty sonarRadiusProp;
        SerializedProperty sonarChargesProp;

        SerializedProperty moveForwardKeyProp;
        SerializedProperty moveBackwardKeyProp;
        SerializedProperty moveLeftKeyProp;
        SerializedProperty moveRightKeyProp;
        SerializedProperty sprintKeyProp;
        SerializedProperty sneakKeyProp;
        SerializedProperty jumpKeyProp;
        SerializedProperty sonarPulseKeyProp;

        SerializedProperty currentSpeedProp;

        void OnEnable()
        {
            playerStatsObjectProp = serializedObject.FindProperty("playerStatsObject");

            cameraTransformProp = serializedObject.FindProperty("cameraTransform");
            playerRootProp = serializedObject.FindProperty("playerRoot");
            soundTriggerSphereProp = serializedObject.FindProperty("soundTriggerSphere");
            sonarTriggerSphereProp = serializedObject.FindProperty("sonarTriggerSphere");

            sonarChargeDisplayTextProp = serializedObject.FindProperty("sonarChargeDisplayText");
            collectedGoldDisplayTextProp = serializedObject.FindProperty("collectedGoldDisplayText");

            walkMoveSpeedProp = serializedObject.FindProperty("walkMoveSpeed");
            sneakMoveSpeedProp = serializedObject.FindProperty("sneakMoveSpeed");
            sprintMoveSpeedProp = serializedObject.FindProperty("sprintMoveSpeed");

            jumpForceProp = serializedObject.FindProperty("jumpForce");
            lookSpeedProp = serializedObject.FindProperty("lookSpeed");

            soundTriggerMultiplierProp = serializedObject.FindProperty("soundTriggerMultiplier");
            soundTriggerRadiusProp = serializedObject.FindProperty("soundTriggerRadius");

            sonarRadiusProp = serializedObject.FindProperty("sonarRadius");
            sonarChargesProp = serializedObject.FindProperty("sonarCharges");

            moveForwardKeyProp = serializedObject.FindProperty("moveForwardKey");
            moveBackwardKeyProp = serializedObject.FindProperty("moveBackwardKey");
            moveLeftKeyProp = serializedObject.FindProperty("moveLeftKey");
            moveRightKeyProp = serializedObject.FindProperty("moveRightKey");
            sprintKeyProp = serializedObject.FindProperty("sprintKey");
            sneakKeyProp = serializedObject.FindProperty("sneakKey");
            jumpKeyProp = serializedObject.FindProperty("jumpKey");
            sonarPulseKeyProp = serializedObject.FindProperty("sonarPulseKey");

            currentSpeedProp = serializedObject.FindProperty("currentSpeed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var pc = (PlayerController)target;

            showDataObjects = EditorGUILayout.Foldout(showDataObjects, "Data Objects", true);
            if (showDataObjects)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(playerStatsObjectProp, new GUIContent("Player Stats Object"));
                }
                EditorGUILayout.Space(6);
            }

            showObjectReferences = EditorGUILayout.Foldout(showObjectReferences, "Object References", true);
            if (showObjectReferences)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(cameraTransformProp, new GUIContent("Camera"));
                    EditorGUILayout.PropertyField(playerRootProp, new GUIContent("Player Root"));
                    EditorGUILayout.PropertyField(soundTriggerSphereProp, new GUIContent("Sound Trigger Sphere"));
                    EditorGUILayout.PropertyField(sonarTriggerSphereProp, new GUIContent("Sonar Trigger Sphere"));
                }
                EditorGUILayout.Space(6);
            }

            showUIObjectReferences = EditorGUILayout.Foldout(showUIObjectReferences, "UI Object References", true);
            if (showUIObjectReferences)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(sonarChargeDisplayTextProp, new GUIContent("Sonar Charge Display Text"));
                    EditorGUILayout.PropertyField(collectedGoldDisplayTextProp, new GUIContent("Collected Gold Display Text"));
                }
                EditorGUILayout.Space(6);
            }

            showPlayerStats = EditorGUILayout.Foldout(showPlayerStats, "Player Stats", true);
            if (showPlayerStats)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    showMovementSpeeds = EditorGUILayout.Foldout(showMovementSpeeds, "Movement Speeds", true);
                    if (showMovementSpeeds)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            if (!pc.statsLoaded)
                            {
                                using (new EditorGUI.DisabledScope(true))
                                {
                                    EditorGUILayout.TextField("Walk Move Speed", "");
                                    EditorGUILayout.TextField("Sneak Move Speed", "");
                                    EditorGUILayout.TextField("Sprint Move Speed", "");
                                    EditorGUILayout.TextField("Current Speed", "");
                                }
                            }
                            else
                            {
                                using (new EditorGUI.DisabledScope(true))
                                {
                                    EditorGUILayout.PropertyField(walkMoveSpeedProp, new GUIContent("Walk Move Speed"));
                                    EditorGUILayout.PropertyField(sneakMoveSpeedProp, new GUIContent("Sneak Move Speed"));
                                    EditorGUILayout.PropertyField(sprintMoveSpeedProp, new GUIContent("Sprint Move Speed"));
                                    EditorGUILayout.PropertyField(currentSpeedProp, new GUIContent("Current Speed"));
                                }
                            }
                        }
                    }

                    showJumpSettings = EditorGUILayout.Foldout(showJumpSettings, "Jump Settings", true);
                    if (showJumpSettings)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            if (!pc.statsLoaded)
                            {
                                using (new EditorGUI.DisabledScope(true))
                                {
                                    EditorGUILayout.TextField("Jump Force", "");
                                }
                            }
                            else
                            {
                                using (new EditorGUI.DisabledScope(true))
                                {
                                    EditorGUILayout.PropertyField(jumpForceProp, new GUIContent("Jump Force"));
                                }
                            }
                        }
                    }

                    showSoundDetection = EditorGUILayout.Foldout(showSoundDetection, "Sound Detection", true);
                    if (showSoundDetection)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            if (!pc.statsLoaded)
                            {
                                using (new EditorGUI.DisabledScope(true))
                                {
                                    EditorGUILayout.TextField("Sound Trigger Multiplier", "");
                                    EditorGUILayout.TextField("Sound Trigger Radius", "");
                                }
                            }
                            else
                            {
                                using (new EditorGUI.DisabledScope(true))
                                {
                                    EditorGUILayout.PropertyField(soundTriggerMultiplierProp, new GUIContent("Sound Trigger Multiplier"));
                                    EditorGUILayout.PropertyField(soundTriggerRadiusProp, new GUIContent("Sound Trigger Radius"));
                                }
                            }
                        }
                    }

                    showSonarPulse = EditorGUILayout.Foldout(showSonarPulse, "Sonar Pulse", true);
                    if (showSonarPulse)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            if (!pc.statsLoaded)
                            {
                                using (new EditorGUI.DisabledScope(true))
                                {
                                    EditorGUILayout.TextField("Sonar Radius", "");
                                    EditorGUILayout.TextField("Sonar Charges", "");
                                }
                            }
                            else
                            {
                                using (new EditorGUI.DisabledScope(true))
                                {
                                    EditorGUILayout.PropertyField(sonarRadiusProp, new GUIContent("Sonar Radius"));
                                    EditorGUILayout.PropertyField(sonarChargesProp, new GUIContent("Sonar Charges"));
                                }
                            }
                        }
                    }

                    showControlValues = EditorGUILayout.Foldout(showControlValues, "Control Values", true);
                    if (showControlValues)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            if (!pc.statsLoaded)
                            {
                                using (new EditorGUI.DisabledScope(true))
                                {
                                    EditorGUILayout.TextField("Look Speed", "");
                                    EditorGUILayout.TextField("Move Forward", "");
                                    EditorGUILayout.TextField("Move Backward", "");
                                    EditorGUILayout.TextField("Move Left", "");
                                    EditorGUILayout.TextField("Move Right", "");
                                    EditorGUILayout.TextField("Sprint", "");
                                    EditorGUILayout.TextField("Sneak", "");
                                    EditorGUILayout.TextField("Jump", "");
                                    EditorGUILayout.TextField("Sonar Pulse", "");
                                }
                            }
                            else
                            {
                                using (new EditorGUI.DisabledScope(true))
                                {
                                    EditorGUILayout.PropertyField(lookSpeedProp, new GUIContent("Look Speed"));
                                    EditorGUILayout.PropertyField(moveForwardKeyProp, new GUIContent("Move Forward"));
                                    EditorGUILayout.PropertyField(moveBackwardKeyProp, new GUIContent("Move Backward"));
                                    EditorGUILayout.PropertyField(moveLeftKeyProp, new GUIContent("Move Left"));
                                    EditorGUILayout.PropertyField(moveRightKeyProp, new GUIContent("Move Right"));
                                    EditorGUILayout.PropertyField(sprintKeyProp, new GUIContent("Sprint"));
                                    EditorGUILayout.PropertyField(sneakKeyProp, new GUIContent("Sneak"));
                                    EditorGUILayout.PropertyField(jumpKeyProp, new GUIContent("Jump"));
                                    EditorGUILayout.PropertyField(sonarPulseKeyProp, new GUIContent("Sonar Pulse"));
                                }
                            }
                        }
                    }
                }

                EditorGUILayout.Space(6);
            }

            DrawPropertiesExcluding(
                serializedObject,
                "m_Script",
                "playerStatsObject",
                "cameraTransform",
                "playerRoot",
                "soundTriggerSphere",
                "sonarTriggerSphere",
                "sonarChargeDisplayText",
                "collectedGoldDisplayText",
                "walkMoveSpeed",
                "sneakMoveSpeed",
                "sprintMoveSpeed",
                "jumpForce",
                "lookSpeed",
                "soundTriggerMultiplier",
                "soundTriggerRadius",
                "sonarRadius",
                "sonarCharges",
                "moveForwardKey",
                "moveBackwardKey",
                "moveLeftKey",
                "moveRightKey",
                "sprintKey",
                "sneakKey",
                "jumpKey",
                "sonarPulseKey",
                "currentSpeed"
            );

            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying)
                Repaint();
        }
    }
#endif
}
