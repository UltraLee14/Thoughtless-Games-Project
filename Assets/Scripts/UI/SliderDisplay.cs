using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

public class SliderDisplay : MonoBehaviour
{
    [SerializeField] Slider targetSlider;
    [SerializeField] TMP_Text valueText;

    [SerializeField, InspectorName("Variable String Name")]
    string variableStringName;

    [SerializeField, InspectorName("Update Scriptable Object")]
    bool updateScriptableObject;

    [SerializeField, InspectorName("Blessing Data")]
    BlessingData blessingData;

    void Update()
    {
        if (targetSlider == null || valueText == null) return;

        int wholeValue = Mathf.RoundToInt(targetSlider.value);
        valueText.text = wholeValue.ToString();
    }

    public void UpdateTargetDataValue()
    {
        if (!updateScriptableObject) return;
        if (blessingData == null) return;
        if (targetSlider == null) return;
        if (string.IsNullOrEmpty(variableStringName)) return;

        int wholeValue = Mathf.RoundToInt(targetSlider.value);

        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo playerSettingsField = typeof(BlessingData).GetField("playerSettings", flags);
        if (playerSettingsField == null) return;

        object playerSettingsObj = playerSettingsField.GetValue(blessingData);
        if (playerSettingsObj == null) return;

        FieldInfo valueField = playerSettingsObj.GetType().GetField(variableStringName, flags);
        if (valueField == null) return;

        if (valueField.FieldType == typeof(int))
        {
            valueField.SetValue(playerSettingsObj, wholeValue);
        }
        else if (valueField.FieldType == typeof(float))
        {
            valueField.SetValue(playerSettingsObj, (float)wholeValue);
        }
        else if (valueField.FieldType == typeof(bool))
        {
            valueField.SetValue(playerSettingsObj, wholeValue != 0);
        }
    }
}
