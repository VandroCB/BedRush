using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/*
 The player's flashlight.
 - Mouse aims it.
 - Normal beam (BeamAngle / BeamRange): only lights things up, drains the battery slowly.
 - Hold right click: the beam narrows into a line (FocusAngle / FocusRange), drains faster,
   and blinds the first monster it hits.
 - At 0% battery, it turns off.
 The look comes from a Spot Light 2D; the gameplay hit-test is a physics CircleCast.
 */

public class Flashlight : MonoBehaviour
{
    private const float MaxBattery = 100f;

    [Header("References")]
    [Tooltip("Spot Light 2D (child of the player) that draws the beam")]
    [SerializeField] private Light2D beamLight;
    [Tooltip("Camera used to turn the mouse position into a world position. Empty = Main Camera")]
    [SerializeField] private Camera aimCamera;

    [Header("Focused beam")]
    [Tooltip("Layers the focused beam can hit or be blocked by: Walls + Monsters")]
    [SerializeField] private LayerMask beamLayers;

    /// <summary>Battery in percent, 0-100.</summary>
    public float Battery { get; private set; }
    public float Battery01 => Battery / MaxBattery;
    /// <summary>Switched off by the LevelManager when the level ends.</summary>
    public bool ControlEnabled { get; set; } = true;
    public bool IsOn => ControlEnabled && Battery > 0f;
    public bool IsFocused { get; private set; }
    public Vector2 AimDirection { get; private set; } = Vector2.right;

    private float _currentAngle;
    private float _currentRange;

    private void Awake()
    {
        Battery = Mathf.Clamp(GameConfig.BatteryStartPercent, 0f, MaxBattery);
        _currentAngle = GameConfig.BeamAngle;
        _currentRange = GameConfig.BeamRange;

        if (aimCamera == null)
            aimCamera = Camera.main;

        if (beamLight != null)
        {
            beamLight.color = GameConfig.FlashlightColor;
            beamLight.intensity = GameConfig.FlashlightIntensity;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        ReadInput();
        DrainBattery();
        UpdateBeamLight();
    }

    // Physicas loop: focus beam interacts with monster
    private void FixedUpdate()
    {
        if (!IsFocused)
            return;

        // IsFocused already implies IsOn.
        // CircleCast used to get a "forgiving aim" compared to RayCast. It stops at the first wall or monster it detects. 
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            GameConfig.FocusBeamWidth * 0.5f,
            AimDirection,
            GameConfig.FocusRange,
            beamLayers);

        if (hit.collider == null)
            return;

        Monster monster = hit.collider.GetComponentInParent<Monster>();
        if (monster != null)
            monster.HitByFocusBeam(Time.fixedDeltaTime);
    }

    // Adds battery (clamped to 100%). 
    // Future modification: initial battery percentage will change depending on difficulty. 
    public void AddBattery(float percent)
    {
        Battery = Mathf.Min(MaxBattery, Battery + percent);
    }

    private void ReadInput()
    {
        IsFocused = false;

        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        if (aimCamera != null)
        {
            Vector2 screenPosition = mouse.position.ReadValue();
            Vector3 world = aimCamera.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, -aimCamera.transform.position.z));
            Vector2 toMouse = (Vector2)world - (Vector2)transform.position;

            if (toMouse.sqrMagnitude > 0.0001f)
                AimDirection = toMouse.normalized;
        }

        IsFocused = IsOn && mouse.leftButton.isPressed;
    }

    private void DrainBattery()
    {
        if (!IsOn)
            return;

        float rate = IsFocused ? GameConfig.BatteryDrainFocus : GameConfig.BatteryDrainNormal;
        Battery = Mathf.Max(0f, Battery - rate * Time.deltaTime);
    }

    private void UpdateBeamLight()
    {
        if (beamLight == null)
            return;

        // Smoothly morph between the normal cone and the focused line.
        float targetAngle = IsFocused ? GameConfig.FocusAngle : GameConfig.BeamAngle;
        float targetRange = IsFocused ? GameConfig.FocusRange : GameConfig.BeamRange;
        float t = 1f - Mathf.Exp(-GameConfig.BeamTransitionSpeed * Time.deltaTime);
        _currentAngle = Mathf.Lerp(_currentAngle, targetAngle, t);
        _currentRange = Mathf.Lerp(_currentRange, targetRange, t);

        beamLight.enabled = IsOn;

        // A Spot Light 2D shines along its local "up" axis.
        beamLight.transform.up = AimDirection;
        beamLight.pointLightOuterAngle = _currentAngle;
        beamLight.pointLightInnerAngle = _currentAngle * GameConfig.BeamInnerAngleRatio;
        beamLight.pointLightOuterRadius = _currentRange;
        beamLight.pointLightInnerRadius = 0f;
    }
}
