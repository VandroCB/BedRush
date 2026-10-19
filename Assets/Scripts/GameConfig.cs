using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// Global values of the game. They are read from Assets/Resources/GameConfig.txt when the game starts.
// Every value has a fallback here, so the game still runs if a line is missing from the TXT.
//
// Naming: keys ending in "Px" are written in pixels in the TXT and returned here in WORLD UNITS
// (pixels / PixelsPerUnit), so the rest of the code never has to convert.
//
// To add a new variable: add one line to the TXT and one property below.

// 'Use a static class as a unit of organization' - Thanks StackOverflow
public static class GameConfig
{
    // Assets/Resources/GameConfig.txt
    private const string FileName = "GameConfig";

    private static Dictionary<string, string> _values;

    // Makes sure the file is re-read on every Play (needed if "Enter Play Mode Options" skips domain reload).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay() => _values = null;

    // ---------- World ----------
    public static float PixelsPerUnit => Mathf.Max(1f, Number("PixelsPerUnit", 32f));

    // ---------- Player ----------
    public static float PlayerSpeed => Pixels("PlayerSpeedPx", 160f);
    public static int PlayerMaxLives => Integer("PlayerMaxLives", 8);
    public static int PlayerStartLives => Integer("PlayerStartLives", 8);
    public static float InvulnerabilitySeconds => Number("InvulnerabilitySeconds", 1.5f);
    public static int HeartPickupLives => Integer("HeartPickupLives", 1);

    // ---------- Flashlight ----------
    public static float BeamAngle => Number("BeamAngleDeg", 60f);
    public static float BeamRange => Pixels("BeamRangePx", 64f);
    public static float FocusAngle => Number("FocusAngleDeg", 4f);
    public static float FocusRange => Pixels("FocusRangePx", 128f);
    public static float FocusBeamWidth => Pixels("FocusBeamWidthPx", 8f);
    public static float BeamInnerAngleRatio => Number("BeamInnerAngleRatio", 0.7f);
    public static float BeamTransitionSpeed => Number("BeamTransitionSpeed", 12f);
    public static Color FlashlightColor => HexColor("FlashlightColor", "FFF4D6");
    public static float FlashlightIntensity => Number("FlashlightIntensity", 1f);

    // ---------- Battery ----------
    public static float BatteryStartPercent => Number("BatteryStartPercent", 100f);
    public static float BatteryDrainNormal => Number("BatteryDrainNormalPerSec", 1f);
    public static float BatteryDrainFocus => Number("BatteryDrainFocusPerSec", 4f);
    public static float BatteryPickupPercent => Number("BatteryPickupPercent", 25f);

    // ---------- Lantern ----------
    public static float LanternDuration => Number("LanternDuration", 30f);
    public static float LanternRadius => Pixels("LanternRadiusPx", 96f);
    public static Color LanternColor => HexColor("LanternColor", "FFE9A8");
    public static float LanternIntensity => Number("LanternIntensity", 0.9f);

    // ---------- Darkness ----------
    public static Color AmbientColor => HexColor("AmbientColor", "191970");
    public static float AmbientIntensity => Number("AmbientIntensity", 1f);

    // ---------- Monsters ----------
    public static int MonsterDamage => Integer("MonsterDamage", 1);
    public static float MonsterSpeedSlow => Pixels("MonsterSpeedSlowPx", 70f);
    public static float MonsterSpeedNormal => Pixels("MonsterSpeedNormalPx", 110f);
    public static float MonsterSpeedFast => Pixels("MonsterSpeedFastPx", 150f);
    public static float MonsterPatrolSpeedFactor => Number("MonsterPatrolSpeedFactor", 0.6f);
    public static float MonsterDetectRadius => Pixels("MonsterDetectRadiusPx", 192f);
    public static float MonsterLoseSightSeconds => Number("MonsterLoseSightSeconds", 1.5f);
    public static float MonsterBlindSeconds => Number("MonsterBlindSeconds", 2.5f);
    public static float MonsterTrailSpacing => Pixels("MonsterTrailSpacingPx", 16f);
    public static int MonsterTrailMaxPoints => Integer("MonsterTrailMaxPoints", 64);
    public static float MonsterMaxHealth => Number("MonsterMaxHealth", 3f);
    public static float FocusDamagePerSecond => Number("FocusDamagePerSecond", 0f);

    // ---------- Camera ----------
    public static float CameraDeadZoneWidth => Pixels("CameraDeadZoneWidthPx", 128f);
    public static float CameraDeadZoneHeight => Pixels("CameraDeadZoneHeightPx", 96f);
    public static float CameraFollowSharpness => Number("CameraFollowSharpness", 8f);

    // ------------------------------------------------------------------
    // Parsing helpers
    // ------------------------------------------------------------------

    private static float Pixels(string key, float fallbackPixels) => Number(key, fallbackPixels) / PixelsPerUnit;

    private static int Integer(string key, int fallback) => Mathf.RoundToInt(Number(key, fallback));

    private static float Number(string key, float fallback)
    {
        EnsureLoaded();
        if (_values.TryGetValue(key, out string text) &&
            float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }
        return fallback;
    }

    private static Color HexColor(string key, string fallbackHex)
    {
        EnsureLoaded();
        string hex = _values.TryGetValue(key, out string text) ? text : fallbackHex;
        if (!ColorUtility.TryParseHtmlString("#" + hex.TrimStart('#'), out Color color))
            ColorUtility.TryParseHtmlString("#" + fallbackHex, out color);
        return color;
    }

    private static void EnsureLoaded()
    {
        if (_values != null) return;

        _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        TextAsset file = Resources.Load<TextAsset>(FileName);
        if (file == null)
        {
            Debug.LogWarning("GameConfig.txt not found in Assets/Resources - using built-in default values.");
            return;
        }

        foreach (string rawLine in file.text.Split('\n'))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;

            int equals = line.IndexOf('=');
            if (equals <= 0) continue;

            string key = line.Substring(0, equals).Trim();
            string value = line.Substring(equals + 1).Trim();
            _values[key] = value;
        }
    }
}
