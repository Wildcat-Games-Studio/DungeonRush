using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBoss : MonoBehaviour
{
    public Animator spriteAnimator;
    public SpriteRenderer spriteRenderer;

    [SerializeField]
    private float movementSpeed = 1f;
    [SerializeField]
    private float movementAccel = 1f;

    [SerializeField]
    private float friction = 1f;

    [SerializeField]
    private float attackSpeed = 20f;
    [SerializeField]
    private float attackAccel = 20f;

    [SerializeField]
    private float maxDistance = 5f, minDistance = 2f;      // in attacking range

    [SerializeField]
    private Vector2 velocity;

    [SerializeField]
    private float moveTime = 0f;
    [SerializeField]
    private float minMoveTime = 2f;


    public enum State { Follow, Attack, Recharge };
    public State state = State.Follow;


    [SerializeField]
    private float attacktime = 0;
    [SerializeField]
    private float attackChargeTime = .5f;
    [SerializeField]
    private float attackDuration = .8f; // -attacktime, increase for longer attack mode

    [SerializeField]
    private float chargingTime;
    [SerializeField]
    private float chargingDuration = 2;

    private Vector2 toPlayer;

    //private Transform playerTransform;
    //private float playerX;
    //private float playerY;
    //private float attackSpeedX, attackSpeedY;
    //private float bossX, bossY;
    //private int directionX, directionY;

    [SerializeField]
    private float minX = 1, minY = .5f, maxX = 19, maxY = 9.5f;    // controls boundries 
    private bool boundaries = true;

    public Sprite heartSprite;
    private float heartDistance = .6f;  // space between the hearts
    private int intHearts = 3;      // change to add more health
    private GameObject[] hearts;

    private bool canDamagePlayer;

    // Start is called before the first frame update
    void Start()
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

        hearts = new GameObject[intHearts];
        for (int i = 0; i < intHearts; i++)
        {
            GameObject heart = new GameObject();
            heart.AddComponent<SpriteRenderer>();
            heart.GetComponent<SpriteRenderer>().sprite = heartSprite;
            heart.transform.SetParent(gameObject.transform);
            heart.transform.position = new Vector2(transform.position.x + (i * heartDistance) - heartDistance * ((intHearts % 2 == 0) ? (intHearts / 2) - .5f : (intHearts / 2)), transform.position.y + .7f);
            hearts[i] = heart;
        }
        //takeDamage();
    }


    void StartFollow()
    {
        spriteAnimator.SetTrigger("move");
        state = State.Follow;
        moveTime = 0.0f;
    }
    void StartAttacK()
    {
        state = State.Attack;
        spriteAnimator.SetTrigger("dash");
        attacktime = 0.0f;
    }

    void StartRecharge()
    {
        state = State.Recharge;
        spriteAnimator.SetTrigger("recharge");
        chargingTime = 0.0f;
    }

    void UpdateMovement()
    {
        transform.Translate(velocity * Time.fixedDeltaTime);

        if (!Mathf.Approximately(velocity.x, 0.0f))
        {
            spriteRenderer.flipX = velocity.x < 0;
        }
    }

    void FixedUpdate()
    {
        //bossX = transform.position.x;
        //bossY = transform.position.y;

        switch (state)
        {
            case State.Follow:

                moveTime += Time.deltaTime;
                //move(speed,speed, new Vector2(playerTransform.position.x,playerTransform.position.y));

                //playerX = Math.Abs(gameObject.transform.position.x - playerTransform.position.x);
                //playerY = Math.Abs(gameObject.transform.position.y - playerTransform.position.y);
                //if (moveTime > setMoveTime && ((playerX < maxDistance && playerY < maxDistance) && (playerX > minDistance || playerY > minDistance)))
                //{
                //    directionX = bossX > playerTransform.position.x ? -1 : 1;
                //    directionY = bossY > playerTransform.position.y ? -1 : 1;

                //    StartAttacK();
                //}

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
                    //// Finds slope?
                    //if(playerX > playerY)
                    //{
                    //    attackSpeedX = playerX / playerX;
                    //    attackSpeedY = playerY / playerX;
                    //}
                    //else
                    //{
                    //    attackSpeedX = playerX / playerY;
                    //    attackSpeedY = playerY / playerY;
                    //}

                    //move(attackSpeedX * attackSpeed, attackSpeedY * attackSpeed, new Vector2(bossX + playerX * directionX , bossY + playerY * directionY));

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
                    StartFollow();
                    break;
                }
                chargingTime += Time.deltaTime;
                break;

        }

        UpdateMovement();
        if (boundaries)
        {
            //if(bossX < minX || bossX > maxX || bossY < minY || bossY > maxY)
            //{
            //    StartRecharge();
            //}
            Vector2 bounceNormal = Vector2.zero;

            if (transform.position.x < minX) { bounceNormal = Vector2.right; }
            else if (transform.position.x > maxX) { bounceNormal = Vector2.left; }
            else if (transform.position.y < minY) { bounceNormal = Vector2.up; }
            else if (transform.position.y > maxY) { bounceNormal = Vector2.down; }

            if (bounceNormal != Vector2.zero)
            {
                velocity = velocity.magnitude * 0.9f * bounceNormal;

                if(velocity.magnitude > attackSpeed * 0.5f) StartRecharge();
            }
        }
    }
    //public void move(float speedX, float speedY, Vector2 destination) // moves the boss to a location
    //{
    //    if (destination.x > gameObject.transform.position.x)
    //    {
    //        gameObject.transform.position = new Vector2(gameObject.transform.position.x + speedX * Time.deltaTime, gameObject.transform.position.y);
    //    }
    //    else if (destination.x < gameObject.transform.position.x)
    //    {
    //        gameObject.transform.position = new Vector2(gameObject.transform.position.x - speedX * Time.deltaTime, gameObject.transform.position.y);
    //    }
    //    if (destination.y > gameObject.transform.position.y)
    //    {
    //        gameObject.transform.position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y + speedY * Time.deltaTime);
    //    }
    //    else if (destination.y < gameObject.transform.position.y)
    //    {
    //        gameObject.transform.position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - speedY * Time.deltaTime);
    //    }
    //}
    public void takeDamage()
    {
        Destroy(hearts[--intHearts]);
        if (intHearts == 0)
        {
            // player won
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
