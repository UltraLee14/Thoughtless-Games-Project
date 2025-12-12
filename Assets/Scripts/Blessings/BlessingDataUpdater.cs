using UnityEngine;

public class BlessingDataUpdater : MonoBehaviour
{
    [Header("References")]
    [SerializeField, InspectorName("Blessing Data")]
    BlessingData blessingData;

    [Header("Target")]
    [SerializeField, InspectorName("Blessing Data To Update")]
    string blessingDataToUpdate;

    public void SetIsCarryingTrue()
    {
        for (int i = 0; i < blessingData.blessings.Length; i++)
        {
            if (blessingData.blessings[i].blessingName == blessingDataToUpdate)
            {
                blessingData.blessings[i].isCarrying = true;
                break;
            }
        }
    }

    public void SetAllCarryingToFalse()
    {
        for (int i = 0; i < blessingData.blessings.Length; i++)
        {
            blessingData.blessings[i].isCarrying = false;
        }
    }

    public void BankAllCarry()
    {
        for (int i = 0; i < blessingData.blessings.Length; i++)
        {
            if (blessingData.blessings[i].isCarrying)
            {
                blessingData.blessings[i].unlocked = true;
            }
        }

        SetAllCarryingToFalse();
    }
}
