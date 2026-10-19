using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerPowerUps : MonoBehaviour
{
    [Header("Lantern")]
    [Tooltip("Point Light 2D (child of the player). It is only switched on while the lantern is active")]
    [SerializeField] private Light2D lanternLight;

    public bool LanternActive => LanternTimeLeft > 0f;
    public float LanternTimeLeft { get; private set; }

    private void Awake()
    {
        if (lanternLight == null)
            return;

        lanternLight.color = GameConfig.LanternColor;
        lanternLight.intensity = GameConfig.LanternIntensity;
        lanternLight.pointLightOuterAngle = 360f;
        lanternLight.pointLightInnerAngle = 360f;
        lanternLight.pointLightOuterRadius = GameConfig.LanternRadius;
        lanternLight.pointLightInnerRadius = 0f;
        lanternLight.enabled = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (LanternTimeLeft <= 0f)
            return;

        LanternTimeLeft -= Time.deltaTime;

        if (LanternTimeLeft <= 0f)
        {
            LanternTimeLeft = 0f;
            if (lanternLight != null)
                lanternLight.enabled = false;
        }
    }

    //Starts (or refreshes) the lantern power up
    public void ActivateLantern()
    {
        LanternTimeLeft = GameConfig.LanternDuration;
        if (lanternLight != null)
            lanternLight.enabled = true;
    }
}
