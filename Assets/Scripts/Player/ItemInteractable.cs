using UnityEngine;
using UnityEngine.Events;

public class ItemInteractable : MonoBehaviour, ItemInteractor.IInteractable
{
    [Header("Prompt")]
    [SerializeField] string prompt = "Press F";

    [Header("Events")]
    [SerializeField] UnityEvent onInteract;

    [Header("Loot Settings")]
    [SerializeField, InspectorName("Player Stats Object")]
    PlayerStats playerStatsObject;

    [SerializeField, InspectorName("Is Loot")]
    bool isLoot;

    [SerializeField, InspectorName("Loot Value")]
    int lootValue;

    public void Interact(ItemInteractor interactor)
    {
        if (isLoot && playerStatsObject != null)
            playerStatsObject.pendingGoldBalance += lootValue;

        onInteract.Invoke();
    }

    public string GetInteractPrompt()
    {
        return prompt;
    }
}
