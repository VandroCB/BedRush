using UnityEngine;

/// <summary>
/// Enemigo basico: persigue al jugador si lo tiene cerca y le hace daño al tocarlo.
/// La IA es deliberadamente tonta por ahora; ya la haremos mas interesante.
/// </summary>
public class Enemy : Entity
{
    [Header("Persecucion")]
    [Tooltip("A que distancia detecta al jugador. Fuera de este radio se queda quieto.")]
    [SerializeField] private float detectionRange = 8f;

    [Header("Ataque por contacto")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float attackCooldown = 1f;

    private Entity _target;
    private float _nextAttackTime;

    protected override void Awake()
    {
        base.Awake();
        _target = FindFirstObjectByType<PlayerMovement>();
    }

    private void Update()
    {
        if (!IsAlive || _target == null || !_target.IsAlive)
        {
            Move(Vector2.zero);
            return;
        }

        Vector2 toTarget = _target.transform.position - transform.position;
        bool inRange = toTarget.sqrMagnitude <= detectionRange * detectionRange;

        Move(inRange ? toTarget.normalized : Vector2.zero);
    }

    private void OnCollisionEnter2D(Collision2D collision) => TryDamage(collision.collider);

    private void OnCollisionStay2D(Collision2D collision) => TryDamage(collision.collider);

    private void TryDamage(Collider2D other)
    {
        if (!IsAlive || Time.time < _nextAttackTime)
            return;

        // GetComponentInParent para que funcione aunque el collider viva en un hijo del sprite.
        Entity victim = other.GetComponentInParent<Entity>();
        if (victim == null || victim == this || victim is Enemy)
            return;

        _nextAttackTime = Time.time + attackCooldown;
        victim.TakeDamage(contactDamage, this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
