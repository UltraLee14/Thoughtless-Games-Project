using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimAllbutOne : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, InspectorName("Handlight Tag")]
    string handlightTag = "handlight";

    [SerializeField, InspectorName("Dim Intensity Multiplier")]
    float dimIntensityMultiplier = 0.1f;

    [Header("Fade Settings")]
    [SerializeField, InspectorName("Fade Target Intensity")]
    float fadeTargetIntensity = 0f;

    [Header("Runtime (Read Only)")]
    [SerializeField, InspectorName("Hand Light")]
    Light handLight;

    [SerializeField, InspectorName("Scene Lights (Dimmed)")]
    List<Light> sceneLights = new List<Light>();

    Coroutine fadeRoutine;

    void Start()
    {
        sceneLights.Clear();
        handLight = null;

        Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

        for (int i = 0; i < allLights.Length; i++)
        {
            Light l = allLights[i];
            if (l == null) continue;

            if (l.gameObject.tag == handlightTag)
            {
                handLight = l;
                continue;
            }

            sceneLights.Add(l);
        }

        float m = Mathf.Max(0f, dimIntensityMultiplier);

        for (int i = 0; i < sceneLights.Count; i++)
        {
            Light l = sceneLights[i];
            if (l == null) continue;

            l.intensity *= m;
        }
    }

    public void FadeAllLights()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeAllLightsRoutine());
    }

    IEnumerator FadeAllLightsRoutine()
    {
        float duration = 5f;

        int count = sceneLights.Count;
        float[] startIntensities = new float[count];

        for (int i = 0; i < count; i++)
        {
            Light l = sceneLights[i];
            startIntensities[i] = l != null ? l.intensity : 0f;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);

            for (int i = 0; i < count; i++)
            {
                Light l = sceneLights[i];
                if (l == null) continue;

                l.intensity = Mathf.Lerp(startIntensities[i], fadeTargetIntensity, a);
            }

            yield return null;
        }

        for (int i = 0; i < count; i++)
        {
            Light l = sceneLights[i];
            if (l == null) continue;

            l.intensity = fadeTargetIntensity;
        }

        fadeRoutine = null;
    }
}
