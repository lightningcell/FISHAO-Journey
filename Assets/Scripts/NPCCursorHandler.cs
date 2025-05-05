using UnityEngine;

public class NPCCursorHandler : MonoBehaviour
{
    [Tooltip("Cursor texture to show when hovering over NPC.")]
    public Texture2D pointerCursor;
    private Texture2D defaultCursor;
    private Vector2 hotspot = Vector2.zero;

    void Awake()
    {
        // Store default cursor (null reverts to system default)
        defaultCursor = null;
    }

    void OnMouseEnter()
    {
        if (pointerCursor != null)
            Cursor.SetCursor(pointerCursor, hotspot, CursorMode.Auto);
    }

    void OnMouseExit()
    {
        Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
    }
}