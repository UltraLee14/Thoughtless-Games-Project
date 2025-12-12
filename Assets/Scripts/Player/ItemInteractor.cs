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

    [Header("Hover Events")]
    [SerializeField] UnityEvent OnHoverInteractable;
    [SerializeField] UnityEvent OnStopHoverInteractable;

    [Header("Read Only")]
    [SerializeField] string currentPrompt;
    [SerializeField] GameObject currentTarget;

    IInteractable currentInteractable;
    RaycastHit hit;
    bool wasHovering;

    void Reset()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
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
        Vector3 direction = playerCamera.transform.forward;

        var triggerMode = includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        if (Physics.Raycast(origin, direction, out hit, interactRange, interactMask, triggerMode))
        {
            currentInteractable = hit.collider.GetComponentInParent<IInteractable>();

            if (currentInteractable != null)
            {
                currentTarget = hit.collider.gameObject;
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
}
