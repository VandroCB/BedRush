using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Feedback")]
    [Tooltip("Sprite that blinks while the player is invulnerable (optional)")]
    [SerializeField] private SpriteRenderer sprite;
    [Tooltip("Blinks per second while invulnerable")]
    [SerializeField] private float blinkRate = 10f;

    //Raised whenever lives change. Arguments: current lives, max lives.
    public event Action<int, int> LivesChanged;
    //Raised once, when lives reach 0.
    public event Action Died;

    public int Lives { get; private set; }
    public int MaxLives => GameConfig.PlayerMaxLives;
    public bool IsFull => Lives >= MaxLives;
    public bool IsDead => Lives <= 0;

    private float _invulnerableUntil;

    // Awake sets the value, the UI reads it in its own Start (Awake always runs before any Start).
    private void Awake()
    {
        Lives = Mathf.Clamp(GameConfig.PlayerStartLives, 1, MaxLives);
    }

    private void Update()
    {
        if (sprite == null)
            return;

        bool invulnerable = !IsDead && Time.time < _invulnerableUntil;
        bool hidden = invulnerable && Mathf.Repeat(Time.time * blinkRate, 1f) < 0.5f;

        Color color = sprite.color;
        color.a = hidden ? 0.35f : 1f;
        sprite.color = color;
    }

    //Removes lives. Returns false if ignored (already dead or still invulnerable)
    public bool TakeDamage(int amount)
    {
        if (IsDead || Time.time < _invulnerableUntil)
            return false;

        Lives = Mathf.Max(0, Lives - amount);
        _invulnerableUntil = Time.time + GameConfig.InvulnerabilitySeconds;
        LivesChanged?.Invoke(Lives, MaxLives);

        if (IsDead)
            Died?.Invoke();

        return true;
    }

    //Adds lives. Returns false if already at max (so the power-up stays on the floor)
    public bool TryHeal(int amount)
    {
        if (IsDead || IsFull)
            return false;

        Lives = Mathf.Min(MaxLives, Lives + amount);
        LivesChanged?.Invoke(Lives, MaxLives);
        return true;
    }
}
