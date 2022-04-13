using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class bilesPlayerController : MonoBehaviour
{
    public PlayerStats playerStats;
    public SpellStats spellStats;

    [SerializeField]
    private Hurtbox _hurtbox;

    public Animator anim;

    private enum State { Moving };
    private State m_state;

    private Rigidbody2D m_rigidbody;

    private Vector2 m_inputVec;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (GameState.Instance.player == null) { GameState.Instance.player = this; }
        _hurtbox.onHit = OnHit;
    }

    private void OnHit(int damage)
    {
        playerStats.currentHealth -= damage;
        print("Player Health: " + playerStats.currentHealth);
    }

    public void MovementInput(InputAction.CallbackContext context)
    {
        m_inputVec = context.ReadValue<Vector2>();
    }

    public void AttackInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (context.ReadValueAsButton())
            {
                anim.SetTrigger("attack");

            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 newVelo = m_rigidbody.velocity;

        if(m_inputVec != Vector2.zero)
        {
            Vector2 targetVelo = m_inputVec * playerStats.maxSpeed;
            newVelo = Vector2.Lerp(newVelo, targetVelo, Time.fixedDeltaTime * playerStats.accelRate);

            anim.SetFloat("Horizontal", m_inputVec.x);
            anim.SetFloat("Vertical", m_inputVec.y);

            anim.SetFloat("Speed", 1.0f);
        }
        else
        {
            newVelo = Vector2.Lerp(newVelo, Vector2.zero, Time.fixedDeltaTime * playerStats.friction);
            anim.SetFloat("Speed", 0.0f);
        }


        m_rigidbody.velocity = newVelo;
    }
}
