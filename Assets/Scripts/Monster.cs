using System.Collections.Generic;
using UnityEngine;

public enum MonsterSpeed
{
    Slow,
    Normal,
    Fast
}


// A monster. It patrols between points, hunts the player when it has a clear line of sight,
// and when hit by the focused flashlight beam it is blinded: it walks back along its own steps
// (a trail of breadcrumbs) and can't hurt the player until it recovers.
// Touching the player removes hearts.

[RequireComponent(typeof(Rigidbody2D))]
public class Monster : MonoBehaviour
{
    private enum State
    {
        Patrol,
        Chase,
        Retreat
    }

    [Header("Behaviour")]
    [Tooltip("How fast this monster moves. The actual speeds are in GameConfig.txt")]
    [SerializeField] private MonsterSpeed speedTier = MonsterSpeed.Normal;
    [Tooltip("Points it walks between while it hasn't noticed the player. Empty = stands still. " +
             "Keep a clear straight path between consecutive points.")]
    [SerializeField] private Transform[] patrolPoints;
    [Tooltip("Layers that block its line of sight: Walls")]
    [SerializeField] private LayerMask sightBlockers;

    [Header("References")]
    [Tooltip("Player to hunt. Empty = found automatically")]
    [SerializeField] private PlayerHealth player;

    [Header("Feedback")]
    [Tooltip("Sprite tinted while the monster is blinded (optional)")]
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Color blindedColor = new Color(1f, 1f, 0.55f);

    // True while it is blinded and walking back. A blinded monster can't hurt the player.
    public bool IsBlinded => _state == State.Retreat;

    private Rigidbody2D _rigidbody;
    private Vector2[] _patrolPositions;
    private int _patrolIndex;
    private State _state = State.Patrol;

    private readonly List<Vector2> _trail = new();
    private Vector2 _lastKnownPlayerPosition;
    private float _loseSightTimer;
    private float _retreatTimer;
    private float _health;
    private Color _normalColor = Color.white;

    private float Speed
    {
        get
        {
            switch (speedTier)
            {
                case MonsterSpeed.Slow: return GameConfig.MonsterSpeedSlow;
                case MonsterSpeed.Fast: return GameConfig.MonsterSpeedFast;
                default: return GameConfig.MonsterSpeedNormal;
            }
        }
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0f;
        _rigidbody.freezeRotation = true;
        _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;

        _health = GameConfig.MonsterMaxHealth;

        // Copy the positions now, so it doesn't matter if the points are moved or parented later.
        _patrolPositions = new Vector2[patrolPoints != null ? patrolPoints.Length : 0];
        for (int i = 0; i < _patrolPositions.Length; i++)
            _patrolPositions[i] = patrolPoints[i].position;

        if (sprite != null)
            _normalColor = sprite.color;
    }

    private void Start()
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerHealth>();
    }

    private void Update()
    {
        if (sprite != null)
            sprite.color = IsBlinded ? blindedColor : _normalColor;
    }

    // All the 'CPU' movement happens in the physics loop.
    private void FixedUpdate()
    {
        Vector2 velocity;

        if (_state == State.Retreat)
        {
            velocity = Retreat();
        }
        else
        {
            UpdateAwareness();
            velocity = _state == State.Chase ? MoveToward(_lastKnownPlayerPosition, Speed) : Patrol();
            RecordTrail();
        }

        _rigidbody.linearVelocity = velocity;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (IsBlinded)
            return;

        PlayerHealth victim = collision.collider.GetComponentInParent<PlayerHealth>();
        if (victim != null)
            victim.TakeDamage(GameConfig.MonsterDamage);
    }

    // Called every physics step the focused flashlight beam is on this monster.
    public void HitByFocusBeam(float deltaTime)
    {
        _state = State.Retreat;
        _retreatTimer = GameConfig.MonsterBlindSeconds;

        float damagePerSecond = GameConfig.FocusDamagePerSecond;
        if (damagePerSecond > 0f)
        {
            _health -= damagePerSecond * deltaTime;
            if (_health <= 0f)
                Disappear();
        }
    }

    // Removes the monster (killed, or the level ended).
    public void Disappear()
    {
        Destroy(gameObject);
    }

    // ------------------------------------------------------------------

    private void UpdateAwareness()
    {
        if (CanSeePlayer())
        {
            _lastKnownPlayerPosition = player.transform.position;
            _loseSightTimer = GameConfig.MonsterLoseSightSeconds;
            _state = State.Chase;
        }
        else if (_state == State.Chase)
        {
            _loseSightTimer -= Time.fixedDeltaTime;
            if (_loseSightTimer <= 0f)
                _state = State.Patrol;
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null || player.IsDead)
            return false;

        Vector2 toPlayer = (Vector2)player.transform.position - _rigidbody.position;
        float distance = toPlayer.magnitude;

        if (distance > GameConfig.MonsterDetectRadius)
            return false;
        if (distance < 0.001f)
            return true;

        return Physics2D.Raycast(_rigidbody.position, toPlayer / distance, distance, sightBlockers).collider == null;
    }

    private Vector2 Patrol()
    {
        if (_patrolPositions.Length == 0)
            return Vector2.zero;

        Vector2 target = _patrolPositions[_patrolIndex];
        if (Vector2.Distance(_rigidbody.position, target) < 0.1f)
        {
            _patrolIndex = (_patrolIndex + 1) % _patrolPositions.Length;
            target = _patrolPositions[_patrolIndex];
        }

        return MoveToward(target, Speed * GameConfig.MonsterPatrolSpeedFactor);
    }

    // Walks back over the breadcrumbs, until the blind timer runs out or the trail is empty.
    private Vector2 Retreat()
    {
        _retreatTimer -= Time.fixedDeltaTime;

        while (_trail.Count > 0 && Vector2.Distance(_rigidbody.position, _trail[_trail.Count - 1]) < 0.1f)
            _trail.RemoveAt(_trail.Count - 1);

        if (_retreatTimer <= 0f || _trail.Count == 0)
        {
            _state = State.Patrol;
            return Vector2.zero;
        }

        return MoveToward(_trail[_trail.Count - 1], Speed);
    }

    private void RecordTrail()
    {
        Vector2 position = _rigidbody.position;

        if (_trail.Count > 0 && Vector2.Distance(position, _trail[_trail.Count - 1]) < GameConfig.MonsterTrailSpacing)
            return;

        _trail.Add(position);

        if (_trail.Count > GameConfig.MonsterTrailMaxPoints)
            _trail.RemoveAt(0);
    }

    // Velocity towards a point; never overshoots it within one physics step.
    private Vector2 MoveToward(Vector2 target, float speed)
    {
        Vector2 delta = target - _rigidbody.position;
        float distance = delta.magnitude;

        if (distance < 0.01f)
            return Vector2.zero;

        float maxSpeedToStopOnTarget = distance / Time.fixedDeltaTime;
        return delta / distance * Mathf.Min(speed, maxSpeedToStopOnTarget);
    }
}
