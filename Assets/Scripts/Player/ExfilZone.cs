using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ExfilZone : MonoBehaviour
{
    [Header("UI")]
    [SerializeField, InspectorName("Timer Text")]
    TMP_Text timerText;

    [Header("Settings")]
    [SerializeField, InspectorName("Extraction Time")]
    float extractionTime;

    [Header("Collider")]
    [SerializeField, InspectorName("Assigned Collider")]
    Collider assignedCollider;

    [Header("Events")]
    [SerializeField, InspectorName("Start Extract Event")]
    UnityEvent startExtractEvent;

    [SerializeField, InspectorName("Cancel Extract Event")]
    UnityEvent cancelExtractEvent;

    [SerializeField, InspectorName("On Extraction Complete")]
    UnityEvent onExtractionComplete;

    float timeRemaining;
    bool inExfil;
    Collider currentExfil;

    void Awake()
    {
        if (assignedCollider == null)
            assignedCollider = GetComponent<Collider>();

        timeRemaining = extractionTime;
    }

    void Update()
    {
        if (!inExfil) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining < 0f)
            timeRemaining = 0f;

        timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

        if (timeRemaining <= 0f)
        {
            inExfil = false;
            currentExfil = null;
            onExtractionComplete.Invoke();
            timeRemaining = extractionTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Exfil")) return;
        if (!other.isTrigger) return;
        if (!IsAssignedColliderOverlapping(other)) return;

        currentExfil = other;
        inExfil = true;
        timeRemaining = extractionTime;
        startExtractEvent.Invoke();
        timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Exfil")) return;
        if (currentExfil != other) return;

        if (IsAssignedColliderOverlapping(other)) return;

        inExfil = false;
        currentExfil = null;
        timeRemaining = extractionTime;
        cancelExtractEvent.Invoke();
    }

    bool IsAssignedColliderOverlapping(Collider exfilCollider)
    {
        if (assignedCollider == null) return true;

        Vector3 direction;
        float distance;

        return Physics.ComputePenetration(
            assignedCollider, assignedCollider.transform.position, assignedCollider.transform.rotation,
            exfilCollider, exfilCollider.transform.position, exfilCollider.transform.rotation,
            out direction, out distance
        );
    }
}
