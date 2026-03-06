using UnityEngine;
using UnityEngine.SceneManagement;
using Castlebound.Gameplay.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }
    [SerializeField] GameObject gameOverUI; // optional canvas element

    public Random LootRng { get; private set; }

    bool gameOver;
    GameOverScreenController gameOverScreenController;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        LootRng = new Random();
        ResolveGameOverScreenController();
    }

    public void OnPlayerDied()
    {
        if (gameOver) return;
        gameOver = true;

        if (gameOverScreenController != null)
        {
            gameOverScreenController.Show();
        }
        else if (gameOverUI)
        {
            gameOverUI.SetActive(true);
        }

        // Freeze time if you like:
        // Time.timeScale = 0f;
    }

    public void RequestRestart()
    {
        // Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (!gameOver) return;

        if (WasRestartKeyPressed())
        {
            RequestRestart();
        }
    }

    bool WasRestartKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Space))
            return true;
#endif
        return false;
    }

    void ResolveGameOverScreenController()
    {
        if (gameOverUI == null)
            return;

        gameOverScreenController = gameOverUI.GetComponent<GameOverScreenController>();
        if (gameOverScreenController == null)
            gameOverScreenController = gameOverUI.AddComponent<GameOverScreenController>();

        gameOverScreenController.SetRestartHandler(RequestRestart);
    }
}
