using UnityEngine;
using UnityEngine.Events;

public class AllOfferingsUnlocked : MonoBehaviour
{
    [Header("References")]
    [SerializeField, InspectorName("Offering Data")]
    OfferingData offeringData;

    [Header("Events")]
    [SerializeField, InspectorName("Unlock End Event")]
    UnityEvent UnlockEndEvent = new UnityEvent();

    void Start()
    {
        if (offeringData == null) return;

        if (offeringData.Offerings == null || offeringData.Offerings.Length == 0) return;

        for (int i = 0; i < offeringData.Offerings.Length; i++)
        {
            if (!offeringData.Offerings[i].unlocked)
                return;
        }

        UnlockEndEvent.Invoke();
    }
}
