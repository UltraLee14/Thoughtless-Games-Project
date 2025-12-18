using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerManager : MonoBehaviour
{
    [System.Serializable]
    public struct AxisMoveSpeed
    {
        [InspectorName("Z (Forward / Back)")]
        public float z;

        [InspectorName("X (Right / Left)")]
        public float x;
    }

    [SerializeField, InspectorName("Blessing Data")]
    BlessingData blessingData;

    [SerializeField, InspectorName("Camera")]
    GameObject cameraObject;

    [SerializeField, InspectorName("Walk Move Speed")]
    AxisMoveSpeed walkMoveSpeed;

    [SerializeField, InspectorName("Sneak Move Speed")]
    AxisMoveSpeed sneakMoveSpeed;

    [SerializeField, InspectorName("Sprint Move Speed")]
    AxisMoveSpeed sprintMoveSpeed;

    [SerializeField, InspectorName("Sneak Active")]
    bool sneakActive;

    [SerializeField, InspectorName("Is Sprint Active")]
    bool isSprintActive;

    [SerializeField, InspectorName("Can Jump")]
    bool canJump;

    [SerializeField, InspectorName("Jump Force")]
    float jumpForce;

    [SerializeField, InspectorName("Look Speed")]
    float lookSpeed;

    [SerializeField, InspectorName("Player Root")]
    GameObject playerRoot;

    [SerializeField, InspectorName("Player Ground Collider")]
    Collider playerGroundCollider;

    [SerializeField, InspectorName("Sound Detection Trigger")]
    Collider soundDetectionTrigger;

    [SerializeField, HideInInspector, InspectorName("Sonar Collider")]
    Collider sonarCollider;

    [SerializeField, HideInInspector, InspectorName("Sonar Collider Size")]
    float sonarColliderSize;

    [SerializeField, HideInInspector, InspectorName("Sonar Charges")]
    int sonarCharges;

    [SerializeField, HideInInspector, InspectorName("Sonar Text")]
    TMP_Text sonarText;

    [SerializeField, InspectorName("Detection Multiplier")]
    float detectionMultiplier;

    [SerializeField, InspectorName("Detection Multiplier Step")]
    float detectionMultiplierStep;

    [SerializeField, HideInInspector, InspectorName("Clank Timer")]
    float clankTimer;

    [SerializeField, HideInInspector, InspectorName("Clank Increase Multiplier")]
    float clankIncreaseMultiplier;

    [SerializeField, HideInInspector, InspectorName("Heartbeat Interval")]
    float heartbeatInterval;

    [SerializeField, HideInInspector, InspectorName("Play Heartbeat")]
    UnityEvent PlayHeartbeat;

    [SerializeField, HideInInspector, InspectorName("Clank 0 Event")]
    UnityEvent Clank0Event;
    [SerializeField, HideInInspector, InspectorName("Clank 1 Event")]
    UnityEvent Clank1Event;
    [SerializeField, HideInInspector, InspectorName("Clank 2 Event")]
    UnityEvent Clank2Event;
    [SerializeField, HideInInspector, InspectorName("Clank 3 Event")]
    UnityEvent Clank3Event;
    [SerializeField, HideInInspector, InspectorName("Clank 4 Event")]
    UnityEvent Clank4Event;
    [SerializeField, HideInInspector, InspectorName("Clank Max Event")]
    UnityEvent ClankMaxEvent;

    [SerializeField, HideInInspector, InspectorName("Clank 0 Min Timer")]
    float clank0MinTimer;
    [SerializeField, HideInInspector, InspectorName("Clank 0 Max Timer")]
    float clank0MaxTimer;

    [SerializeField, HideInInspector, InspectorName("Clank 1 Min Timer")]
    float clank1MinTimer;
    [SerializeField, HideInInspector, InspectorName("Clank 1 Max Timer")]
    float clank1MaxTimer;

    [SerializeField, HideInInspector, InspectorName("Clank 2 Min Timer")]
    float clank2MinTimer;
    [SerializeField, HideInInspector, InspectorName("Clank 2 Max Timer")]
    float clank2MaxTimer;

    [SerializeField, HideInInspector, InspectorName("Clank 3 Min Timer")]
    float clank3MinTimer;
    [SerializeField, HideInInspector, InspectorName("Clank 3 Max Timer")]
    float clank3MaxTimer;

    [SerializeField, HideInInspector, InspectorName("Clank 4 Min Timer")]
    float clank4MinTimer;
    [SerializeField, HideInInspector, InspectorName("Clank 4 Max Timer")]
    float clank4MaxTimer;

    [SerializeField, InspectorName("Player Rigidbody")]
    Rigidbody playerRigidbody;

    [SerializeField, InspectorName("Velocity Tracker")]
    VelocityTracker velocityTracker;

    [SerializeField, InspectorName("Is Moving")]
    bool isMoving;

    [SerializeField, InspectorName("Player Move Speed"), ReadOnly]
    AxisMoveSpeed playerMoveSpeed;

    [SerializeField, InspectorName("Sneak Set To True")]
    UnityEvent SneakSetToTrue;

    [SerializeField, InspectorName("Sneak Set To False")]
    UnityEvent SneakSetToFalse;

    [SerializeField, InspectorName("Sprint Set To True")]
    UnityEvent SprintSetToTrue;

    [SerializeField, InspectorName("Sprint Set To False")]
    UnityEvent SprintSetToFalse;

    [Header("Movement Events")]
    [SerializeField, InspectorName("Start Sprint")]
    UnityEvent StartSprint;

    [SerializeField, InspectorName("Start Walk")]
    UnityEvent StartWalk;

    [SerializeField, InspectorName("Start Sneak")]
    UnityEvent StartSneak;

    [SerializeField, InspectorName("Not Moving")]
    UnityEvent NotMoving;

    [SerializeField, InspectorName("Start Event")]
    UnityEvent StartEvent;

    [SerializeField, HideInInspector, InspectorName("Debug Clank Timer Text")]
    TMP_Text debugClankTimerText;

    float cameraPitch;
    Vector3 currentLocalMoveVelocity;
    const float accelTime = 0.1f;
    bool jumpRequested;

    bool clank0Fired, clank1Fired, clank2Fired, clank3Fired, clank4Fired;

    enum MoveEventState
    {
        NotMoving,
        Walk,
        Sprint,
        Sneak
    }

    MoveEventState lastMoveEventState = MoveEventState.NotMoving;
    bool lastIsMoving;

    void Start()
    {
        playerMoveSpeed.z = walkMoveSpeed.z;
        playerMoveSpeed.x = walkMoveSpeed.x;

        if (playerRigidbody == null && playerRoot != null)
            playerRigidbody = playerRoot.GetComponent<Rigidbody>();

        Vector3 angles = cameraObject.transform.localEulerAngles;
        cameraPitch = angles.x;
        if (cameraPitch > 180f)
            cameraPitch -= 360f;

        clankTimer = 0f;

        ApplyBlessingData();

        InvokeRepeating(nameof(ClankTick), 1f, 1f);

        StartEvent?.Invoke();

        StartCoroutine(HeartbeatLoop());

        if (Application.isFocused)
            Cursor.visible = false;

        lastIsMoving = isMoving;
        lastMoveEventState = MoveEventState.NotMoving;

        UpdateSonarText();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        Cursor.visible = !hasFocus;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
        playerRoot.transform.Rotate(0f, mouseX, 0f, Space.Self);

        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        Vector3 angles = cameraObject.transform.localEulerAngles;
        angles.x = cameraPitch;
        cameraObject.transform.localEulerAngles = angles;

        bool sprintKey = Input.GetKey(KeyCode.LeftShift);
        bool sneakKey = Input.GetKey(KeyCode.LeftControl);

        if (sprintKey)
        {
            SetSprintActive(true);
            SetSneakActive(false);
        }
        else if (sneakKey)
        {
            SetSneakActive(true);
            SetSprintActive(false);
        }
        else
        {
            SetSprintActive(false);
            SetSneakActive(false);
        }

        AxisMoveSpeed targetSpeed = walkMoveSpeed;
        if (isSprintActive)
            targetSpeed = sprintMoveSpeed;
        else if (sneakActive)
            targetSpeed = sneakMoveSpeed;

        playerMoveSpeed = targetSpeed;

        float inputZ = 0f;
        if (Input.GetKey(KeyCode.W)) inputZ += 1f;
        if (Input.GetKey(KeyCode.S)) inputZ -= 1f;

        float inputX = 0f;
        if (Input.GetKey(KeyCode.D)) inputX += 1f;
        if (Input.GetKey(KeyCode.A)) inputX -= 1f;

        float desiredZ = inputZ * playerMoveSpeed.z;
        float desiredX = inputX * playerMoveSpeed.x;

        float accelZ = playerMoveSpeed.z != 0f ? Mathf.Abs(playerMoveSpeed.z) / accelTime : 0f;
        float accelX = playerMoveSpeed.x != 0f ? Mathf.Abs(playerMoveSpeed.x) / accelTime : 0f;

        currentLocalMoveVelocity.z = Mathf.MoveTowards(
            currentLocalMoveVelocity.z,
            desiredZ,
            accelZ * Time.deltaTime
        );

        currentLocalMoveVelocity.x = Mathf.MoveTowards(
            currentLocalMoveVelocity.x,
            desiredX,
            accelX * Time.deltaTime
        );

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            jumpRequested = true;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            TryUseSonar();
        }

        if (debugClankTimerText != null)
            debugClankTimerText.text = clankTimer.ToString();

        UpdateSonarText();

        EvaluateMovementEvents();
    }

    void FixedUpdate()
    {
        if (playerRigidbody != null)
        {
            Vector3 worldMove = playerRoot.transform.TransformDirection(currentLocalMoveVelocity);
            playerRigidbody.MovePosition(playerRigidbody.position + worldMove * Time.fixedDeltaTime);

            if (jumpRequested && canJump)
            {
                playerRigidbody.AddForce(playerRoot.transform.up * jumpForce, ForceMode.VelocityChange);
                canJump = false;
            }

            isMoving = currentLocalMoveVelocity.sqrMagnitude > 0.0001f;

            if (soundDetectionTrigger != null)
            {
                float radius = 0f;

                if (isMoving)
                {
                    float absX = Mathf.Abs(currentLocalMoveVelocity.x);
                    float absZ = Mathf.Abs(currentLocalMoveVelocity.z);
                    radius = Mathf.Max(absX, absZ) * detectionMultiplier;
                }

                if (soundDetectionTrigger is SphereCollider sphere)
                {
                    sphere.radius = radius;
                }
                else if (soundDetectionTrigger is CapsuleCollider capsule)
                {
                    capsule.radius = radius;
                }
            }
        }
        else
        {
            Vector3 worldMove = playerRoot.transform.TransformDirection(currentLocalMoveVelocity);
            playerRoot.transform.position += worldMove * Time.fixedDeltaTime;

            isMoving = currentLocalMoveVelocity.sqrMagnitude > 0.0001f;
        }
        jumpRequested = false;

        EvaluateMovementEvents();
    }

    void UpdateSonarText()
    {
        if (sonarText != null)
            sonarText.text = $"Sonar Pulse: {sonarCharges}";
    }

    void EvaluateMovementEvents()
    {
        if (lastIsMoving && !isMoving)
        {
            NotMoving?.Invoke();
        }

        MoveEventState currentState = DetermineMoveEventState();

        if (currentState != lastMoveEventState)
        {
            if (currentState == MoveEventState.Sprint)
                StartSprint?.Invoke();
            else if (currentState == MoveEventState.Sneak)
                StartSneak?.Invoke();
            else if (currentState == MoveEventState.Walk)
                StartWalk?.Invoke();

            lastMoveEventState = currentState;
        }

        lastIsMoving = isMoving;
    }

    MoveEventState DetermineMoveEventState()
    {
        if (!isMoving)
            return MoveEventState.NotMoving;

        if (isSprintActive)
            return MoveEventState.Sprint;

        if (sneakActive)
            return MoveEventState.Sneak;

        return MoveEventState.Walk;
    }

    void ClankTick()
    {
        clankTimer += 1f * clankIncreaseMultiplier;

        CheckAndFireRange(clank0MinTimer, clank0MaxTimer, ref clank0Fired, Clank0Event);
        CheckAndFireRange(clank1MinTimer, clank1MaxTimer, ref clank1Fired, Clank1Event);
        CheckAndFireRange(clank2MinTimer, clank2MaxTimer, ref clank2Fired, Clank2Event);
        CheckAndFireRange(clank3MinTimer, clank3MaxTimer, ref clank3Fired, Clank3Event);
        CheckAndFireRange(clank4MinTimer, clank4MaxTimer, ref clank4Fired, Clank4Event);
    }

    void CheckAndFireRange(float min, float max, ref bool fired, UnityEvent evt)
    {
        if (!fired && clankTimer >= min && clankTimer <= max)
        {
            evt?.Invoke();
            fired = true;
        }
    }

    public IEnumerator HeartbeatLoop()
    {
        while (true)
        {
            PlayHeartbeat?.Invoke();
            Debug.Log($"Play Heartbeat fired on '{name}'");
            yield return new WaitForSeconds(heartbeatInterval);
        }
    }

    public void SetHeartbeatInterval(float value)
    {
        heartbeatInterval = value;
    }

    public void IncreaseDetectionMultiplier()
    {
        detectionMultiplier += detectionMultiplierStep;
    }

    void SetSneakActive(bool value)
    {
        if (sneakActive == value)
            return;

        sneakActive = value;

        if (sneakActive)
        {
            if (SneakSetToTrue != null)
                SneakSetToTrue.Invoke();
        }
        else
        {
            if (SneakSetToFalse != null)
                SneakSetToFalse.Invoke();
        }
    }

    void SetSprintActive(bool value)
    {
        if (isSprintActive == value)
            return;

        isSprintActive = value;

        if (isSprintActive)
        {
            if (SprintSetToTrue != null)
                SprintSetToTrue.Invoke();
        }
        else
        {
            if (SprintSetToFalse != null)
                SprintSetToFalse.Invoke();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && !other.CompareTag("Player"))
        {
            canJump = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger && !other.CompareTag("Player"))
        {
            if (playerGroundCollider != null)
            {
                Vector3 checkPos = playerGroundCollider.bounds.center;
                float checkRadius = Mathf.Min(
                    playerGroundCollider.bounds.extents.x,
                    playerGroundCollider.bounds.extents.z
                ) * 0.9f;

                Collider[] hits = Physics.OverlapSphere(
                    checkPos,
                    checkRadius,
                    ~0,
                    QueryTriggerInteraction.Ignore
                );

                bool stillGrounded = false;
                foreach (var c in hits)
                {
                    if (!c.isTrigger && !c.CompareTag("Player"))
                    {
                        stillGrounded = true;
                        break;
                    }
                }

                if (!stillGrounded)
                    canJump = false;
            }
            else
            {
                canJump = false;
            }
        }
    }

    void TryUseSonar()
    {
        if (sonarCharges <= 0)
            return;

        if (sonarCollider == null)
            return;

        sonarCharges--;
        UpdateSonarText();
        StartCoroutine(SonarPulse());
    }

    IEnumerator SonarPulse()
    {
        sonarCollider.enabled = true;
        yield return new WaitForSeconds(0.1f);
        sonarCollider.enabled = false;
    }

    void ApplyBlessingData()
    {
        if (blessingData == null)
            return;

        ApplyPlayerSettingsFromBlessingData();

        if (blessingData.blessings != null)
        {
            var blessings = blessingData.blessings;
            for (int i = 0; i < blessings.Length; i++)
            {
                var blessing = blessings[i];
                if (!blessing.blessingActive)
                    continue;

                var dataArray = blessing.blessingData;
                if (dataArray == null)
                    continue;

                for (int j = 0; j < dataArray.Length; j++)
                {
                    var element = dataArray[j];
                    if (string.IsNullOrEmpty(element.variableStringName))
                        continue;

                    ApplyVariableOverride(element.variableStringName, element.variableValue);
                }
            }
        }

        ApplySonarSettings();
        UpdateSonarText();
    }

    void ApplyPlayerSettingsFromBlessingData()
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo playerSettingsField = typeof(BlessingData).GetField("playerSettings", flags);
        if (playerSettingsField == null)
            return;

        object playerSettingsObj = playerSettingsField.GetValue(blessingData);
        if (playerSettingsObj == null)
            return;

        var settingsType = playerSettingsObj.GetType();
        FieldInfo[] settingFields = settingsType.GetFields(flags);

        for (int i = 0; i < settingFields.Length; i++)
        {
            FieldInfo f = settingFields[i];
            object val = f.GetValue(playerSettingsObj);
            if (val == null)
                continue;

            float numericValue;

            if (val is int intVal)
                numericValue = intVal;
            else if (val is float floatVal)
                numericValue = floatVal;
            else if (val is bool boolVal)
                numericValue = boolVal ? 1f : 0f;
            else
                continue;

            ApplyVariableOverride(f.Name, numericValue);
        }
    }

    void ApplySonarSettings()
    {
        if (sonarCollider == null)
            return;

        if (sonarCollider is SphereCollider sphere)
        {
            sphere.radius = sonarColliderSize;
        }
        else if (sonarCollider is CapsuleCollider capsule)
        {
            capsule.radius = sonarColliderSize;
        }
    }

    void ApplyVariableOverride(string variableName, float value)
    {
        if (string.IsNullOrEmpty(variableName))
            return;

        string fieldName = variableName;
        string subField = null;

        int dotIndex = variableName.IndexOf('.');
        if (dotIndex >= 0)
        {
            fieldName = variableName.Substring(0, dotIndex);
            subField = variableName.Substring(dotIndex + 1);
        }

        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo field = GetType().GetField(fieldName, flags);
        if (field == null)
            return;

        if (subField == null)
        {
            if (field.FieldType == typeof(float))
                field.SetValue(this, value);
            else if (field.FieldType == typeof(int))
                field.SetValue(this, (int)value);
            else if (field.FieldType == typeof(bool))
                field.SetValue(this, value != 0f);

            return;
        }

        object fieldValue = field.GetValue(this);

        if (field.FieldType == typeof(AxisMoveSpeed))
        {
            AxisMoveSpeed axis = (AxisMoveSpeed)fieldValue;
            if (subField == "z")
                axis.z = value;
            else if (subField == "x")
                axis.x = value;

            field.SetValue(this, axis);
        }
    }
}

