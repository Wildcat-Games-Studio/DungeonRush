using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CrystalController : MonoBehaviour
{
    #region Variables

    [SerializeField]
    private Hurtbox _hurtbox;
    [SerializeField]
    private Hitbox _hitbox;
    [SerializeField]
    private int _maxHealth = 10;
    private int _health;
    [SerializeField]
    private AnimationEventDispatch _crystalDispatch = null;

    private int _healthStage = 0;

    [SerializeField]
    private ParticleSystem _damageSystem;

    [Header("Crystal animations")]
    [SerializeField]
    private Animator _crystalAnimator = null;
    [SerializeField]
    private Animator _shadowAnimator = null;

    [Header("Bubble animations")]
    [SerializeField]
    private Animator _bubbleAnimator = null;
    [SerializeField]
    private AnimationEventDispatch _bubbleDispatch = null;

    [Header("Effect animations")]
    [SerializeField]
    private Animator _effectAnimator = null;
    [SerializeField]
    private AnimationEventDispatch _effectDispatch = null;

    [Header("Wait variables")]
    [SerializeField]
    private float _waitMin = 0;
    [SerializeField]
    private float _waitMax = 0;

    [Header("Laser variables")]
    [SerializeField]
    private LayerMask _laserLayer = 0;
    [SerializeField]
    private Transform _laserOrigin = null;
    [SerializeField]
    private EdgeCollider2D _laserCollider = null;

    [Header("Target state")]
    [SerializeField]
    private LineRenderer _targetLine = null;
    [SerializeField]
    private float _targetTime = 0.0f;
    [SerializeField]
    private float _targetSpeed = 1.0f;

    [Header("Shoot state")]
    [SerializeField]
    private LineRenderer _shootLine = null;
    [SerializeField]
    private float _shootTime = 0.0f;
    [SerializeField]
    private float _shootSpeed = 1.0f;

    [Header("Teleport state")]
    [SerializeField]
    private float _teleportRangeFromPlayer = 10.0f;
    [SerializeField]
    private float _teleportRangeFromWall = 10.0f;

    private Vector2 _sampledPlayerPos = Vector2.zero;


    // do not directly set this
    private State _state = null;

    // states that the machine can use
    private StateWaiting _stateWait;
    private StateCharge _stateCharge;
    private StateTarget _stateTarget;
    private StateShoot _stateShoot;
    private StateTeleport _stateTeleport;
    private StateDead _stateDead;

    #endregion

    private void Awake()
    {
        // initialize every state
        _stateWait = new StateWaiting(this);
        _stateCharge = new StateCharge(this);
        _stateTarget = new StateTarget(this);
        _stateShoot = new StateShoot(this);
        _stateTeleport = new StateTeleport(this);
        _stateDead = new StateDead(this);

        _health = _maxHealth;
    }

    void Start()
    {
        // add functions to the animation event dispatchers, this needs to be done for every animation event
        _bubbleDispatch.animationEvents.Add(_stateCharge.BubbleGrown);
        _bubbleDispatch.animationEvents.Add(_stateTarget.BubblePopped);

        _effectDispatch.animationEvents.Add(_stateTeleport.Hide);
        _effectDispatch.animationEvents.Add(_stateTeleport.Teleport);

        _crystalDispatch.animationEvents.Add(_stateDead.Falling);
        _crystalDispatch.animationEvents.Add(_stateDead.Dead);

        _hurtbox.onHit = OnHit;

        SetState(_stateWait);
    }

    private void OnHit(int damage)
    {
        int last_health = _health;
        _health -= damage;

        _damageSystem.Play();

        if (_health <= 0)
        {
            SetState(_stateDead);
            return;
        }

        int half_health = (int)(0.5 * _maxHealth);
        int quarter_health = (int)(0.25 * _maxHealth);

        if(_health <= half_health && last_health > half_health)
        {
            _healthStage = 1;
            _crystalAnimator.SetFloat("Damage", 1);
        }
        else if (_health <= quarter_health && last_health > quarter_health)
        {
            _healthStage = 2;
            _crystalAnimator.SetFloat("Damage", 2);
            _shadowAnimator.SetFloat("Damage", 1);
        }
    }

    void Update()
    {
        _state?.Update();
    }

    void SetState(State state)
    {
        _state?.Exit();
        _state = state;
        _state?.Enter();
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
        Handles.DrawWireDisc(_laserOrigin.position, Vector3.forward, _teleportRangeFromPlayer);
        Handles.color = Color.cyan;
        Handles.DrawWireDisc(_laserOrigin.position, Vector3.forward, _teleportRangeFromWall);
    }

    #region States

    class State
    {
        protected CrystalController _machine;
        public State(CrystalController machine) => _machine = machine;

        // called right when the state is activated
        public virtual void Enter() { }
        // called right before the state is deactivated
        public virtual void Exit() { }
        // called on every unity update
        public virtual void Update() { }
    }

    class StateWaiting : State
    {
        private float _waitTime;

        public StateWaiting(CrystalController machine) : base(machine) { }

        public override void Enter()
        {
            _machine._hitbox.gameObject.SetActive(true);
            _machine._hurtbox.gameObject.SetActive(true);
            _machine._laserCollider.gameObject.SetActive(false);
            float healthMultipler = (3 - _machine._healthStage) / 3.0f;
            _waitTime = Random.Range(_machine._waitMin * healthMultipler, _machine._waitMax * healthMultipler);
        }

        public override void Update()
        {
            if (_waitTime <= 0.0f)
            {
                // start the animation and change state at the end of it
                _machine.SetState(_machine._stateCharge);
                return;
            }

            _waitTime -= Time.deltaTime;
        }

    }

    class StateCharge : State
    {
        public StateCharge(CrystalController machine) : base(machine) { }

        public override void Enter()
        {
            _machine._hurtbox.gameObject.SetActive(false);
            // start the animation and let it handle the state
            _machine._bubbleAnimator.SetTrigger("grow");
        }

        public void BubbleGrown()
        {
            // animation has ended, change state
            _machine.SetState(_machine._stateTarget);
        }
    }

    class StateTarget : State
    {
        private float _stateTime;
        private float _countDown;
        private bool _valid;

        private float _speed;

        public StateTarget(CrystalController machine) : base(machine) { }

        public override void Enter()
        {
            // initialize variables for state lifetime
            _valid = true;
            _stateTime = _machine._targetTime;
            _countDown = _stateTime;

            // initialize line inside the origin to hide it
            _machine._targetLine.SetPositions(new Vector3[] { _machine._laserOrigin.position, _machine._laserOrigin.position });
            _machine._targetLine.gameObject.SetActive(true);

            // initialzie the player position to avoid any jump
            _machine._sampledPlayerPos = GameState.Instance.player.transform.position;

            // cache the speed to allow the line to slow down over time
            _speed = _machine._targetSpeed;
        }

        public override void Exit()
        {
            // hide the line by reseting
            _machine._targetLine.SetPositions(new Vector3[] { _machine._laserOrigin.position, _machine._laserOrigin.position });
            _machine._targetLine.gameObject.SetActive(false);
        }

        public override void Update()
        {
            if (_countDown <= 0.0f && _valid)
            {
                // start the animation and change state at the end of it
                _machine._bubbleAnimator.SetTrigger("pop");
                _valid = false;
                return;
            }

            // slow down the follow throughout the state
            float speed = _speed * (_countDown / _stateTime);

            // allow the laser to follow behind the player
            _machine._sampledPlayerPos = Vector2.Lerp(_machine._sampledPlayerPos, GameState.Instance.player.transform.position, Time.deltaTime * speed);

            // calculate the direction and then shoot the line to a wall
            Vector2 dir2Player = (_machine._sampledPlayerPos - (Vector2)_machine._laserOrigin.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(_machine._laserOrigin.position, dir2Player, Mathf.Infinity, _machine._laserLayer);
            if (hit)
            {
                _machine._targetLine.SetPosition(1, hit.point);
            }

            // decrease the width of the line throughout the life time
            _machine._targetLine.startWidth = _countDown / _stateTime;

            _countDown -= Time.deltaTime;
        }

        public void BubblePopped()
        {
            _machine._hurtbox.gameObject.SetActive(true);
            // animation has ended
            _machine.SetState(_machine._stateShoot);
        }
    }

    class StateShoot : State
    {
        private float _stateTime;
        private float _countDown;

        private float _rotationDir;
        private float _laserAngle;

        private float _shootSpeed;

        public StateShoot(CrystalController machine) : base(machine) { }

        public override void Enter()
        {
            // set the variables to allow the state to end after a time
            _stateTime = _machine._shootTime;
            _countDown = _stateTime;

            // initialize the laser to the origin to hide it
            _machine._shootLine.SetPositions(new Vector3[] { _machine._laserOrigin.position, _machine._laserOrigin.position });
            _machine._shootLine.gameObject.SetActive(true);
            _machine._laserCollider.SetPoints(new List<Vector2> { Vector2.zero, Vector2.zero });

            _machine._laserCollider.gameObject.SetActive(true);

            // calculate the angle to rotate in
            Vector2 toSample = (_machine._sampledPlayerPos - (Vector2)_machine._laserOrigin.position).normalized;
            Vector2 toPlayer = (GameState.Instance.player.transform.position - _machine._laserOrigin.position).normalized;
            _rotationDir = Mathf.Sign(Vector2.SignedAngle(toSample, toPlayer));

            // convert the vector to an angle for easier rotation
            _laserAngle = Vector2.SignedAngle(Vector2.right, toSample) * Mathf.Deg2Rad;

            _shootSpeed = _machine._shootSpeed * _machine._healthStage + 1.0f;
        }

        public override void Exit()
        {
            // reset the laser to hide it
            _machine._shootLine.SetPositions(new Vector3[] { _machine._laserOrigin.position, _machine._laserOrigin.position });
            _machine._shootLine.gameObject.SetActive(false);
            _machine._laserCollider.gameObject.SetActive(false);
        }

        public override void Update()
        {
            if (_countDown <= 0.0f)
            {
                _machine.SetState(_machine._stateTeleport);
                return;
            }

            // rotate the line and convert it to a vector
            _laserAngle += _rotationDir * _shootSpeed * Time.deltaTime;

            Vector2 dir2Player;
            dir2Player.x = Mathf.Cos(_laserAngle);
            dir2Player.y = Mathf.Sin(_laserAngle);

            RaycastHit2D hit = Physics2D.Raycast(_machine._laserOrigin.position, dir2Player, Mathf.Infinity, _machine._laserLayer);
            if (hit)
            {
                _machine._shootLine.SetPosition(1, hit.point);
                _machine._laserCollider.SetPoints(new List<Vector2> { _machine._laserOrigin.position - _machine._laserCollider.transform.position,
                                                                    hit.point - (Vector2)_machine._laserCollider.transform.position });
            }

            _countDown -= Time.deltaTime;
        }
    }

    class StateTeleport : State
    {
        private bool _firstRound;

        public StateTeleport(CrystalController machine) : base(machine) { }

        public override void Enter()
        {
            _machine._hurtbox.gameObject.SetActive(false);
            _firstRound = true;
            _machine._effectAnimator.SetTrigger("action");
            _machine._hitbox.gameObject.SetActive(false);
        }

        public override void Exit()
        {
        }

        // animation has reached largest point
        public void Hide()
        {
            if (_firstRound)
            {
                _machine._crystalAnimator.SetBool("isVisible", false);
                _machine._shadowAnimator.SetBool("isVisible", false);
            }
            else
            {
                _machine._crystalAnimator.SetBool("isVisible", true);
                _machine._shadowAnimator.SetBool("isVisible", true);
            }
        }

        // animation has reached the end
        public void Teleport()
        {
            if (_firstRound)
            {
                Vector2 originOffset = _machine._laserOrigin.position - _machine.transform.position;
                _machine.transform.position = FindTeleportLocation() + originOffset;
                _firstRound = false;
                _machine._effectAnimator.SetTrigger("action");
            }
            else
            {
                _machine.SetState(_machine._stateWait);
            }

        }

        private Vector2 FindTeleportLocation()
        {
            Vector2 res = new Vector2();

            float distsqr = _machine._teleportRangeFromPlayer * _machine._teleportRangeFromPlayer;

            Vector2 extents = new Vector2(GameState.Instance.GetRoomWidth() / 2.0f - _machine._teleportRangeFromWall, GameState.Instance.GetRoomHeight() / 2.0f - _machine._teleportRangeFromWall);

            uint timesDone = 500;

            do
            {
                res.x = Random.Range(-extents.x, extents.x);
                res.y = Random.Range(-extents.y, extents.y);
            }
            while (Vector2.SqrMagnitude(res - (Vector2)GameState.Instance.player.transform.position) < distsqr && timesDone-- > 1);

            return res;
        }
    }

    class StateDead : State
    {
        public StateDead(CrystalController machine) : base(machine) { }

        public override void Enter()
        {
            _machine._crystalAnimator.SetBool("isDead", true);
            _machine._hitbox.gameObject.SetActive(false);
        }

        public void Dead()
        {
            Destroy(_machine.gameObject);
        }
        public void Falling()
        {
            _machine._shadowAnimator.SetBool("isVisible", false);
        }
    }

    #endregion
}
