using UnityEngine;
using UnityEngine.Rendering.Universal;

// for Global Light 2D. It paints everything that isn't lit in the midnight-blue "shadow" tone defined in GameConfig.txt (AmbientColor / AmbientIntensity).
public class LevelLightning : MonoBehaviour
{
    private void Awake()
    {
        Light2D globalLight = GetComponent<Light2D>();
        globalLight.color = GameConfig.AmbientColor;
        globalLight.intensity = GameConfig.AmbientIntensity;
    }
}
