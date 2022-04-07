using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBoss : MonoBehaviour
{
    public Sprite[] sprites;
    public string playerName = "player";
    private float speed = 1f;
    private float attackSpeed = 20f;
    private float maxDistance = 3f, minDistance = .5f;      // in attacking range
    private float moveTime = 0f;
    private float setMoveTime = 2f;
    public int state = 0;
    public float attacktime = 0;
    private float setAttackTime = .5f;
    private float attackDuration = .7f; // -attacktime
    private float chargingDuration = 2;
    private float chargingTime;
    private Transform playerTransform;
    private int currentImg;
    private float playerX;
    private float playerY;
    private float attackSpeedX, attackSpeedY;
    private float bossX, bossY;
    private int directionX, directionY;
    private float minX =1, minY=.5f, maxX=19, maxY=9.5f;    // controls boundries 
    private bool boundaries = true;

    public Sprite heartSprite;
    private float heartDistance = .6f;  // space between the hearts
    private int intHearts = 3;      // change to add more health
    private GameObject[] hearts;

    private bool canDamagePlayer;

    // Start is called before the first frame update
    void Start()
    {
        hearts = new GameObject[intHearts];
        playerTransform = GameObject.Find(playerName).GetComponent<Transform>();
        for(int i = 0; i < intHearts; i++)
        {
            GameObject heart = new GameObject();
            heart.AddComponent<SpriteRenderer>();
            heart.GetComponent<SpriteRenderer>().sprite = heartSprite;
            heart.transform.SetParent(gameObject.transform);
            heart.transform.position = new Vector2(transform.position.x + (i * heartDistance) - heartDistance * ((intHearts%2==0)?(intHearts/2)-.5f:(intHearts/2)), transform.position.y + .7f);
            hearts[i] = heart;
        }
        //takeDamage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        bossX = transform.position.x;
        bossY = transform.position.y;

        switch (state)
        {
            case 0: // fallow player 
                if (currentImg != 0)
                    gameObject.GetComponent<SpriteRenderer>().sprite = sprites[0];  // sets boss sprite to default
                move(speed,speed, new Vector2(playerTransform.position.x,playerTransform.position.y));
                moveTime += Time.deltaTime;
                playerX = Math.Abs(gameObject.transform.position.x - playerTransform.position.x);
                playerY = Math.Abs(gameObject.transform.position.y - playerTransform.position.y);
                if (moveTime > setMoveTime && playerX < maxDistance && playerY < maxDistance && playerX > minDistance && playerY > minDistance )
                {
                    moveTime = 0;
                    state = 1;
                    directionX = bossX > playerTransform.position.x ? -1 : 1;
                    directionY = bossY > playerTransform.position.y ? -1 : 1;

                }
                currentImg = 0;
                break;
            case 1: // attack player
                if (currentImg != 1)
                    gameObject.GetComponent<SpriteRenderer>().sprite = sprites[1];      // sets boss sprite to attack sprite
                attacktime += Time.deltaTime;
                if (attacktime > setAttackTime)
                {
                    if(playerX > playerY)
                    {
                        attackSpeedX = playerX / playerX;
                        attackSpeedY = playerY / playerX;
                    }
                    else
                    {
                        attackSpeedX = playerX / playerY;
                        attackSpeedY = playerY / playerY;
                    }

                    move(attackSpeedX * attackSpeed, attackSpeedY * attackSpeed, new Vector2(bossX + playerX * directionX , bossY + playerY * directionY));
                }
                if(attacktime > attackDuration)
                {
                    state = 2;
                    chargingTime = 0;
                }
                if(boundaries)
                    if(bossX < minX || bossX > maxX || bossY < minY || bossY > maxY)
                    {
                        state = 2;
                        chargingTime = 0;
                    }
                currentImg = 1;
                break;
            case 2: // recharging
                if (currentImg != 2)
                {
                    gameObject.GetComponent<SpriteRenderer>().sprite = sprites[2];  // sets boss sprite to charaging sprite
                    attacktime = 0;
                }
                if(chargingTime > chargingDuration)
                {
                    state = 0;
                }
                chargingTime += Time.deltaTime;
                currentImg = 2;
                break;

        }
        
    }
    public void move(float speedX, float speedY, Vector2 destination) // moves the boss to a location
    {
        if (destination.x > gameObject.transform.position.x)
        {
            gameObject.transform.position = new Vector2(gameObject.transform.position.x + speedX * Time.deltaTime, gameObject.transform.position.y);
        }
        else if (destination.x < gameObject.transform.position.x)
        {
            gameObject.transform.position = new Vector2(gameObject.transform.position.x - speedX * Time.deltaTime, gameObject.transform.position.y);
        }
        if (destination.y > gameObject.transform.position.y)
        {
            gameObject.transform.position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y + speedY * Time.deltaTime);
        }
        else if (destination.y < gameObject.transform.position.y)
        {
            gameObject.transform.position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - speedY * Time.deltaTime);
        }
    }
    public void takeDamage()
    {
        Destroy(hearts[--intHearts]);
        if(intHearts == 0)
        {
            // player won
        }

    }
}
