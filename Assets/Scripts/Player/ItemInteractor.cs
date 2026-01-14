using UnityEngine;
using UnityEngine.Events;

public class ItemInteractor : MonoBehaviour
{
    public interface IInteractable
    {
        void Interact(ItemInteractor interactor);
        string GetInteractPrompt();
    }

    [Header("References")]
    [SerializeField] Camera playerCamera;

    [Header("Settings")]
    [SerializeField] float interactRange = 3f;
    [SerializeField] LayerMask interactMask = ~0;
    [SerializeField] KeyCode interactKey = KeyCode.F;
    [SerializeField] bool includeTriggers = false;

    [Header("Interact Cone")]
    [SerializeField, InspectorName("Cone Angle (deg)")]
    float coneAngle = 20f;

    [SerializeField, InspectorName("Overlap Buffer Size")]
    int overlapBufferSize = 64;

    [Header("Hover Events")]
    [SerializeField] UnityEvent OnHoverInteractable;
    [SerializeField] UnityEvent OnStopHoverInteractable;

    [Header("Read Only")]
    [SerializeField] string currentPrompt;
    [SerializeField] GameObject currentTarget;

    IInteractable currentInteractable;
    RaycastHit hit;
    bool wasHovering;

    Collider[] overlapBuffer;

    void Reset()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Awake()
    {
        int size = Mathf.Max(1, overlapBufferSize);
        overlapBuffer = new Collider[size];
    }

    void OnValidate()
    {
        overlapBufferSize = Mathf.Max(1, overlapBufferSize);

        if (overlapBuffer == null || overlapBuffer.Length != overlapBufferSize)
            overlapBuffer = new Collider[overlapBufferSize];

        if (coneAngle < 0f) coneAngle = 0f;
        if (coneAngle > 179.9f) coneAngle = 179.9f;
        if (interactRange < 0f) interactRange = 0f;
    }

    void Update()
    {
        Scan();
        HandleHoverEvents();
        TryInteract();
    }

    void Scan()
    {
        currentInteractable = null;
        currentTarget = null;
        currentPrompt = "";

        if (playerCamera == null) return;

        Vector3 origin = playerCamera.transform.position;
        Vector3 forward = playerCamera.transform.forward;

        var triggerMode = includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        float range = Mathf.Max(0f, interactRange);
        float halfAngle = coneAngle * 0.5f;

        float bestDist = float.PositiveInfinity;
        Collider bestCol = null;
        RaycastHit bestHit = default;

        int count = Physics.OverlapSphereNonAlloc(origin, range, overlapBuffer, interactMask, triggerMode);

        for (int i = 0; i < count; i++)
        {
            Collider col = overlapBuffer[i];
            if (col == null) continue;

            Vector3 closest = col.ClosestPoint(origin);
            Vector3 to = closest - origin;

            float dist = to.magnitude;
            if (dist <= 0f || dist > range) continue;

            float angle = Vector3.Angle(forward, to);
            if (angle > halfAngle) continue;

            Vector3 dir = to / dist;

            if (!Physics.Raycast(origin, dir, out RaycastHit h, dist + 0.01f, interactMask, triggerMode))
                continue;

            if (h.collider != col && !h.collider.transform.IsChildOf(col.transform) && !col.transform.IsChildOf(h.collider.transform))
                continue;

            var interactable = col.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            if (dist < bestDist)
            {
                bestDist = dist;
                bestCol = col;
                bestHit = h;
            }
        }

        if (bestCol != null)
        {
            hit = bestHit;
            currentInteractable = bestCol.GetComponentInParent<IInteractable>();

            if (currentInteractable != null)
            {
                currentTarget = bestHit.collider.gameObject;
                currentPrompt = currentInteractable.GetInteractPrompt();
            }
        }
    }

    void HandleHoverEvents()
    {
        bool hovering = currentInteractable != null;

        if (hovering && !wasHovering)
            OnHoverInteractable.Invoke();

        if (!hovering && wasHovering)
            OnStopHoverInteractable.Invoke();

        wasHovering = hovering;
    }

    void TryInteract()
    {
        if (currentInteractable == null) return;

        if (Input.GetKeyDown(interactKey))
        {
            currentInteractable.Interact(this);
        }
    }

    public bool HasTarget()
    {
        return currentInteractable != null;
    }

    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }

    public string GetCurrentPrompt()
    {
        return currentPrompt;
    }

    void OnDrawGizmosSelected()
    {
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null) return;

        Vector3 origin = cam.transform.position;
        Vector3 forward = cam.transform.forward;

        float range = Mathf.Max(0f, interactRange);

        Gizmos.color = new Color(0f, 1f, 0.35f, 0.85f);

        if (coneAngle <= 0f)
        {
            Gizmos.DrawLine(origin, origin + forward * range);
            Gizmos.DrawWireSphere(origin + forward * range, 0.05f);
            return;
        }

        float halfAngleRad = Mathf.Deg2Rad * (coneAngle * 0.5f);
        float baseRadius = Mathf.Tan(halfAngleRad) * range;

        Vector3 endCenter = origin + forward * range;

        int segments = 28;
        Vector3 right = cam.transform.right;
        Vector3 up = cam.transform.up;

        Vector3 prev = endCenter + (right * baseRadius);
        Gizmos.DrawLine(origin, prev);

        for (int i = 1; i <= segments; i++)
        {
            float t = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 p = endCenter + (right * Mathf.Cos(t) + up * Mathf.Sin(t)) * baseRadius;

            Gizmos.DrawLine(prev, p);
            Gizmos.DrawLine(origin, p);

            prev = p;
        }

        Gizmos.DrawWireSphere(endCenter, 0.03f);
    }
}
