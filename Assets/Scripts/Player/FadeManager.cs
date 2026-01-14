using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    [SerializeField]
    Image targetImage;

    [SerializeField]
    float FadeInDuration;

    [SerializeField]
    float FadeoutDuration;

    [SerializeField]
    UnityEvent FadeOutEvent;

    [SerializeField]
    UnityEvent FadeInEvent;

    [SerializeField, InspectorName("Fade In Target Alpha (0-255)")]
    int FadeInTargetAlpha = 0;

    [SerializeField, InspectorName("Fade Out Target Alpha (0-255)")]
    int FadeOutTargetAlpha = 255;

    Coroutine currentFade;

    void OnValidate()
    {
        if (FadeInTargetAlpha < 0) FadeInTargetAlpha = 0;
        if (FadeInTargetAlpha > 255) FadeInTargetAlpha = 255;

        if (FadeOutTargetAlpha < 0) FadeOutTargetAlpha = 0;
        if (FadeOutTargetAlpha > 255) FadeOutTargetAlpha = 255;
    }

    public void FadeOut()
    {
        if (targetImage == null) return;
        if (currentFade != null) StopCoroutine(currentFade);

        float targetAlpha = FadeOutTargetAlpha / 255f;
        currentFade = StartCoroutine(FadeRoutine(targetAlpha, FadeoutDuration, FadeOutEvent));
    }

    public void FadeIn()
    {
        if (targetImage == null) return;
        if (currentFade != null) StopCoroutine(currentFade);

        float targetAlpha = FadeInTargetAlpha / 255f;
        currentFade = StartCoroutine(FadeRoutine(targetAlpha, FadeInDuration, FadeInEvent));
    }

    IEnumerator FadeRoutine(float targetAlpha, float duration, UnityEvent completeEvent)
    {
        if (duration <= 0f)
        {
            Color instantColor = targetImage.color;
            instantColor.a = targetAlpha;
            targetImage.color = instantColor;
            if (completeEvent != null) completeEvent.Invoke();
            currentFade = null;
            yield break;
        }

        float startAlpha = targetImage.color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            Color c = targetImage.color;
            c.a = alpha;
            targetImage.color = c;
            yield return null;
        }

        Color finalColor = targetImage.color;
        finalColor.a = targetAlpha;
        targetImage.color = finalColor;

        if (completeEvent != null) completeEvent.Invoke();
        currentFade = null;
    }
}
