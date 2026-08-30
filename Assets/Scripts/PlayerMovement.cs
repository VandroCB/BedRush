using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D _rigidbody;
    private Vector2 _movement;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _movement = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            _movement.y += 1;

        if (Keyboard.current.sKey.isPressed)
            _movement.y -= 1;

        if (Keyboard.current.aKey.isPressed)
            _movement.x -= 1;

        if (Keyboard.current.dKey.isPressed)
            _movement.x += 1;

        _movement = _movement.normalized;
    }

    private void FixedUpdate()
    {
        _rigidbody.linearVelocity = _movement * speed;
    }
}