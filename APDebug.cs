using UnityEngine;

namespace MessengerRando;

internal static class APDebug
{
    public static void DrawCollider(GameObject parent, BoxCollider2D collider = null)
    {
#if DEBUG
        DrawCollider_(parent, collider);
#endif
    }

    private static void DrawCollider_(GameObject parent, BoxCollider2D collider)
    {
        if (collider == null)
            collider = parent.GetComponent<BoxCollider2D>();
        if (collider == null)
            return;

        var drawer = parent.AddComponent<DebugColliderDrawer>();
        drawer.transform.SetParent(parent.transform, false);
        drawer.Initialize(collider);
    }

    private class DebugColliderDrawer : MonoBehaviour
    {
        private const string OutlineObjectName = "NecroTriggerDebugOutline";

        private BoxCollider2D boxCollider;
        private LineRenderer lineRenderer;

        public void Initialize(BoxCollider2D collider)
        {
            boxCollider = collider;
            lineRenderer = gameObject.AddComponent<LineRenderer>();

            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = 4;
            lineRenderer.startWidth = 0.08f;
            lineRenderer.endWidth = 0.08f;
            lineRenderer.startColor = Color.magenta;
            lineRenderer.endColor = Color.magenta;
            lineRenderer.sortingOrder = 1000;

            UpdateOutline();
        }

        private void LateUpdate()
        {
            UpdateOutline();
        }

        private void UpdateOutline()
        {
            Vector2 halfSize = boxCollider.size * 0.5f;
            Vector2 center = boxCollider.offset;

            lineRenderer.SetPosition(0, new Vector3(center.x - halfSize.x, center.y - halfSize.y, 0f));
            lineRenderer.SetPosition(1, new Vector3(center.x - halfSize.x, center.y + halfSize.y, 0f));
            lineRenderer.SetPosition(2, new Vector3(center.x + halfSize.x, center.y + halfSize.y, 0f));
            lineRenderer.SetPosition(3, new Vector3(center.x + halfSize.x, center.y - halfSize.y, 0f));
        }
    }
}
