using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Estados posibles del juego
public enum GameState { Start, Playing, Paused, Victory, GameOver }

// Controla el flujo del juego. El jugador NO conoce al GameManager:
// el GameManager escucha los eventos de PlayerHealth y los demás scripts
// (ítems, terminal) le avisan con AddScore() y Win().
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private GameState state;
    private int score;
    private PlayerHealth playerHealth;

    private Texture2D overlayTexture;
    private GUIStyle titleStyle;
    private GUIStyle textStyle;
    private GUIStyle hudStyle;

    public GameState State => state;
    public int Score => score;

    // Otros scripts pueden suscribirse para reaccionar a los cambios de estado
    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        score = 0;

        // Escucha la salud del jugador
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
            playerHealth.OnDied += HandlePlayerDied;
        }
        else
        {
            Debug.LogWarning("[GameManager] No se encontró PlayerHealth en la escena.");
        }

        SetState(GameState.Start);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
            playerHealth.OnDied -= HandlePlayerDied;
        }
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        bool pausePressed = Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape);

        switch (state)
        {
            case GameState.Start:
                if (Input.GetKeyDown(KeyCode.Return)) SetState(GameState.Playing);
                break;

            case GameState.Playing:
                if (pausePressed) SetState(GameState.Paused);
                break;

            case GameState.Paused:
                if (pausePressed) SetState(GameState.Playing);
                break;

            case GameState.Victory:
            case GameState.GameOver:
                if (Input.GetKeyDown(KeyCode.R)) Restart();
                break;
        }
    }

    // ---------- CAMBIO DE ESTADO ----------
    private void SetState(GameState newState)
    {
        state = newState;

        // Solo corre el tiempo mientras se juega: así la pausa congela todo
        Time.timeScale = (state == GameState.Playing) ? 1f : 0f;

        Debug.Log("[GameManager] Estado: " + state + " | Vida: " + HealthText() + " | Puntaje: " + score);
        OnStateChanged?.Invoke(state);
    }

    private string HealthText()
    {
        if (playerHealth == null) return "-";
        return playerHealth.CurrentHealth + "/" + playerHealth.MaxHealth;
    }

    // ---------- API PARA OTROS SCRIPTS ----------
    public void AddScore(int points)
    {
        if (state != GameState.Playing) return;

        score += points;
        Debug.Log("[GameManager] Puntaje: " + score + " (+" + points + ")");
    }

    public void Win()
    {
        if (state != GameState.Playing) return;
        SetState(GameState.Victory);
    }

    // ---------- EVENTOS DEL JUGADOR ----------
    private void HandleHealthChanged(int currentHealth)
    {
        Debug.Log("[GameManager] Vida actual: " + HealthText());
    }

    private void HandlePlayerDied()
    {
        if (state != GameState.Playing) return;
        SetState(GameState.GameOver);
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---------- PANTALLA ----------
    private void OnGUI()
    {
        if (titleStyle == null) CreateStyles();

        // HUD arriba a la derecha
        GUI.Label(new Rect(Screen.width - 440f, 20f, 420f, 40f),
            "Vida: " + HealthText() + "   Puntos: " + score, hudStyle);

        if (state == GameState.Playing) return;

        // Fondo oscuro translúcido para los mensajes
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture);

        float cy = Screen.height / 2f;
        switch (state)
        {
            case GameState.Start:
                GUI.Label(new Rect(0, cy - 80f, Screen.width, 70f), "GHOST INFILTRATION", titleStyle);
                GUI.Label(new Rect(0, cy, Screen.width, 40f), "Presioná ENTER para comenzar", textStyle);
                GUI.Label(new Rect(0, cy + 40f, Screen.width, 40f), "WASD mover · Espacio saltar · E interactuar · Flechas cámara · P pausa", textStyle);
                break;

            case GameState.Paused:
                GUI.Label(new Rect(0, cy - 60f, Screen.width, 70f), "PAUSA", titleStyle);
                GUI.Label(new Rect(0, cy + 10f, Screen.width, 40f), "P o ESC para continuar", textStyle);
                break;

            case GameState.Victory:
                GUI.Label(new Rect(0, cy - 80f, Screen.width, 70f), "¡VICTORIA!", titleStyle);
                GUI.Label(new Rect(0, cy, Screen.width, 40f), "Puntaje final: " + score, textStyle);
                GUI.Label(new Rect(0, cy + 40f, Screen.width, 40f), "R para reiniciar", textStyle);
                break;

            case GameState.GameOver:
                GUI.Label(new Rect(0, cy - 80f, Screen.width, 70f), "DERROTA", titleStyle);
                GUI.Label(new Rect(0, cy, Screen.width, 40f), "Puntaje: " + score, textStyle);
                GUI.Label(new Rect(0, cy + 40f, Screen.width, 40f), "R para reiniciar", textStyle);
                break;
        }
    }

    private void CreateStyles()
    {
        overlayTexture = new Texture2D(1, 1);
        overlayTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.6f));
        overlayTexture.Apply();

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 56;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.white;

        textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize = 22;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.normal.textColor = Color.white;

        hudStyle = new GUIStyle(GUI.skin.label);
        hudStyle.fontSize = 22;
        hudStyle.fontStyle = FontStyle.Bold;
        hudStyle.alignment = TextAnchor.MiddleRight;
        hudStyle.normal.textColor = Color.white;
    }
}
