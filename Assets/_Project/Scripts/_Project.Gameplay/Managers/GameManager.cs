using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }
    [SerializeField] GameObject gameOverUI; // optional canvas element

    public Random LootRng { get; private set; }

    bool gameOver;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        LootRng = new Random();
    }

    public void OnPlayerDied()
    {
        if (gameOver) return;
        gameOver = true;
        if (gameOverUI) gameOverUI.SetActive(true);
        // Freeze time if you like:
        // Time.timeScale = 0f;
    }

    void Update()
    {
        if (!gameOver) return;
        // Space = restart scene
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
