using System.Collections.Generic;
using UnityEngine;
using Ilumisoft.RadarSystem;

public class RadarStarter : MonoBehaviour
{
    [System.Serializable]
    public struct RadarObjectElement
    {
        [InspectorName("Radar Object")]
        public GameObject radarObject;

        [InspectorName("Locatable")]
        public Locatable locatable;
    }

    [Header("References")]
    [SerializeField, InspectorName("Blessing Data")]
    BlessingData blessingData;

    [Header("Radar Objects")]
    [SerializeField, InspectorName("Radar Objects")]
    RadarObjectElement[] radarObjects;

    void Start()
    {
        for (int i = 0; i < radarObjects.Length; i++)
            radarObjects[i].locatable.enabled = false;

        if (blessingData == null) return;

        int count = Mathf.Min(radarObjects.Length, blessingData.blessings.Length);
        if (count <= 0) return;

        List<int> eligible = new List<int>(count);
        for (int i = 0; i < count; i++)
        {
            if (blessingData.blessings[i].unlocked == false)
                eligible.Add(i);
        }

        if (eligible.Count == 0) return;

        int pickedIndex = eligible[Random.Range(0, eligible.Count)];
        radarObjects[pickedIndex].locatable.enabled = true;
    }
}
