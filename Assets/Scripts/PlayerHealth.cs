using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public float HealthNormalized => currentHealth / maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0f);

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }
}