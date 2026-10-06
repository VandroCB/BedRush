using UnityEngine;

public class FlashlightDamage : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 25f;

    private void OnTriggerStay2D(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

        if (enemy == null)
            return;

        enemy.TakeDamage(damagePerSecond * Time.fixedDeltaTime);
    }
}