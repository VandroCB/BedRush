using System;
using UnityEngine;

/// <summary>
/// Base de todo lo vivo en el juego: jugador, enemigos y cualquier cosa
/// que se mueva o pueda recibir daño. Centraliza vida, daño y movimiento.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Entity : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 100;
    [Tooltip("Segundos de invulnerabilidad tras recibir un golpe. Evita que el daño por contacto vacie la vida en un frame.")]
    [SerializeField] private float invulnerabilityTime = 0.3f;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    public int MaxHealth => maxHealth;
    public int Health { get; private set; }
    public bool IsAlive => !_dead;

    /// <summary>Ultima direccion hacia la que se movio. Sirve para apuntar la linterna o el sprite.</summary>
    public Vector2 Facing { get; private set; } = Vector2.down;

    /// <summary>Se dispara al recibir daño: (quien lo recibe, cuanto).</summary>
    public event Action<Entity, int> Damaged;

    /// <summary>Se dispara una sola vez, al morir.</summary>
    public event Action<Entity> Died;

    protected Rigidbody2D Body { get; private set; }

    private Vector2 _moveDirection;
    private float _invulnerableUntil;
    private bool _dead;

    protected virtual void Awake()
    {
        Body = GetComponent<Rigidbody2D>();
        Health = maxHealth;
    }

    protected virtual void FixedUpdate()
    {
        Body.linearVelocity = IsAlive ? _moveDirection * moveSpeed : Vector2.zero;
    }

    /// <summary>
    /// Pide moverse en una direccion. Una magnitud mayor que 1 se normaliza,
    /// asi el stick analogico conserva su intensidad y el teclado no corre mas en diagonal.
    /// </summary>
    public void Move(Vector2 direction)
    {
        if (direction.sqrMagnitude > 1f)
            direction = direction.normalized;

        _moveDirection = direction;

        if (direction.sqrMagnitude > 0.0001f)
            Facing = direction.normalized;
    }

    /// <summary>
    /// Aplica daño. Lo ignora si esta muerta o dentro de los i-frames.
    /// El daño continuo (la linterna) pasa ignoreInvulnerability para marcar su propio ritmo:
    /// si no, seria el invulnerabilityTime de la victima quien decidiria cuanto quema la linterna.
    /// </summary>
    public void TakeDamage(int amount, Entity source = null, bool ignoreInvulnerability = false)
    {
        if (_dead || amount <= 0)
            return;

        if (!ignoreInvulnerability)
        {
            if (Time.time < _invulnerableUntil)
                return;

            _invulnerableUntil = Time.time + invulnerabilityTime;
        }

        Health = Mathf.Max(0, Health - amount);

        Damaged?.Invoke(this, amount);
        OnDamaged(amount, source);

        if (Health == 0)
            Kill();
    }

    public void Heal(int amount)
    {
        if (_dead || amount <= 0)
            return;

        Health = Mathf.Min(maxHealth, Health + amount);
    }

    /// <summary>Mata la entidad de inmediato, sin pasar por la vida ni los i-frames.</summary>
    public void Kill()
    {
        if (_dead)
            return;

        _dead = true;
        Health = 0;
        _moveDirection = Vector2.zero;

        Died?.Invoke(this);
        OnDied();
    }

    /// <summary>Gancho para reaccionar al daño: parpadeo, sonido, knockback.</summary>
    protected virtual void OnDamaged(int amount, Entity source) { }

    /// <summary>Por defecto la entidad desaparece. El jugador lo sobreescribe para el game over.</summary>
    protected virtual void OnDied()
    {
        gameObject.SetActive(false);
    }
}
