using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public delegate void CollidedWith(Collider2D collider, Vector2 normal);
    public CollidedWith collidedWith;

    public LayerMask mask;
    public Color color;

    public enum HitboxType { box, sphere };
    public HitboxType type;

    private void FixedUpdate()
    {
        Collider2D collider;

        switch (type)
        {
            case HitboxType.box:
                collider = Physics2D.OverlapBox(transform.position, transform.localScale / 2.0f, 0.0f, mask);
                break;
            case HitboxType.sphere:
            default:
                collider = Physics2D.OverlapCircle(transform.position, transform.localScale.x / 2.0f, mask);
                break;
        }

        if (collider)
        {
            collidedWith?.Invoke(collider, (collider.transform.position - transform.position).normalized);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        switch (type)
        {
            case HitboxType.box:
                Gizmos.DrawCube(transform.position, transform.localScale);
                break;
            case HitboxType.sphere:
                Gizmos.DrawSphere(transform.position, transform.localScale.x / 2.0f);
                break;
        }

    }
}
