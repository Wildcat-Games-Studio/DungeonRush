using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox: MonoBehaviour
{
    public delegate void OnHit(int damage);

    public OnHit onHit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Hitbox hitbox = collision.gameObject.GetComponent<Hitbox>();
        onHit?.Invoke(hitbox.damage);
    }
}
