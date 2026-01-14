using UnityEngine;

public class BlessingDataUpdater : MonoBehaviour
{
    [Header("References")]
    [SerializeField, InspectorName("Player Stats Object")]
    PlayerStats playerStatsObject;

    [Header("Target")]
    [SerializeField, InspectorName("Blessing Data To Update")]
    string blessingDataToUpdate;

    public void SetIsCarryingTrue()
    {
        if (playerStatsObject == null) return;
        if (playerStatsObject.blessings == null) return;

        for (int i = 0; i < playerStatsObject.blessings.Length; i++)
        {
            if (playerStatsObject.blessings[i].blessingName == blessingDataToUpdate)
            {
                playerStatsObject.blessings[i].isCarrying = true;
                break;
            }
        }
    }

    public void SetAllCarryingToFalse()
    {
        if (playerStatsObject == null) return;
        if (playerStatsObject.blessings == null) return;

        for (int i = 0; i < playerStatsObject.blessings.Length; i++)
        {
            playerStatsObject.blessings[i].isCarrying = false;
        }
    }

    public void BankAllCarry()
    {
        if (playerStatsObject == null) return;
        if (playerStatsObject.blessings == null) return;

        for (int i = 0; i < playerStatsObject.blessings.Length; i++)
        {
            if (playerStatsObject.blessings[i].isCarrying)
            {
                playerStatsObject.blessings[i].unlocked = true;
            }
        }

        SetAllCarryingToFalse();
    }
}
