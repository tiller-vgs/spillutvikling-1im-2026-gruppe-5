using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutlineCollider : MonoBehaviour
{
    private void OnEnable()
    {
        EnsureCollider();
    }

    private void OnValidate()
    {
        EnsureCollider();
    }

    private void EnsureCollider()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return;
        }

        PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
        if (polygonCollider == null)
        {
            polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
        }

        polygonCollider.isTrigger = false;
        polygonCollider.usedByEffector = false;
    }
}
