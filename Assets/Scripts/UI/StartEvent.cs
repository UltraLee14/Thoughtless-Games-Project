using UnityEngine;
using UnityEngine.Events;

public class StartEvent : MonoBehaviour
{
    [SerializeField]
    public UnityEvent StartUpEvent = new UnityEvent();

    void Start()
    {
        StartUpEvent.Invoke();
    }
}
