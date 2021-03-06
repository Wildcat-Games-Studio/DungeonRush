using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class bilesPlayerController : MonoBehaviour
{
    [SerializeField]
    private EntityStats entityStats;

    [SerializeField]
    private Hitbox hitBox;

    [SerializeField]
    private float _maxSpeed = 20;
    [SerializeField]
    private float _accel = 10;
    [SerializeField]
    private float _friction = 50;

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
        hitBox.collidedWith = SwordDamage;
        if (GameState.Instance.player == null) { GameState.Instance.player = this; }
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

    private void SwordDamage(Collider2D collider, Vector2 normal)
    {
        Rigidbody2D rb = collider.attachedRigidbody;

        if (rb != null)
        {
            EntityStats stats = rb.GetComponent<EntityStats>();

            if (stats != null)
            {
                stats.Damage(1);
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 newVelo = m_rigidbody.velocity;

        if(m_inputVec != Vector2.zero)
        {
            Vector2 targetVelo = m_inputVec * _maxSpeed;
            newVelo = Vector2.Lerp(newVelo, targetVelo, Time.fixedDeltaTime * _accel);

            anim.SetFloat("Horizontal", m_inputVec.x);
            anim.SetFloat("Vertical", m_inputVec.y);

            anim.SetFloat("Speed", 1.0f);
        }
        else
        {
            newVelo = Vector2.Lerp(newVelo, Vector2.zero, Time.fixedDeltaTime * _friction);
            anim.SetFloat("Speed", 0.0f);
        }


        m_rigidbody.velocity = newVelo;
    }
}
