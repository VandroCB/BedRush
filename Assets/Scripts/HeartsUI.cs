using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Shows one heart per life. When a life is lost, a heart disappears

public class HeartsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth player;
    [Tooltip("UI Image prefab with the heart sprite")]
    [SerializeField] private Image heartPrefab;
    [Tooltip("Parent of the hearts. Give it a Horizontal Layout Group")]
    [SerializeField] private Transform container;

    private readonly List<Image> _hearts = new();

    private void Start()
    {
        for (int i = 0; i < player.MaxLives; i++)
            _hearts.Add(Instantiate(heartPrefab, container));

        Refresh(player.Lives, player.MaxLives);
        player.LivesChanged += Refresh;
    }

    private void OnDestroy()
    {
        if (player != null)
            player.LivesChanged -= Refresh;
    }

    private void Refresh(int lives, int maxLives)
    {
        for (int i = 0; i < _hearts.Count; i++)
            _hearts[i].enabled = i < lives;
    }
}
