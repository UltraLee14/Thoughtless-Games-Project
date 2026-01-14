using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlessingSelectManager : MonoBehaviour
{
    [SerializeField, InspectorName("Player Stats Object")]
    PlayerStats playerStatsObject;

    [SerializeField, InspectorName("Starting Blessing")]
    int StartingBlessing;

    [SerializeField, InspectorName("Slot Images")]
    Image[] slotImages;

    [SerializeField, InspectorName("Slot Texts")]
    TMP_Text[] slotTexts;

    int[] currentBlessingIndices;

    void Start()
    {
        for (int i = 0; i < playerStatsObject.blessings.Length; i++)
            playerStatsObject.blessings[i].blessingActive = false;

        currentBlessingIndices = new int[slotImages.Length];

        for (int slot = 0; slot < slotImages.Length; slot++)
        {
            currentBlessingIndices[slot] = 0;
            UpdateSlotImage(slot);
        }
    }

    public void NextUpgrade(int slotIndex)
    {
        ChangeUpgrade(slotIndex, 1);
    }

    public void PrevUpgrade(int slotIndex)
    {
        ChangeUpgrade(slotIndex, -1);
    }

    void ChangeUpgrade(int slotIndex, int direction)
    {
        int previousIndex = currentBlessingIndices[slotIndex];
        int newIndex = FindNextEligibleIndex(previousIndex, direction);

        if (newIndex == previousIndex)
            return;

        if (previousIndex != 0)
            playerStatsObject.blessings[previousIndex].blessingActive = false;

        currentBlessingIndices[slotIndex] = newIndex;

        if (newIndex != 0)
            playerStatsObject.blessings[newIndex].blessingActive = true;

        UpdateSlotImage(slotIndex);
    }

    int FindNextEligibleIndex(int startIndex, int direction)
    {
        int length = playerStatsObject.blessings.Length;
        int index = startIndex;

        for (int i = 0; i < length; i++)
        {
            index = (index + direction + length) % length;

            if (IsBlessingEligible(index))
                return index;
        }

        return startIndex;
    }

    int FindNextEligibleIndexInclusive(int startIndex)
    {
        int length = playerStatsObject.blessings.Length;
        int index = startIndex;

        for (int i = 0; i < length; i++)
        {
            if (IsBlessingEligible(index))
                return index;

            index++;
            if (index >= length)
                index = 0;
        }

        return startIndex;
    }

    bool IsBlessingEligible(int index)
    {
        var blessing = playerStatsObject.blessings[index];

        if (!blessing.unlocked)
            return false;

        if (index == 0)
            return true;

        return !blessing.blessingActive;
    }

    void UpdateSlotImage(int slotIndex)
    {
        var blessing = playerStatsObject.blessings[currentBlessingIndices[slotIndex]];
        slotImages[slotIndex].sprite = blessing.itemImage;

        if (slotTexts != null && slotIndex < slotTexts.Length && slotTexts[slotIndex] != null)
            slotTexts[slotIndex].text = blessing.blessingName;
    }
}
