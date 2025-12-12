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

    Coroutine currentFade;

    public void FadeOut()
    {
        if (targetImage == null) return;
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(1f, FadeoutDuration, FadeOutEvent));
    }

    public void FadeIn()
    {
        if (targetImage == null) return;
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(0f, FadeInDuration, FadeInEvent));
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


