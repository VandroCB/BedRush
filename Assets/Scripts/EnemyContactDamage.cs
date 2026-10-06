using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
  [SerializeField] private float damagePerSecond = 20f;

  private void OnCollisionStay2D(Collision2D collision)
  {
    PlayerHealth player = collision.collider.GetComponentInParent<PlayerHealth>();

    if (player == null)
      return;

    player.TakeDamage(damagePerSecond * Time.fixedDeltaTime);
  }
}