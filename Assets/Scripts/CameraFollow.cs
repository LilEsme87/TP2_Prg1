using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;

    private float distance;        // distancia al jugador
    private float yaw;             // giro horizontal alrededor del jugador (grados)
    private float pitch;           // inclinación vertical (grados)
    private float minPitch;
    private float maxPitch;
    private float rotateSpeed;     // grados por segundo al usar las flechas
    private float lookAhead;       // cuánto mira hacia adelante
    private float smoothSpeed;

    private float collisionPadding;
    private float minDistance;
    private int obstacleMask;

    private void Start()
    {
        distance = 8f;
        yaw = 0f;
        pitch = 20f;
        minPitch = 5f;
        maxPitch = 60f;
        rotateSpeed = 90f;
        lookAhead = 4f;
        smoothSpeed = 8f;

        collisionPadding = 0.4f;
        minDistance = 1f;
        obstacleMask = ~0; // todas las capas

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) target = player.transform;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Flechas: izquierda/derecha giran la cámara, arriba/abajo la suben o bajan
        float dt = Time.unscaledDeltaTime;
        if (Time.timeScale != 0f)
        {
            if (Input.GetKey(KeyCode.RightArrow)) yaw += rotateSpeed * dt;
            if (Input.GetKey(KeyCode.LeftArrow)) yaw -= rotateSpeed * dt;
            if (Input.GetKey(KeyCode.UpArrow)) pitch += rotateSpeed * dt;
            if (Input.GetKey(KeyCode.DownArrow)) pitch -= rotateSpeed * dt;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // Posición deseada: detrás del jugador según el giro y la inclinación
        Vector3 pivot = target.position;
        Vector3 offset = Quaternion.Euler(pitch, yaw, 0f) * (Vector3.back * distance);
        Vector3 desired = pivot + offset;

        // Si hay algo entre el jugador y la cámara, acercar la cámara
        Vector3 direction = desired - pivot;
        float dist = direction.magnitude;
        direction /= dist;

        float closest = dist;
        RaycastHit[] hits = Physics.RaycastAll(pivot, direction, dist, obstacleMask, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            // Ignora al jugador y lo que lleva encima
            if (hit.transform.IsChildOf(target)) continue;
            if (hit.distance < closest) closest = hit.distance;
        }

        if (closest < dist)
            desired = pivot + direction * Mathf.Max(closest - collisionPadding, minDistance);

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);

        // Mira un poco por delante del jugador, en la dirección del giro
        Vector3 lookDir = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
        transform.LookAt(pivot + Vector3.up * 1.5f + lookDir * lookAhead);
    }
}