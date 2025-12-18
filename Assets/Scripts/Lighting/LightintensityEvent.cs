using UnityEngine;
using System.Collections;

public class LightIntensityEvent : MonoBehaviour
{
    [System.Serializable]
    public struct IntensityPoint
    {
        public float Intensity;
        public Color Color;
        public float transitionDuration;
        public float delay;
    }

    [SerializeField, InspectorName("Intensity Points")]
    public IntensityPoint[] intensityPoints;

    Light l;
    Coroutine seq;

    void Awake()
    {
        l = GetComponent<Light>();
    }

    public void StartLightingEvent()
    {
        if (seq != null) StopCoroutine(seq);
        seq = StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        for (int i = 0; i < intensityPoints.Length; i++)
        {
            var p = intensityPoints[i];
            float d = p.transitionDuration;
            float t = 0f;

            float startI = l.intensity;
            Color startC = l.color;

            if (d <= 0f)
            {
                l.intensity = p.Intensity;
                l.color = p.Color;
            }
            else
            {
                while (t < d)
                {
                    t += Time.deltaTime;
                    float a = Mathf.Clamp01(t / d);
                    l.intensity = Mathf.Lerp(startI, p.Intensity, a);
                    l.color = Color.Lerp(startC, p.Color, a);
                    yield return null;
                }

                l.intensity = p.Intensity;
                l.color = p.Color;
            }

            if (p.delay > 0f) yield return new WaitForSeconds(p.delay);
        }

        seq = null;
    }
}
