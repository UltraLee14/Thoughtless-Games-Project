using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineBetweenPoints : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
    }

    void Update()
    {
        if (startPoint != null && endPoint != null)
        {
            line.SetPosition(0, startPoint.position);
            line.SetPosition(1, endPoint.position);
        }
    }
}
