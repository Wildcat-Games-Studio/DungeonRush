using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class bilesPlayerController : MonoBehaviour
{
    public PlayerStats playerStats;
    public SpellStats spellStats;

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
    }

    public void MovementInput(InputAction.CallbackContext context)
    {
        m_inputVec = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector2 newVelo = m_rigidbody.velocity;

        if(m_inputVec != Vector2.zero)
        {
            Vector2 targetVelo = m_inputVec * playerStats.maxSpeed;
            newVelo = Vector2.Lerp(newVelo, targetVelo, Time.fixedDeltaTime * playerStats.accelRate);
        }
        else
        {
            newVelo = Vector2.Lerp(newVelo, Vector2.zero, Time.fixedDeltaTime * playerStats.friction);
        }


        m_rigidbody.velocity = newVelo;
    }
}
