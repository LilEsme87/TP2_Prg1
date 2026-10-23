using UnityEngine;

// Terminal de datos: se activa con E (Raycast de PlayerInteraction).
// Solo da acceso si el jugador reunió suficientes puntos; entonces gana el juego.
public class Terminal : MonoBehaviour, IInteractable
{
    private int requiredScore;
    private bool used;
    private Renderer terminalRenderer;

    private string message;
    private float messageEndTime;
    private GUIStyle messageStyle;

    public string Prompt => used ? "" : "Usar terminal";

    private void Start()
    {
        requiredScore = 30;   // 3 ítems de datos de 10 puntos
        used = false;

        terminalRenderer = GetComponentInChildren<Renderer>();
        if (terminalRenderer != null)
            terminalRenderer.material.color = new Color(1f, 0.6f, 0.1f);  // naranja: sin usar
    }

    public void Interact()
    {
        if (used) return;

        GameManager gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Playing) return;

        if (gm.Score < requiredScore)
        {
            ShowMessage("Acceso denegado: faltan datos (" + gm.Score + "/" + requiredScore + ")");
            return;
        }

        used = true;
        if (terminalRenderer != null)
            terminalRenderer.material.color = Color.green;

        ShowMessage("Datos descargados");
        gm.Win();
    }

    private void ShowMessage(string text)
    {
        message = text;
        messageEndTime = Time.unscaledTime + 3f;
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(message) || Time.unscaledTime > messageEndTime) return;

        if (messageStyle == null)
        {
            messageStyle = new GUIStyle(GUI.skin.label);
            messageStyle.fontSize = 24;
            messageStyle.fontStyle = FontStyle.Bold;
            messageStyle.alignment = TextAnchor.MiddleCenter;
            messageStyle.normal.textColor = Color.white;
        }

        GUI.Label(new Rect(0, Screen.height - 170f, Screen.width, 40f), message, messageStyle);
    }
}
