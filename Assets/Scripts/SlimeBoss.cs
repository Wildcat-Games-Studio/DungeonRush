﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBoss : MonoBehaviour
{
    public Hitbox bounceBox;
    public GameObject hitBoxPivot;
    private Hitbox hitBox;

    [Header("Rendering")]

    public Animator spriteAnimator;
    public SpriteRenderer spriteRenderer;

    public AnimationEventDispatch animDispatch;

    public float colorChangeSpeed;
    public Color followColor;
    public Color attackColor;
    public Color rechargeColor;

    private Color _currentEdgeColor;
    private Color _targEdgeColor;

    [Header("Follow State")]

    [SerializeField]
    private float movementSpeed = 1f;
    [SerializeField]
    private float movementAccel = 1f;
    [SerializeField]
    private float minMoveTime = 2f;

    private float moveTime = 0f;

    [Header("Attack State")]
    [SerializeField]
    private float attackSpeed = 20f;
    [SerializeField]
    private float attackAccel = 20f;
    [SerializeField]
    private float maxDistance = 5f, minDistance = 2f;      // in attacking range
    [SerializeField]
    private float attackChargeTime = .5f;
    [SerializeField]
    private float attackDuration = .8f; // -attacktime, increase for longer attack mode

    private float attacktime = 0.0f;

    [Header("Recharge State")]
    [SerializeField]
    private float chargingTime = 0.0f;
    [SerializeField]
    private float chargingDuration = 2;

    [SerializeField]
    private float friction = 1f;


    [Header("Boundaries")]

    [SerializeField]
    private float minX = 0.0f;
    [SerializeField]
    private float maxX = 0.0f;
    [SerializeField]
    private float minY = 0.0f;
    [SerializeField]
    private float maxY = 0.0f;
    [SerializeField]
    private bool boundaries = true;

    [Header("Hearts")]
    public GameObject heartPrefab;
    public float heartDistance = .6f;  // space between the hearts
    public int intHearts = 3;      // change to add more health
    private GameObject[] heartTargets;


    private Vector2 velocity;
    private Vector2 toPlayer;
    private enum State { Follow, Attack, Recharge };
    private State state = State.Follow;

    // Start is called before the first frame update
    void Start()
    {
        OrderBounds();

        animDispatch.animationEvents.Add(StartFollow);

        heartTargets = new GameObject[intHearts];
        for (int i = 0; i < intHearts; i++)
        {
            heartTargets[i] = new GameObject();
            heartTargets[i].transform.SetParent(transform);
            heartTargets[i].transform.position = new Vector2(transform.position.x + (i * heartDistance) - heartDistance * ((intHearts % 2 == 0) ? (intHearts / 2) - .5f : (intHearts / 2)), transform.position.y + .7f);

            GameObject heart = Instantiate(heartPrefab);
            Follow follow = heart.GetComponent<Follow>();
            follow.target = heartTargets[i].transform;
        }

        hitBox = hitBoxPivot.GetComponentInChildren<Hitbox>();

        bounceBox.collidedWith = BounceCollide;
        hitBox.collidedWith = DamageCollide;

        _currentEdgeColor = followColor;
        StartFollow();
    }

    void BounceCollide(Collider2D collider, Vector2 normal)
    {
        Rigidbody2D rb = collider.attachedRigidbody;

        if (rb != null)
        {
            Vector2 from_velo = rb.velocity - velocity;

            float normalVelo = Vector2.Dot(from_velo, normal);

            if (normalVelo > 0)
                return;

            rb.velocity -= 10.0f * normalVelo * normal;
        }
    }

    void DamageCollide(Collider2D collider, Vector2 normal)
    {
        Rigidbody2D rb = collider.attachedRigidbody;

        if (rb != null)
        {
            EntityStats stats = rb.GetComponent<EntityStats>();

            if(stats != null)
            {
                Vector3 dir = Vector3.Cross(velocity.normalized, Vector3.forward);
                rb.velocity += 10.0f * (Vector2)dir;

                stats.Damage(10);
            }
        }
    }

    void StartFollow()
    {
        spriteAnimator.ResetTrigger("move");
        state = State.Follow;
        moveTime = 0.0f;

        _targEdgeColor = followColor;
    }
    void StartAttacK()
    {
        state = State.Attack;
        spriteAnimator.SetTrigger("dash");
        attacktime = 0.0f;

        _targEdgeColor = attackColor;
    }

    void StartRecharge()
    {
        state = State.Recharge;
        spriteAnimator.SetTrigger("recharge");
        chargingTime = 0.0f;

        _targEdgeColor = rechargeColor;
    }

    void UpdateMovement()
    {
        transform.Translate(velocity * Time.fixedDeltaTime);

        if (!Mathf.Approximately(velocity.x, 0.0f))
        {
            bool flip = velocity.x < 0;
            hitBoxPivot.transform.rotation = Quaternion.AngleAxis(180.0f * (flip ? 1.0f : 0.0f), Vector3.forward);
            spriteRenderer.flipX = flip;
        }
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Follow:

                moveTime += Time.deltaTime;

                toPlayer = GameState.Instance.player.transform.position - gameObject.transform.position;
                float sqrMag = Vector2.SqrMagnitude(toPlayer);
                toPlayer.Normalize();
                if (moveTime > minMoveTime && sqrMag < maxDistance * maxDistance && sqrMag > minDistance * minDistance)
                {
                    StartAttacK();
                    break;
                }

                velocity = Vector2.Lerp(velocity, toPlayer * movementSpeed, movementAccel * Time.fixedDeltaTime);

                break;
            case State.Attack:

                attacktime += Time.deltaTime;

                if (attacktime > attackChargeTime)
                {
                    Vector2 targ_velo = toPlayer * attackSpeed;
                    Vector2 velo_delta = targ_velo - velocity;

                    float velo_len = velo_delta.magnitude;
                    Vector2 to_targ = velo_delta.normalized;

                    float accel = attackAccel * attackSpeed * Time.fixedDeltaTime;

                    if (accel > velo_len) accel = velo_len;

                    velocity += accel * to_targ;
                }

                if (attacktime > attackDuration)
                {
                    StartRecharge();
                    break;
                }
                break;
            case State.Recharge:

                velocity = Vector2.Lerp(velocity, Vector2.zero, friction * Time.fixedDeltaTime);

                if (chargingTime > chargingDuration)
                {
                    spriteAnimator.ResetTrigger("recharge");
                    spriteAnimator.SetTrigger("move");
                    break;
                }
                chargingTime += Time.deltaTime;
                break;

        }

        UpdateMovement();
        if (boundaries)
        {
            Vector2 bounceNormal = Vector2.zero;

            if (transform.position.x < minX) { bounceNormal = Vector2.right; }
            else if (transform.position.x > maxX) { bounceNormal = Vector2.left; }
            else if (transform.position.y < minY) { bounceNormal = Vector2.up; }
            else if (transform.position.y > maxY) { bounceNormal = Vector2.down; }

            if (bounceNormal != Vector2.zero)
            {
                velocity = velocity.magnitude * 0.9f * bounceNormal;

                if (velocity.magnitude > attackSpeed * 0.5f)
                {
                    StartRecharge();
                }
            }
        }

        _currentEdgeColor = Color.Lerp(_currentEdgeColor, _targEdgeColor, Time.fixedDeltaTime * colorChangeSpeed);
        spriteRenderer.sharedMaterial.SetColor("_EdgeCol", _currentEdgeColor);
    }

    //public void takeDamage()
    //{
    //    Destroy(hearts[--intHearts]);
    //    if (intHearts == 0)
    //    {
    //        // player won
    //    }

    //}

    private void OrderBounds()
    {
        float t;
        if (minX > maxX)
        {
            t = minX;
            minX = maxX;
            maxX = t;
        }
        if (minY > maxY)
        {
            t = minY;
            minY = maxY;
            maxY = t;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(gameObject.transform.position, minDistance);
        Gizmos.DrawWireSphere(gameObject.transform.position, maxDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3((maxX + minX) * 0.5f, (maxY + minY) * 0.5f, 0.0f), new Vector3(maxX - minX, maxY - minY, 1.0f));
    }
}
