using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

[CreateAssetMenu(fileName = "PlayerStats", menuName = "ScriptableObjects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Serializable]
    public class StatValue
    {
        [InspectorName("Element Name")]
        public string elementName;

        [InspectorName("Base Value")]
        public string baseValue;

        [SerializeField, InspectorName("Load Value")]
        string loadValue;

        [Header("Value Type")]
        [InspectorName("String")]
        public bool valueTypeString = true;

        [InspectorName("Int")]
        public bool valueTypeInt;

        [InspectorName("Float")]
        public bool valueTypeFloat;

        public string GetLoadValue()
        {
            return loadValue;
        }

        public void SetLoadValue(string v)
        {
            loadValue = v;
        }
    }

    [Serializable]
    public class BlessingDataElement
    {
        [InspectorName("Target Stat Value")]
        public string targetStatValue;

        [InspectorName("Stat Value Modifier")]
        public string statValueModifier;

        [Header("Modifier Type")]
        [InspectorName("Add")]
        public bool add = true;

        [InspectorName("Multiply")]
        public bool multiply;
    }

    [Serializable]
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

    [Serializable]
    public class ControlValue
    {
        [InspectorName("Action Name")]
        public string actionName;

        [InspectorName("Is Keybind")]
        public bool isKeybind = true;

        [InspectorName("Default Key")]
        public KeyCode defaultKey;

        [InspectorName("Bound Key")]
        public KeyCode boundKey;
    }

    [Serializable]
    public class OfferingDataElement
    {
        [InspectorName("Variable String Name")]
        public string variableStringName;

        [InspectorName("Variable Value")]
        public float variableValue;
    }

    [Serializable]
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

    [SerializeField, InspectorName("Stat Values")]
    public StatValue[] statValues;

    [SerializeField, InspectorName("Control Values")]
    public ControlValue[] controlValues;

    [SerializeField, InspectorName("Look Speed")]
    public float lookSpeed;

    [InspectorName("Blessings")]
    public Blessing[] blessings;

    [InspectorName("Offerings")]
    public Offering[] offerings;

    [Header("Loot")]
    [SerializeField, InspectorName("Gold Balance")]
    public int goldBalance;

    [SerializeField, InspectorName("Pending Gold Balance")]
    public int pendingGoldBalance;

    public void RecalculateLoadValues()
    {
        if (statValues != null)
        {
            for (int i = 0; i < statValues.Length; i++)
            {
                if (statValues[i] == null) continue;
                statValues[i].SetLoadValue(statValues[i].baseValue ?? string.Empty);
            }
        }

        if (blessings == null || statValues == null) return;

        for (int b = 0; b < blessings.Length; b++)
        {
            var blessing = blessings[b];
            if (blessing == null) continue;
            if (!blessing.blessingActive) continue;
            if (blessing.blessingData == null) continue;

            for (int d = 0; d < blessing.blessingData.Length; d++)
            {
                var e = blessing.blessingData[d];
                if (e == null) continue;
                if (string.IsNullOrWhiteSpace(e.targetStatValue)) continue;

                int statIndex = FindStatIndexByName(e.targetStatValue);
                if (statIndex < 0) continue;

                var s = statValues[statIndex];
                if (s == null) continue;

                string baseRaw = s.baseValue ?? string.Empty;
                string currentRaw = s.GetLoadValue() ?? baseRaw;
                string modRaw = e.statValueModifier ?? string.Empty;

                string computed = ComputeModifiedValue(currentRaw, baseRaw, modRaw, e.add, e.multiply);
                s.SetLoadValue(ApplyValueType(computed, s.valueTypeString, s.valueTypeInt, s.valueTypeFloat));
            }
        }
    }

    int FindStatIndexByName(string name)
    {
        if (statValues == null) return -1;

        for (int i = 0; i < statValues.Length; i++)
        {
            var s = statValues[i];
            if (s == null) continue;

            if (string.Equals((s.elementName ?? string.Empty).Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    string ComputeModifiedValue(string currentRaw, string baseRaw, string modRaw, bool add, bool multiply)
    {
        bool a = add && !multiply;
        bool m = multiply && !add;

        if (!a && !m)
            a = true;

        if (float.TryParse(baseRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float baseNum) &&
            float.TryParse(modRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float modNum))
        {
            float result = a ? (baseNum + modNum) : (baseNum * modNum);
            return result.ToString(CultureInfo.InvariantCulture);
        }

        if (float.TryParse(currentRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float currentNum) &&
            float.TryParse(modRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float modNum2))
        {
            float result = a ? (currentNum + modNum2) : (currentNum * modNum2);
            return result.ToString(CultureInfo.InvariantCulture);
        }

        if (a)
            return (baseRaw ?? string.Empty) + (modRaw ?? string.Empty);

        return baseRaw ?? string.Empty;
    }

    string ApplyValueType(string raw, bool asString, bool asInt, bool asFloat)
    {
        bool s = asString && !asInt && !asFloat;
        bool i = asInt && !asString && !asFloat;
        bool f = asFloat && !asString && !asInt;

        if (!s && !i && !f)
            s = true;

        if (s) return raw ?? string.Empty;

        if (i)
        {
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int iv))
                return iv.ToString(CultureInfo.InvariantCulture);

            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float fv))
                return Mathf.RoundToInt(fv).ToString(CultureInfo.InvariantCulture);

            return "0";
        }

        if (f)
        {
            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float fv))
                return fv.ToString(CultureInfo.InvariantCulture);

            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int iv))
                return ((float)iv).ToString(CultureInfo.InvariantCulture);

            return "0";
        }

        return raw ?? string.Empty;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerStats))]
