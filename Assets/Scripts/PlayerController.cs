using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public Rigidbody2D rb;
    public Animator anim;

    private float speedAddition;
    private Vector2 moveDirection;

    // Start is called before the first frame update
    void Start()
    {
        //if(GameState.Instance.player == null) GameState.Instance.player = this;
        speedAddition = 2;
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInputs();
        Animate();

        if(Input.GetKeyDown("space"))
        {
            StartCoroutine("Attack");
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    void ProcessInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;
        moveSpeed = Mathf.Clamp(moveDirection.magnitude, 0.0f, 1.0f);
    }

    void Move()
    {
        rb.velocity = moveDirection* (moveSpeed + speedAddition);
    }

    void Animate()
    {
        if(moveDirection != Vector2.zero)
        {
            anim.SetFloat("Horizontal", moveDirection.x);
            anim.SetFloat("Vertical", moveDirection.y);
        }
        anim.SetFloat("Speed", moveSpeed);
    }

    private IEnumerator Attack()
    {
        anim.SetTrigger("attack");
        yield return null;
    }
}
