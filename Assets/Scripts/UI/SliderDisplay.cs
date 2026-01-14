using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderDisplay : MonoBehaviour
{
    [SerializeField] Slider targetSlider;
    [SerializeField] TMP_Text valueText;

    [SerializeField, InspectorName("Update Player Stats Object")]
    bool updateScriptableObject;

    [SerializeField, InspectorName("Player Stats Object")]
    PlayerStats playerStatsObject;

    void Update()
    {
        if (targetSlider == null || valueText == null) return;

        int wholeValue = Mathf.RoundToInt(targetSlider.value);
        valueText.text = wholeValue.ToString();
    }

    public void UpdateTargetDataValue()
    {
        if (!updateScriptableObject) return;
        if (playerStatsObject == null) return;
        if (targetSlider == null) return;

        playerStatsObject.lookSpeed = targetSlider.value;
    }
}
