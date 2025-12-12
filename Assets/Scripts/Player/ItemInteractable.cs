using UnityEngine;
using UnityEngine.Events;

public class ItemInteractable : MonoBehaviour, ItemInteractor.IInteractable
{
    [Header("Prompt")]
    [SerializeField] string prompt = "Press F";

    [Header("Events")]
    [SerializeField] UnityEvent onInteract;

    public void Interact(ItemInteractor interactor)
    {
        onInteract.Invoke();
    }

    public string GetInteractPrompt()
    {
        return prompt;
    }
}
