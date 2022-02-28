using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Animator))]
public class CrystalBoss : MonoBehaviour
{
    [Header("State Times")]
    public float chargeTime;
    public float holdTime;
    public float collapseTime;
    public float shootTime;
    public float recoverTime;
    public Vector2 teleportWaitTimeRange;

    [Header("Teleport Min Distances")]
    public float teleportRangeFromPlayer;
    public float teleportRangeFromWalls;

    [Header("Attack Variables")]
    public float laserSpeed;
    public float spikeSpeed;
    [Range(0.0f, 1.0f)]
    public float spikeChance;
    public uint maxTeleportsBeforeSpike;

    [Header("Hold State Variables")]
    public float maxBubbleSize;
    public float holdWobble;
    public float holdAmplitude;

    [Header("External Dependencies")]
    public SpriteRenderer mainSprite;
    public Transform bubbleTransform;
    public LineRenderer targetLine;
    public LineRenderer attackLine;
    public SpikeGenerator spikeGenerator;

    private Animator m_animator;

    [SerializeField]
    private enum State { Idle, Charge, Hold, Collapse, Shoot, Recover, Teleport, TeleportWaiting, Spiking };
    [SerializeField]
    private State m_state;

    private float m_timer;

    private Vector2 m_hitPoint;
    private float m_teleportWaitTime;

    private float m_laserAngle;
    private float m_laserTraceDir;
    private uint m_numTeleports;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();

