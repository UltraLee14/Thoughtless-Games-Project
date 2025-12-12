using UnityEngine;
using TMPro;

public class VelocityTracker : MonoBehaviour
{
    public Rigidbody targetRigidbody;
    public TMP_Text velocityText;

    void Update()
    {
        if (targetRigidbody == null || velocityText == null)
            return;

        Vector3 localVelocity = targetRigidbody.transform.InverseTransformDirection(targetRigidbody.linearVelocity);
        velocityText.text = $"X: {localVelocity.x:F2}  Y: {localVelocity.y:F2}  Z: {localVelocity.z:F2}";
    }
}
