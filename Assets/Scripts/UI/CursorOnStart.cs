using UnityEngine;

public class CursorOnStart : MonoBehaviour
{
    [SerializeField, InspectorName("Visable")]
    bool visable = true;

    void Start()
    {
        Cursor.visible = visable;
    }
}
