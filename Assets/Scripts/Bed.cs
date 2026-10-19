using UnityEngine;

// When the player touches it, monsters disappear and the level ends.
[RequireComponent(typeof(Collider2D))]
public class Bed : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerHealth>() == null)
            return;

        if (LevelManager.Instance != null)
            LevelManager.Instance.CompleteLevel();
    }
}
