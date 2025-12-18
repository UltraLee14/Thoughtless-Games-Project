using UnityEngine;

public class OfferingDataUpdate : MonoBehaviour
{
    [Header("References")]
    [SerializeField, InspectorName("Offering Data")]
    OfferingData offeringData;

    [Header("Target")]
    [SerializeField, InspectorName("Offering Data To Update")]
    string offeringDataToUpdate;

    public void SetIsCarryingTrue()
    {
        for (int i = 0; i < offeringData.Offerings.Length; i++)
        {
            if (offeringData.Offerings[i].offeringName == offeringDataToUpdate)
            {
                offeringData.Offerings[i].isCarrying = true;
                break;
            }
        }
    }

    public void SetAllCarryingToFalse()
    {
        for (int i = 0; i < offeringData.Offerings.Length; i++)
        {
            offeringData.Offerings[i].isCarrying = false;
        }
    }

    public void BankAllCarry()
    {
        for (int i = 0; i < offeringData.Offerings.Length; i++)
        {
            if (offeringData.Offerings[i].isCarrying)
            {
                offeringData.Offerings[i].unlocked = true;
            }
        }

        SetAllCarryingToFalse();
    }
}
