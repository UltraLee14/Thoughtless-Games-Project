using UnityEngine;

public class OfferingDataUpdate : MonoBehaviour
{
    [Header("References")]
    [SerializeField, InspectorName("Player Stats Object")]
    PlayerStats playerStatsObject;

    [Header("Target")]
    [SerializeField, InspectorName("Offering Data To Update")]
    string offeringDataToUpdate;

    public void SetIsCarryingTrue()
    {
        if (playerStatsObject == null) return;
        if (playerStatsObject.offerings == null) return;

        for (int i = 0; i < playerStatsObject.offerings.Length; i++)
        {
            if (playerStatsObject.offerings[i].offeringName == offeringDataToUpdate)
            {
                playerStatsObject.offerings[i].isCarrying = true;
                break;
            }
        }
    }

    public void SetAllCarryingToFalse()
    {
        if (playerStatsObject == null) return;
        if (playerStatsObject.offerings == null) return;

        for (int i = 0; i < playerStatsObject.offerings.Length; i++)
        {
            playerStatsObject.offerings[i].isCarrying = false;
        }
    }

    public void BankAllCarry()
    {
        if (playerStatsObject == null) return;
        if (playerStatsObject.offerings == null) return;

        for (int i = 0; i < playerStatsObject.offerings.Length; i++)
        {
            if (playerStatsObject.offerings[i].isCarrying)
            {
                playerStatsObject.offerings[i].unlocked = true;
            }
        }

        SetAllCarryingToFalse();
    }
}
