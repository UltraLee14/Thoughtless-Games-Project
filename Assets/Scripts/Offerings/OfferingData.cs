using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "OfferingData", menuName = "ScriptableObjects/OfferingData")]
public class OfferingData : ScriptableObject
{
    [System.Serializable]
    public class OfferingDataElement
    {
        [InspectorName("Variable String Name")]
        public string variableStringName;

        [InspectorName("Variable Value")]
        public float variableValue;
    }

    [System.Serializable]
    public class Offering
    {
        [InspectorName("Offering Name")]
        public string offeringName;

        [InspectorName("Unlocked")]
        public bool unlocked;

        [InspectorName("Is Carrying")]
        public bool isCarrying;

        [InspectorName("Offering Data")]
        public OfferingDataElement[] offeringData;
    }

    [InspectorName("Offerings")]
    public Offering[] Offerings;
}

#if UNITY_EDITOR
[CustomEditor(typeof(OfferingData))]
public class OfferingDataEditor : Editor
{
    SerializedProperty offeringsProp;

    void OnEnable()
    {
        offeringsProp = serializedObject.FindProperty("Offerings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(offeringsProp, true);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