public class PlayerStatsEditor : Editor
{
    bool showPlayerStats = true;
    bool showBlessings = true;
    bool showControlSettings = true;
    bool showOfferings = true;
    bool showLoot = true;

    SerializedProperty statValuesProp;
    SerializedProperty blessingsProp;
    SerializedProperty controlValuesProp;
    SerializedProperty lookSpeedProp;
    SerializedProperty offeringsProp;

    SerializedProperty goldBalanceProp;
    SerializedProperty pendingGoldBalanceProp;

    ReorderableList statList;

    void OnEnable()
    {
        statValuesProp = serializedObject.FindProperty("statValues");
        blessingsProp = serializedObject.FindProperty("blessings");
        controlValuesProp = serializedObject.FindProperty("controlValues");
        lookSpeedProp = serializedObject.FindProperty("lookSpeed");
        offeringsProp = serializedObject.FindProperty("offerings");

        goldBalanceProp = serializedObject.FindProperty("goldBalance");
        pendingGoldBalanceProp = serializedObject.FindProperty("pendingGoldBalance");

        statList = new ReorderableList(serializedObject, statValuesProp, true, true, true, true);
        statList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Stat Values");
        };

        statList.elementHeightCallback = (int index) =>
        {
            float h = EditorGUIUtility.singleLineHeight;
            h += EditorGUIUtility.singleLineHeight + 4f;
            h += EditorGUIUtility.singleLineHeight + 4f;
            h += EditorGUIUtility.singleLineHeight + 4f;
            h += EditorGUIUtility.singleLineHeight + 6f;
            h += EditorGUIUtility.singleLineHeight + 2f;
            return h + 10f;
        };

        statList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            rect.y += 2f;

            var el = statValuesProp.GetArrayElementAtIndex(index);
            var elementNameProp = el.FindPropertyRelative("elementName");
            var baseValueProp = el.FindPropertyRelative("baseValue");
            var loadValueProp = el.FindPropertyRelative("loadValue");
            var strProp = el.FindPropertyRelative("valueTypeString");
            var intProp = el.FindPropertyRelative("valueTypeInt");
            var floatProp = el.FindPropertyRelative("valueTypeFloat");

            string labelName = string.IsNullOrWhiteSpace(elementNameProp.stringValue) ? $"Element {index}" : elementNameProp.stringValue;
            Rect line = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(line, labelName, EditorStyles.boldLabel);

            line.y += EditorGUIUtility.singleLineHeight + 4f;
            EditorGUI.PropertyField(line, elementNameProp, new GUIContent("Element Name"));

            line.y += EditorGUIUtility.singleLineHeight + 4f;
            EditorGUI.PropertyField(line, baseValueProp, new GUIContent("Base Value"));

            line.y += EditorGUIUtility.singleLineHeight + 4f;
            using (new EditorGUI.DisabledScope(true))
                EditorGUI.PropertyField(line, loadValueProp, new GUIContent("Load Value"));

            line.y += EditorGUIUtility.singleLineHeight + 6f;
            EditorGUI.LabelField(line, "Value Type", EditorStyles.boldLabel);

