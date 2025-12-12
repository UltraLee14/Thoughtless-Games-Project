using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField, InspectorName("Collision Filter Tag")]
    public List<string> collisionFilterTag = new List<string>();

    [SerializeField, InspectorName("Begin Overlap Event")]
    UnityEvent BeginOverlapEvent;

    [SerializeField, InspectorName("End Overlap Event")]
    UnityEvent EndOverlapEvent;

    void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < collisionFilterTag.Count; i++)
        {
            string tagName = collisionFilterTag[i];
            if (!string.IsNullOrEmpty(tagName) && other.CompareTag(tagName))
            {
                BeginOverlapEvent.Invoke();
                return;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        for (int i = 0; i < collisionFilterTag.Count; i++)
        {
            string tagName = collisionFilterTag[i];
            if (!string.IsNullOrEmpty(tagName) && other.CompareTag(tagName))
            {
                EndOverlapEvent.Invoke();
                return;
            }
        }
    }
}
