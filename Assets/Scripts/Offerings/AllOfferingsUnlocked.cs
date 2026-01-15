using UnityEngine;
using UnityEngine.Events;

public class AllOfferingsUnlocked : MonoBehaviour
{
    [Header("References")]
    [SerializeField, InspectorName("Player Stats Object")]
    PlayerStats playerStatsObject;

    [Header("Events")]
    [SerializeField, InspectorName("Unlock End Event")]
    UnityEvent UnlockEndEvent = new UnityEvent();

    void Start()
    {
        if (playerStatsObject == null) return;

        if (playerStatsObject.offerings == null || playerStatsObject.offerings.Length == 0) return;

        for (int i = 0; i < playerStatsObject.offerings.Length; i++)
        {
            if (!playerStatsObject.offerings[i].unlocked)
                return;
        }

        UnlockEndEvent.Invoke();
    }
}
