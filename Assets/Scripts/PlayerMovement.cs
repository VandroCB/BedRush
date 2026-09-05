using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Control del jugador. Solo lee el input y se lo pasa a Entity,
/// que es quien resuelve el movimiento por fisica.
/// </summary>
public class PlayerMovement : Entity
{
    private void Update()
    {
        Move(ReadMoveInput());
    }

    private static Vector2 ReadMoveInput()
    {
        Vector2 input = Vector2.zero;

        // Keyboard.current es null si no hay teclado conectado (build movil, por ejemplo).
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
        }

        // El stick solo manda si el teclado esta quieto, para que no se peleen.
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null && input.sqrMagnitude < 0.0001f)
            input = gamepad.leftStick.ReadValue();

        return input;
    }

    protected override void OnDied()
    {
        // TODO: pantalla de game over. Por ahora dejamos el cuerpo en escena
        // para ver donde caiste, en vez de desactivarlo.
        enabled = false;
    }
}
