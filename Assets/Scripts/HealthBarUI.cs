using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
  [SerializeField] private PlayerHealth playerHealth;
  [SerializeField] private RectTransform healthBarFill;

  private float maxWidth;

  private void Awake()
  {
    maxWidth = healthBarFill.sizeDelta.x;
  }

  private void Update()
  {
    Vector2 size = healthBarFill.sizeDelta;
    size.x = maxWidth * playerHealth.HealthNormalized;
    healthBarFill.sizeDelta = size;
  }
}