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

    private Animator _animator;

    private CrystalBossState _currentState;
    public CrystalBossState CurrentState
    {
        get { return _currentState; }
        set
        {

            _currentState?.Exit();
            _currentState = value;
            _currentState?.Enter();

        }
    }
    public CrystalBossIdle StateIdle { get; private set; }
    public CrystalBossCharge StateCharge { get; private set; }
    public CrystalBossHold StateHold { get; private set; }
    public CrystalBossCollapse StateCollapse { get; private set; }

    public float Timer { get; private set; }

    private Vector2 _hitPoint;
    private float _teleportWaitTime;

    private float _laserAngle;
    private float _laserTraceDir;
    private uint _numTeleports;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        Timer = 0.0f;
        _numTeleports = 0;

        StateIdle = new CrystalBossIdle(this, 1.0f);
        StateCharge = new CrystalBossCharge(this, chargeTime, bubbleTransform, maxBubbleSize);
        StateHold = new CrystalBossHold(this, holdTime, bubbleTransform, holdWobble, holdAmplitude);
        StateCollapse = new CrystalBossCollapse(this, collapseTime, bubbleTransform);
        CurrentState = StateIdle;
    }

    void Start()
    {
        targetLine.enabled = false;
        attackLine.enabled = false;

        //spikeGenerator.attackFinished = FinishSpike;
    }

    void Update()
    {
        CurrentState.Update(Time.deltaTime);
    }

    public void TickTimer(float delta) => Timer += delta;
    public void ResetTimer() => Timer = 0.0f;
    public bool CheckTimer(float value) => Timer >= value;


    //private void CollapseState()
    //{
    //    if (Timer >= collapseTime)
    //    {
    //        Timer = 0.0f;
    //        _state = State.Shoot;

    //        bubbleTransform.localScale = Vector2.zero;
    //        attackLine.SetPosition(1, _hitPoint);

    //        Vector2 dir2player = (GameState.Instance.player.transform.position - transform.position).normalized;
    //        float playerAngle = Vector2.SignedAngle(Vector2.right, dir2player) * Mathf.Deg2Rad;
    //        _laserTraceDir = Mathf.Sign(playerAngle - _laserAngle);

    //        return;
    //    }

    //    Timer += Time.deltaTime;

    //    bubbleTransform.localScale = Vector2.Lerp(new Vector2(maxBubbleSize, maxBubbleSize), Vector2.zero, Timer / collapseTime);
    //    attackLine.SetPosition(1, Vector2.Lerp(transform.position, _hitPoint, Timer / collapseTime));

    //}

    //private void ShootState()
    //{
    //    if (Timer >= shootTime)
    //    {
    //        Timer = 0.0f;
    //        _state = State.Recover;
    //        return;
    //    }

    //    Timer += Time.deltaTime;

    //    _laserAngle += _laserTraceDir * Time.deltaTime * laserSpeed;
    //    Vector2 dir = new Vector2();
    //    dir.x = Mathf.Cos(_laserAngle);
    //    dir.y = Mathf.Sin(_laserAngle);

    //    Vector2? endpt = CalcLaserEnd(dir);
    //    if (endpt != null)
    //    {
    //        attackLine.SetPosition(1, (Vector3)endpt);
    //        _hitPoint = endpt.Value;
    //    }
    //}

    //private void RecoverState()
    //{
    //    if (Timer >= recoverTime)
    //    {
    //        Timer = 0.0f;
    //        bubbleTransform.localScale = Vector2.zero;
    //        attackLine.SetPosition(1, _hitPoint);
    //        attackLine.enabled = false;

    //        ChangeStateTeleport();

    //        return;
    //    }

    //    Timer += Time.deltaTime;

    //    if (Timer < collapseTime)
    //    {
    //        attackLine.SetPosition(0, Vector2.Lerp(transform.position, _hitPoint, Timer * 2.0f / collapseTime));
    //    }
    //}

    //private void ChangeStateTeleport()
    //{
    //    _state = State.Teleport;
    //    _animator.SetTrigger("Teleport");
    //}

    //private void StartAppear()
    //{
    //    _state = State.Teleport;
    //    _animator.SetTrigger("Appear");
    //    mainSprite.forceRenderingOff = false;
    //}

    //private void TeleportWaitState()
    //{
    //    if (Timer >= _teleportWaitTime)
    //    {
    //        Timer = 0.0f;

    //        if (_numTeleports >= maxTeleportsBeforeSpike || Random.value < spikeChance)
    //        {
    //            _numTeleports = 0;
    //            ChangeStateSpiking();
    //        }
    //        else
    //        {
    //            _numTeleports++;
    //            StartAppear();
    //        }

    //        return;
    //    }

    //    Timer += Time.deltaTime;
    //}

    //private void DoTeleport()
    //{
    //    mainSprite.forceRenderingOff = true;
    //    transform.position = FindTeleportLocation();
    //    _state = State.TeleportWaiting;

    //    _teleportWaitTime = Random.Range(teleportWaitTimeRange.x, teleportWaitTimeRange.y);
    //}

    //private void EndTeleport()
    //{
    //    ChangeStateCharge();
    //}

    //private void ChangeStateSpiking()
    //{
    //    _state = State.Spiking;
    //    spikeGenerator.StartRandomAttack(spikeSpeed);
    //}

    //private void FinishSpike()
    //{
    //    StartAppear();
    //}

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

    public Vector2? CalcLaserEnd(Vector2 direction)
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

public class CrystalBossState
{
    protected CrystalBoss _machine { get; private set; }