public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}

[CustomEditor(typeof(PlayerManager))]
public class PlayerManagerEditor : Editor
{
    bool showDebug = true;
    bool showClank = true;
    bool showDifficulty = true;
    bool showSonar = true;

    SerializedProperty debugClankTimerTextProp;
    SerializedProperty clankTimerProp;
    SerializedProperty clankIncreaseMultiplierProp;
    SerializedProperty heartbeatIntervalProp;
    SerializedProperty playHeartbeatProp;

    SerializedProperty clank0Prop;
    SerializedProperty clank1Prop;
    SerializedProperty clank2Prop;
    SerializedProperty clank3Prop;
    SerializedProperty clank4Prop;
    SerializedProperty clankMaxProp;

    SerializedProperty clank0MinProp, clank0MaxProp;
    SerializedProperty clank1MinProp, clank1MaxProp;
    SerializedProperty clank2MinProp, clank2MaxProp;
    SerializedProperty clank3MinProp, clank3MaxProp;
    SerializedProperty clank4MinProp, clank4MaxProp;

    SerializedProperty sonarColliderProp;
    SerializedProperty sonarColliderSizeProp;
    SerializedProperty sonarChargesProp;
    SerializedProperty sonarTextProp;

    void OnEnable()
    {
        debugClankTimerTextProp = serializedObject.FindProperty("debugClankTimerText");
        clankTimerProp = serializedObject.FindProperty("clankTimer");
        clankIncreaseMultiplierProp = serializedObject.FindProperty("clankIncreaseMultiplier");
        heartbeatIntervalProp = serializedObject.FindProperty("heartbeatInterval");
        playHeartbeatProp = serializedObject.FindProperty("PlayHeartbeat");

        clank0Prop = serializedObject.FindProperty("Clank0Event");
        clank1Prop = serializedObject.FindProperty("Clank1Event");
        clank2Prop = serializedObject.FindProperty("Clank2Event");
        clank3Prop = serializedObject.FindProperty("Clank3Event");
        clank4Prop = serializedObject.FindProperty("Clank4Event");
        clankMaxProp = serializedObject.FindProperty("ClankMaxEvent");

        clank0MinProp = serializedObject.FindProperty("clank0MinTimer");
        clank0MaxProp = serializedObject.FindProperty("clank0MaxTimer");
        clank1MinProp = serializedObject.FindProperty("clank1MinTimer");
        clank1MaxProp = serializedObject.FindProperty("clank1MaxTimer");
        clank2MinProp = serializedObject.FindProperty("clank2MinTimer");
        clank2MaxProp = serializedObject.FindProperty("clank2MaxTimer");
        clank3MinProp = serializedObject.FindProperty("clank3MinTimer");
        clank3MaxProp = serializedObject.FindProperty("clank3MaxTimer");
        clank4MinProp = serializedObject.FindProperty("clank4MinTimer");
        clank4MaxProp = serializedObject.FindProperty("clank4MaxTimer");

        sonarColliderProp = serializedObject.FindProperty("sonarCollider");
        sonarColliderSizeProp = serializedObject.FindProperty("sonarColliderSize");
        sonarChargesProp = serializedObject.FindProperty("sonarCharges");
        sonarTextProp = serializedObject.FindProperty("sonarText");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space();
        showSonar = EditorGUILayout.Foldout(showSonar, "Sonar Settings", true);
        if (showSonar)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(sonarColliderProp, new GUIContent("Sonar Collider"));
            EditorGUILayout.PropertyField(sonarColliderSizeProp, new GUIContent("Sonar Collider Size"));
            EditorGUILayout.PropertyField(sonarChargesProp, new GUIContent("Sonar Charges"));
            EditorGUILayout.PropertyField(sonarTextProp, new GUIContent("Sonar Text"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        showClank = EditorGUILayout.Foldout(showClank, "Clank Settings", true);
        if (showClank)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(clankTimerProp, new GUIContent("Clank Timer"));
            EditorGUILayout.PropertyField(clankIncreaseMultiplierProp, new GUIContent("Clank Increase Multiplier"));
            EditorGUILayout.PropertyField(heartbeatIntervalProp, new GUIContent("Heartbeat Interval"));
            EditorGUILayout.PropertyField(playHeartbeatProp, new GUIContent("Play Heartbeat"));

            EditorGUILayout.Space();
            showDifficulty = EditorGUILayout.Foldout(showDifficulty, "Difficulty events", true);
            if (showDifficulty)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Clank 0");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(clank0Prop, new GUIContent("Event"));
                EditorGUILayout.PropertyField(clank0MinProp, new GUIContent("Min Timer"));
                EditorGUILayout.PropertyField(clank0MaxProp, new GUIContent("Max Timer"));
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Clank 1");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(clank1Prop, new GUIContent("Event"));
                EditorGUILayout.PropertyField(clank1MinProp, new GUIContent("Min Timer"));
                EditorGUILayout.PropertyField(clank1MaxProp, new GUIContent("Max Timer"));
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Clank 2");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(clank2Prop, new GUIContent("Event"));
                EditorGUILayout.PropertyField(clank2MinProp, new GUIContent("Min Timer"));
                EditorGUILayout.PropertyField(clank2MaxProp, new GUIContent("Max Timer"));
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Clank 3");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(clank3Prop, new GUIContent("Event"));
                EditorGUILayout.PropertyField(clank3MinProp, new GUIContent("Min Timer"));
                EditorGUILayout.PropertyField(clank3MaxProp, new GUIContent("Max Timer"));
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Clank 4");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(clank4Prop, new GUIContent("Event"));
                EditorGUILayout.PropertyField(clank4MinProp, new GUIContent("Min Timer"));
                EditorGUILayout.PropertyField(clank4MaxProp, new GUIContent("Max Timer"));
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(clankMaxProp, new GUIContent("Clank Max Event"));

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        showDebug = EditorGUILayout.Foldout(showDebug, "Debug", true);
        if (showDebug)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(debugClankTimerTextProp, new GUIContent("Debug Clank Timer Text"));
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
