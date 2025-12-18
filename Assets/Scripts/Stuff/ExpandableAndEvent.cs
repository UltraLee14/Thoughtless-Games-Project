using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExpandableAndEvent : MonoBehaviour
{
    [System.Serializable]
    public class Condition
    {
        [SerializeField, InspectorName("Condtition Bool")]
        public bool conditionBool;
    }

    [SerializeField, InspectorName("Conditions")]
    public List<Condition> Conditions = new List<Condition>();

    [SerializeField, InspectorName("Conditions Met Event")]
    public UnityEvent ConditionsMetEvent = new UnityEvent();

    [SerializeField, InspectorName("Conditions Not Met Event")]
    public UnityEvent ConditionsNotMetEvent = new UnityEvent();

    public void SetElementBoolTrue(int index)
    {
        if (index < 0 || index >= Conditions.Count) return;
        if (Conditions[index] == null) return;

        Conditions[index].conditionBool = true;
        AttemptEvent();
    }

    public void SetElementBoolFalse(int index)
    {
        if (index < 0 || index >= Conditions.Count) return;
        if (Conditions[index] == null) return;

        Conditions[index].conditionBool = false;
    }

    public void AttemptEvent()
    {
        bool allTrue = true;

        for (int i = 0; i < Conditions.Count; i++)
        {
            if (Conditions[i] == null || !Conditions[i].conditionBool)
            {
                allTrue = false;
                break;
            }
        }

        if (allTrue) ConditionsMetEvent.Invoke();
        else ConditionsNotMetEvent.Invoke();
    }
}
