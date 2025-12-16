using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFadeToColor : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField, InspectorName("Fade Speed (Seconds)")]
    float fadeSpeed = 1f;

    [SerializeField, InspectorName("Target Color")]
    Color targetColor = Color.white;

    Light targetLight;
    Coroutine fadeRoutine;

    void Awake()
    {
        targetLight = GetComponent<Light>();
    }

    public void FadeToColor()
    {
        if (targetLight == null) targetLight = GetComponent<Light>();

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(targetColor));
    }

    IEnumerator FadeRoutine(Color goal)
    {
        Color start = targetLight.color;

        float duration = Mathf.Max(0.0001f, fadeSpeed);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            targetLight.color = Color.Lerp(start, goal, Mathf.Clamp01(t));
            yield return null;
        }

        targetLight.color = goal;
        fadeRoutine = null;
    }

    public void SetTargetColor(Color newTargetColor)
    {
        targetColor = newTargetColor;
    }
}
