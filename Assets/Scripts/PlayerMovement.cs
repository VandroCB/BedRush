using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    //[SerializeField] private float speed = 5f;
    public bool MovementEnabled { get; set; } = true;

    private Rigidbody2D _rigidbody;
    private Vector2 _movement;



    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        // Top-down setup. Collisions with walls come for free from the Rigidbody2D + Collider2D.
        _rigidbody.gravityScale = 0f;
        _rigidbody.freezeRotation = true;
        _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        _movement = Vector2.zero;

        //added for end level logic 
        Keyboard keyboard = Keyboard.current;
        if (!MovementEnabled || keyboard == null)
            return;

        if (keyboard.wKey.isPressed)
            _movement.y += 1;

        if (keyboard.sKey.isPressed)
            _movement.y -= 1;

        if (keyboard.aKey.isPressed)
            _movement.x -= 1;

        if (keyboard.dKey.isPressed)
            _movement.x += 1;

        _movement = _movement.normalized;
    }

    private void FixedUpdate()
    {
        _rigidbody.linearVelocity = _movement * GameConfig.PlayerSpeed;
    }
}