    public CrystalBossState(CrystalBoss machine) => _machine = machine;

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(float delta) { }
}

public class CrystalBossIdle : CrystalBossState
{
    private float _stateTime;
    public CrystalBossIdle(CrystalBoss state, float stateTime) : base(state)
    {
        _stateTime = stateTime;
    }

    public override void Exit()
    {
        _machine.ResetTimer();
    }

    public override void Update(float delta)
    {
        if (_machine.CheckTimer(_stateTime))
        {
            _machine.CurrentState = _machine.StateCharge;
            return;
        }

        _machine.TickTimer(delta);
    }
}

public class CrystalBossCharge : CrystalBossState
{
    private float _stateTime;
    private Transform _bubbleTransform;
    private float _maxBubbleSize;
    public CrystalBossCharge(CrystalBoss state, float stateTime, Transform bubbleTransform, float maxBubbleSize) : base(state)
    {
        _stateTime = stateTime;
        _bubbleTransform = bubbleTransform;
        _maxBubbleSize = maxBubbleSize;
    }

    public override void Enter()
    {
        _machine.targetLine.enabled = true;
        _machine.targetLine.SetPositions(new Vector3[] { _machine.transform.position, _machine.transform.position });
    }

    public override void Exit()
    {
        _machine.ResetTimer();
        _bubbleTransform.localScale = new Vector2(_maxBubbleSize, _maxBubbleSize);
        //_laserAngle = Vector2.SignedAngle(Vector2.right, dir2player) * Mathf.Deg2Rad;
    }

    public override void Update(float delta)
    {
        Vector2 dir2player = (GameState.Instance.player.transform.position - _machine.transform.position).normalized;

        if (_machine.CheckTimer(_stateTime))
        {
            _machine.CurrentState = _machine.StateHold;
            return;
        }

        _machine.TickTimer(delta);

        _bubbleTransform.localScale = Vector2.Lerp(Vector2.zero, new Vector2(_maxBubbleSize, _maxBubbleSize), _machine.Timer / _stateTime);

        Vector2? endpt = _machine.CalcLaserEnd(dir2player);

        if (endpt != null)
        {
            _machine.targetLine.SetPosition(1, endpt.Value);
            //_hitPoint = endpt.Value;
        }
    }
}

public class CrystalBossHold : CrystalBossState
{
    private float _stateTime;
    private Transform _bubbleTransform;
    private float _maxBubbleSize;
    private float _holdWobble;
    private float _holdAmplitude;

    public CrystalBossHold(CrystalBoss state, float stateTime, Transform bubbleTransform, float holdWobble, float holdAmplitude) : base(state)
    {
        _stateTime = stateTime;
        _bubbleTransform = bubbleTransform;
        _maxBubbleSize = 0.0f;
        _holdWobble = holdWobble;
        _holdAmplitude = holdAmplitude;
    }

    public override void Enter()
    {
        _maxBubbleSize = _bubbleTransform.localScale.x;
        _machine.targetLine.enabled = true;
    }

    public override void Exit()
    {
        _machine.ResetTimer();
        _machine.targetLine.enabled = false;
        //_laserAngle = Vector2.SignedAngle(Vector2.right, dir2player) * Mathf.Deg2Rad;
    }

    public override void Update(float delta)
    {

        if(_machine.CheckTimer(_stateTime))
        {
            _machine.CurrentState = _machine.StateCollapse;
            return;
        }
        _machine.TickTimer(delta);

        float s = Mathf.Sin(Time.time * _holdWobble) * _holdAmplitude;
        _bubbleTransform.localScale = new Vector2(_maxBubbleSize + s, _maxBubbleSize + s);
    }
}

public class CrystalBossCollapse : CrystalBossState
{
    private float _stateTime;
    private Transform _bubbleTransform;
    private float _maxBubbleSize;

    public CrystalBossCollapse(CrystalBoss state, float stateTime, Transform bubbleTransform) : base(state)
    {
        _stateTime = stateTime;
        _bubbleTransform = bubbleTransform;
        _maxBubbleSize = 0.0f;
    }

    public override void Enter()
    {
        _machine.attackLine.enabled = true;
        _machine.attackLine.SetPosition(0, _machine.transform.position);
        _machine.attackLine.SetPosition(1, _machine.transform.position);

        _maxBubbleSize = _bubbleTransform.localScale.x;
    }

    public override void Exit()
    {
        _machine.ResetTimer();
        //_laserAngle = Vector2.SignedAngle(Vector2.right, dir2player) * Mathf.Deg2Rad;
    }

    public override void Update(float delta)
    {
        if (_machine.CheckTimer(_stateTime))
        {
            _machine.CurrentState = _machine.StateIdle;
            //_state = State.Shoot;

            _bubbleTransform.localScale = Vector2.zero;
            _machine.attackLine.SetPosition(1, _machine.targetLine.GetPosition(1));

            //Vector2 dir2player = (GameState.Instance.player.transform.position - transform.position).normalized;
            //float playerAngle = Vector2.SignedAngle(Vector2.right, dir2player) * Mathf.Deg2Rad;
            //_laserTraceDir = Mathf.Sign(playerAngle - _laserAngle);

            return;
        }
        _machine.TickTimer(delta);

        _bubbleTransform.localScale = Vector2.Lerp(new Vector2(_maxBubbleSize, _maxBubbleSize), Vector2.zero, _machine.Timer / _stateTime);
        _machine.attackLine.SetPosition(1, Vector2.Lerp(_machine.transform.position, _machine.targetLine.GetPosition(1), _machine.Timer / _stateTime));
    }
}
