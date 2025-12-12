using UnityEngine;
using System.Collections;

public class SetCameraFOVToPoint : MonoBehaviour
{
    [System.Serializable]
    public class Point
    {
        public float fieldOfView = 60f;
        public bool useFOV = true;
        public float fovDuration = 0f;
        public bool fovLinear = true;
    }

    public Camera targetCamera;
    public Point[] points;

    private Coroutine moveRoutine;
    private int currentIndex = -1;

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();
    }

    private void Start()
    {
        SetToPoint(0);
    }

    public void SetToPoint(int index)
    {
        if (points == null || index < 0 || index >= points.Length) return;
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        currentIndex = index;
        moveRoutine = StartCoroutine(ApplyPoint(points[index]));
    }

    public void NextPoint()
    {
        if (points == null || points.Length == 0) return;
        int next = (currentIndex + 1) % points.Length;
        SetToPoint(next);
    }

    private IEnumerator ApplyPoint(Point p)
    {
        float startFOV = targetCamera.fieldOfView;
        float targetFOV = p.useFOV ? p.fieldOfView : startFOV;

        bool anyDuration = p.useFOV && p.fovDuration > 0f;

        if (!anyDuration)
        {
            targetCamera.fieldOfView = targetFOV;
            moveRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (true)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / p.fovDuration);
            t = p.fovLinear ? t : EaseCubic(t);

            float newFOV = Mathf.Lerp(startFOV, targetFOV, t);
            targetCamera.fieldOfView = newFOV;

            if (t >= 1f) break;
            yield return null;
        }

        moveRoutine = null;
    }

    private float EaseCubic(float t)
    {
        return t * t * (3f - 2f * t);
    }
}
