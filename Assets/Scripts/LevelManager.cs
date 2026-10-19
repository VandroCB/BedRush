using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/*
Level flow: win when the player reaches the bed, lose when lives run out.
After either, the game freezes, an end panel is shown and R restarts the level. A button will be added on the next Alpha.
 */

public class LevelManager : MonoBehaviour
{

    public static LevelManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerHealth player;

    [Header("End screens (UI panels)")]
    [Tooltip("Shown when the player reaches the bed")]
    [SerializeField] private GameObject winPanel;
    [Tooltip("Shown when the player loses all lives")]
    [SerializeField] private GameObject losePanel;

    public bool LevelEnded { get; private set; }

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        player.Died += OnPlayerDied;
    }

    private void OnDestroy()
    {
        if (player != null)
            player.Died -= OnPlayerDied;

        if (Instance == this)
            Instance = null;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!LevelEnded)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Called by the bed
    public void CompleteLevel()
    {
        if (LevelEnded)
            return;

        EndLevel();
        // Make monsters active on the scene disappear. Excludes inactive elements.
        foreach (Monster monster in FindObjectsByType<Monster>(FindObjectsInactive.Exclude))
            monster.Disappear();

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    // Self explanatory lol
    private void OnPlayerDied()
    {
        if (LevelEnded)
            return;

        EndLevel();

        if (losePanel != null)
            losePanel.SetActive(true);
    }

    private void EndLevel()
    {
        LevelEnded = true;

        if (player.TryGetComponent(out PlayerMovement movement))
            movement.MovementEnabled = false;

        if (player.TryGetComponent(out Flashlight flashlight))
            flashlight.ControlEnabled = false;

        Time.timeScale = 0f; // freezes physics, monsters and timers
    }
}
