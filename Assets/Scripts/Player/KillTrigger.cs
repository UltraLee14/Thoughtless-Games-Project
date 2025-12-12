using UnityEngine;
using UnityEngine.Events;

public class KillTrigger : MonoBehaviour
{
    public bool Alive = true;

    [Header("References")]
    [SerializeField, InspectorName("Self Collider")]
    public Collider selfCollider;

    [SerializeField, InspectorName("Kill Event")]
    public UnityEvent killEvent;

    public void KillShot()
    {
        if (!Alive) return;

        Alive = false;
        killEvent.Invoke();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!Alive) return;
        if (!collision.gameObject.CompareTag("Enemy")) return;

        KillShot();
    }
}
