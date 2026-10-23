using UnityEngine;

// Opcional D: zona de barro. Reduce la velocidad al entrar y la restablece al salir.
// Su Collider debe tener Is Trigger activado.
public class MudZone : MonoBehaviour
{
    private float slowFactor;     // 0.4 = el jugador va al 40% de su velocidad
    private bool playerInside;
    private GUIStyle labelStyle;

    private void Start()
    {
        slowFactor = 0.4f;
        playerInside = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null) return;

        player.Stats.Velocity = player.Stats.BaseVelocity * slowFactor;
        playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null) return;

        player.Stats.ResetVelocity();
        playerInside = false;
    }

    private void OnGUI()
    {
        if (!playerInside) return;

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 22;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = new Color(0.8f, 0.55f, 0.25f);
        }

        GUI.Label(new Rect(0, 20f, Screen.width, 40f), "Barro: velocidad reducida", labelStyle);
    }
}