        m_state = State.Idle;
        m_timer = 0.0f;
        m_numTeleports = 0;
    }

    void Start()
    {
        targetLine.enabled = false;
        attackLine.enabled = false;

        spikeGenerator.attackFinished = FinishSpike;
    }

    void Update()
    {
        switch (m_state)
        {
            case State.Idle: IdleState(); break;
            case State.Charge: ChargeState(); break;
            case State.Hold: HoldState(); break;
            case State.Collapse: CollapseState(); break;
            case State.Shoot: ShootState(); break;
            case State.Recover: RecoverState(); break;
            case State.TeleportWaiting: TeleportWaitState(); break;
            default: break;
        }
    }

    private void IdleState()
    {
        if (m_timer >= 1.0f)
        {
            m_timer = 0.0f;
            ChangeStateCharge();
            return;
        }

        m_timer += Time.deltaTime;
    }

    private void ChangeStateCharge()
    {
        m_state = State.Charge;
        targetLine.enabled = true;

        targetLine.SetPositions(new Vector3[] { transform.position, transform.position });
    }

    private void ChargeState()
    {
        Vector2 dir2player = (GameState.Instance.player.transform.position - transform.position).normalized;

        if (m_timer >= chargeTime)
        {
            m_timer = 0.0f;
            m_state = State.Hold;

            bubbleTransform.localScale = new Vector2(maxBubbleSize, maxBubbleSize);
            m_laserAngle = Vector2.SignedAngle(Vector2.right, dir2player) * Mathf.Deg2Rad;

            return;
        }

        m_timer += Time.deltaTime;
        bubbleTransform.localScale = Vector2.Lerp(Vector2.zero, new Vector2(maxBubbleSize, maxBubbleSize), m_timer / chargeTime);

        Vector2? endpt = CalcLaserEnd(dir2player);

        if(endpt != null)
        {
            targetLine.SetPosition(1, endpt.Value);
            m_hitPoint = endpt.Value;
        }
    }

    private void HoldState()
    {
        if (m_timer >= holdTime)
        {
            m_timer = 0.0f;
            m_state = State.Collapse;

            attackLine.SetPosition(0, transform.position);
            attackLine.SetPosition(1, transform.position);
            targetLine.enabled = false;
            attackLine.enabled = true;

            return;
        }

        m_timer += Time.deltaTime;
        float s = Mathf.Sin(m_timer * holdWobble) * holdAmplitude;
        bubbleTransform.localScale = new Vector2(maxBubbleSize + s, maxBubbleSize + s);
    }

    private void CollapseState()
    {
        if (m_timer >= collapseTime)
        {
            m_timer = 0.0f;
            m_state = State.Shoot;

            bubbleTransform.localScale = Vector2.zero;
            attackLine.SetPosition(1, m_hitPoint);

            Vector2 dir2player = (GameState.Instance.player.transform.position - transform.position).normalized;
            float playerAngle = Vector2.SignedAngle(Vector2.right, dir2player) * Mathf.Deg2Rad;
            m_laserTraceDir = Mathf.Sign(playerAngle - m_laserAngle);

            return;
        }

        m_timer += Time.deltaTime;

        bubbleTransform.localScale = Vector2.Lerp(new Vector2(maxBubbleSize, maxBubbleSize), Vector2.zero, m_timer / collapseTime);
        attackLine.SetPosition(1, Vector2.Lerp(transform.position, m_hitPoint, m_timer / collapseTime));

    }

    private void ShootState()
    {
        if (m_timer >= shootTime)
        {
            m_timer = 0.0f;
            m_state = State.Recover;
            return;
        }

        m_timer += Time.deltaTime;

        m_laserAngle += m_laserTraceDir * Time.deltaTime * laserSpeed;
        Vector2 dir = new Vector2();
        dir.x = Mathf.Cos(m_laserAngle);
        dir.y = Mathf.Sin(m_laserAngle);

        Vector2? endpt = CalcLaserEnd(dir);
        if (endpt != null)
        {
            attackLine.SetPosition(1, (Vector3)endpt);
            m_hitPoint = endpt.Value;
        }
    }

    private void RecoverState()
    {
        if (m_timer >= recoverTime)
        {
            m_timer = 0.0f;
            bubbleTransform.localScale = Vector2.zero;
            attackLine.SetPosition(1, m_hitPoint);
            attackLine.enabled = false;
            
            ChangeStateTeleport();
            
            return;
        }

        m_timer += Time.deltaTime;

        if(m_timer < collapseTime)
        {
            attackLine.SetPosition(0, Vector2.Lerp(transform.position, m_hitPoint, m_timer * 2.0f / collapseTime));
        }
    }

    private void ChangeStateTeleport()
    {
        m_state = State.Teleport;
        m_animator.SetTrigger("Teleport");
    }

    private void StartAppear()
    {
        m_state = State.Teleport;
        m_animator.SetTrigger("Appear");
        mainSprite.forceRenderingOff = false;
    }

    private void TeleportWaitState()
    {
        if (m_timer >= m_teleportWaitTime)
        {
            m_timer = 0.0f;

            if (m_numTeleports >= maxTeleportsBeforeSpike || Random.value < spikeChance)
            {
                m_numTeleports = 0;
                ChangeStateSpiking();
            }
            else
            {
                m_numTeleports++;
                StartAppear();
            }

            return;
        }

        m_timer += Time.deltaTime;
    }

    private void DoTeleport()
    {
        mainSprite.forceRenderingOff = true;
        transform.position = FindTeleportLocation();
        m_state = State.TeleportWaiting;

        m_teleportWaitTime = Random.Range(teleportWaitTimeRange.x, teleportWaitTimeRange.y);
    }

    private void EndTeleport()
    {
        ChangeStateCharge();
    }

    private void ChangeStateSpiking()
    {
        m_state = State.Spiking;
        spikeGenerator.StartRandomAttack(spikeSpeed);
    }

    private void FinishSpike()
    {
        StartAppear();
    }

    private Vector2 FindTeleportLocation()
    {
        Vector2 res = new Vector2();

        float distsqr = teleportRangeFromPlayer * teleportRangeFromPlayer;

        Vector2 extents = new Vector2(GameState.Instance.GetRoomWidth() / 2.0f - teleportRangeFromWalls, GameState.Instance.GetRoomHeight() / 2.0f - teleportRangeFromWalls);

        uint timesDone = 500;

        do
        {
            res.x = Random.Range(-extents.x, extents.x);
            res.y = Random.Range(-extents.y, extents.y);
        }
        while (Vector2.SqrMagnitude(res - (Vector2)GameState.Instance.player.transform.position) < distsqr && timesDone-- > 1);

        return res;
    }

    private Vector2? CalcLaserEnd(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Mathf.Infinity, ~GameState.Instance.playerLayerMask);
        if (hit)
        {
            return hit.point;
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.forward, teleportRangeFromPlayer);
        Handles.color = Color.cyan;
        Handles.DrawWireDisc(transform.position, Vector3.forward, teleportRangeFromWalls);
    }
}
