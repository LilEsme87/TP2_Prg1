using UnityEngine;

// Interacción a distancia: Raycast frontal filtrado por LayerMask.
// Solo detecta objetos de la capa "Interactable".
public class PlayerInteraction : MonoBehaviour
{
    private float interactRange;
    private LayerMask interactableMask;
    private IInteractable current;
    private GUIStyle promptStyle;

    private void Start()
    {
        interactRange = 3f;
        interactableMask = LayerMask.GetMask("Interactable");

        if (interactableMask == 0)
            Debug.LogWarning("PlayerInteraction: no existe la capa Interactable. Crearla y asignarla a la terminal.");
    }

    private void Update()
    {
        current = null;
        if (Time.timeScale == 0f) return; // juego en pausa

        // Rayo hacia donde mira el jugador
        Vector3 origin = transform.position;
        RaycastHit hit;
        if (interactableMask != 0 &&
            Physics.Raycast(origin, transform.forward, out hit, interactRange, interactableMask, QueryTriggerInteraction.Ignore))
        {
            current = hit.collider.GetComponentInParent<IInteractable>();
        }

        Debug.DrawRay(origin, transform.forward * interactRange, current != null ? Color.yellow : Color.gray);

        if (current != null && Input.GetKeyDown(KeyCode.E))
            current.Interact();
    }

    // Aviso en pantalla al apuntar a algo interactivo
    private void OnGUI()
    {
        if (current == null || string.IsNullOrEmpty(current.Prompt)) return;

        if (promptStyle == null)
        {
            promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 24;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = Color.yellow;
        }

        GUI.Label(new Rect(0, Screen.height - 120f, Screen.width, 40f), "E · " + current.Prompt, promptStyle);
    }
}