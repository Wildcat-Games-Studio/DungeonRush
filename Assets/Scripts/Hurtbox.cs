using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public delegate void OnHit(int damage);
    public OnHit onHit;
    public float iTime;

    private float _nextHitTime = 0.0f;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Time.time >= _nextHitTime)
        {
            _nextHitTime = Time.time + iTime;

            Hitbox hitbox = collision.gameObject.GetComponent<Hitbox>();
            onHit?.Invoke(hitbox.damage);
        }
    }
}
