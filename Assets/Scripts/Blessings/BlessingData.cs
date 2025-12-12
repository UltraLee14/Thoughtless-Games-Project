using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "BlessingData", menuName = "ScriptableObjects/BlessingData")]
public class BlessingData : ScriptableObject
{
    [System.Serializable]
    public class BlessingDataElement
    {
        [InspectorName("Variable String Name")]
        public string variableStringName;

        [InspectorName("Variable Value")]
        public float variableValue;
    }

    [System.Serializable]
    public class Blessing
    {
        [InspectorName("Blessing Name")]
        public string blessingName;

        [InspectorName("Unlocked")]
        public bool unlocked;

        [InspectorName("Blessing Active")]
        public bool blessingActive;

        [InspectorName("Is Carrying")]
        public bool isCarrying;

        [InspectorName("Item Image")]
        public Sprite itemImage;

        [InspectorName("Blessing Data")]
        public BlessingDataElement[] blessingData;
    }

    [System.Serializable]
    public class PlayerSettings
    {
        [InspectorName("LookSpeed")]
        public int lookSpeed = 1;
    }

    [SerializeField, InspectorName("Player Settings")]
    PlayerSettings playerSettings;

    [InspectorName("Blessings")]
    public Blessing[] blessings;
}

#if UNITY_EDITOR
[CustomEditor(typeof(BlessingData))]
public class BlessingDataEditor : Editor
{
    bool showPlayerSettings = true;

    SerializedProperty blessingsProp;
    SerializedProperty playerSettingsProp;

    void OnEnable()
    {
        blessingsProp = serializedObject.FindProperty("blessings");
        playerSettingsProp = serializedObject.FindProperty("playerSettings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(blessingsProp, true);

        EditorGUILayout.Space();
        showPlayerSettings = EditorGUILayout.Foldout(showPlayerSettings, "PlayerSettings", true);
        if (showPlayerSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(playerSettingsProp, new GUIContent("Player Settings"), true);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
