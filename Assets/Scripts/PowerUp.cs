using UnityEngine;

// remaining: Super Lantern, Mask
public enum PowerUpType
{
    Battery,
    Heart,
    Lantern
}

/*
Each powerup lying on the floor. Needs a Collider2D with "Is Trigger" ticked.
If the player can't use it (like heart at max lives), it stays on the floor. Can be pickd later.
 */

[RequireComponent(typeof(Collider2D))]
public class PowerUp : MonoBehaviour
{

    [SerializeField] private PowerUpType type;
    // In case a powerup is not pickable
    private void OnTriggerStay2D(Collider2D other)
    {
        GameObject player = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;

        if (!player.TryGetComponent(out PlayerHealth health))
            return;

        if (TryApply(player, health))
            Destroy(gameObject);
    }

    private bool TryApply(GameObject player, PlayerHealth health)
    {
        switch (type)
        {
            case PowerUpType.Heart:
                return health.TryHeal(GameConfig.HeartPickupLives);

            case PowerUpType.Battery:
                if (player.TryGetComponent(out Flashlight flashlight))
                {
                    flashlight.AddBattery(GameConfig.BatteryPickupPercent);
                    return true;
                }
                return false;

            case PowerUpType.Lantern:
                if (player.TryGetComponent(out PlayerPowerUps powerUps))
                {
                    powerUps.ActivateLantern();
                    return true;
                }
                return false;
        }

        return false;
    }
}