            line.y += EditorGUIUtility.singleLineHeight + 2f;
            float third = line.width / 3f;

            Rect r0 = new Rect(line.x, line.y, third, line.height);
            Rect r1 = new Rect(line.x + third, line.y, third, line.height);
            Rect r2 = new Rect(line.x + third * 2f, line.y, third, line.height);

            bool bStr = EditorGUI.ToggleLeft(r0, "String", strProp.boolValue);
            bool bInt = EditorGUI.ToggleLeft(r1, "Int", intProp.boolValue);
            bool bFloat = EditorGUI.ToggleLeft(r2, "Float", floatProp.boolValue);

            if (bStr != strProp.boolValue || bInt != intProp.boolValue || bFloat != floatProp.boolValue)
            {
                if (bStr)
                {
                    strProp.boolValue = true;
                    intProp.boolValue = false;
                    floatProp.boolValue = false;
                }
                else if (bInt)
                {
                    strProp.boolValue = false;
                    intProp.boolValue = true;
                    floatProp.boolValue = false;
                }
                else if (bFloat)
                {
                    strProp.boolValue = false;
                    intProp.boolValue = false;
                    floatProp.boolValue = true;
                }
                else
                {
                    strProp.boolValue = true;
                    intProp.boolValue = false;
                    floatProp.boolValue = false;
                }
            }
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        showPlayerStats = EditorGUILayout.Foldout(showPlayerStats, "Player Stats", true);
        if (showPlayerStats)
        {
            EditorGUI.indentLevel++;
            statList.DoLayoutList();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(6);
        }

        showBlessings = EditorGUILayout.Foldout(showBlessings, "Blessings", true);
        if (showBlessings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(blessingsProp, true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(6);
        }

        showOfferings = EditorGUILayout.Foldout(showOfferings, "Offerings", true);
        if (showOfferings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(offeringsProp, true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(6);
        }

        showControlSettings = EditorGUILayout.Foldout(showControlSettings, "Control Settings", true);
        if (showControlSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(controlValuesProp, true);
            EditorGUILayout.PropertyField(lookSpeedProp, new GUIContent("Look Speed"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(6);
        }

        showLoot = EditorGUILayout.Foldout(showLoot, "Loot", true);
        if (showLoot)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(goldBalanceProp, new GUIContent("Gold Balance"));
            EditorGUILayout.PropertyField(pendingGoldBalanceProp, new GUIContent("Pending Gold Balance"));
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(PlayerStats.BlessingDataElement))]
public class PlayerStatsBlessingDataElementDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float h = EditorGUIUtility.singleLineHeight;
        h += (EditorGUIUtility.singleLineHeight + 2f) * 2f;
        h += EditorGUIUtility.singleLineHeight + 6f;
        h += EditorGUIUtility.singleLineHeight + 2f;
        return h + 4f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var targetProp = property.FindPropertyRelative("targetStatValue");
        var modProp = property.FindPropertyRelative("statValueModifier");
        var addProp = property.FindPropertyRelative("add");
        var multProp = property.FindPropertyRelative("multiply");

        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(line, targetProp, new GUIContent("Target Stat Value"));

        line.y += EditorGUIUtility.singleLineHeight + 2f;
        EditorGUI.PropertyField(line, modProp, new GUIContent("Stat Value Modifier"));

        line.y += EditorGUIUtility.singleLineHeight + 6f;
        EditorGUI.LabelField(line, "Modifier Type", EditorStyles.boldLabel);

        line.y += EditorGUIUtility.singleLineHeight + 2f;

        float half = line.width / 2f;
        Rect r0 = new Rect(line.x, line.y, half, line.height);
        Rect r1 = new Rect(line.x + half, line.y, half, line.height);

        bool bAdd = EditorGUI.ToggleLeft(r0, "Add", addProp.boolValue);
        bool bMult = EditorGUI.ToggleLeft(r1, "Multiply", multProp.boolValue);

        if (bAdd != addProp.boolValue || bMult != multProp.boolValue)
        {
            if (bAdd)
            {
                addProp.boolValue = true;
                multProp.boolValue = false;
            }
            else if (bMult)
            {
                addProp.boolValue = false;
                multProp.boolValue = true;
            }
            else
            {
                addProp.boolValue = true;
                multProp.boolValue = false;
            }
        }

        EditorGUI.EndProperty();
    }
}
#endif
