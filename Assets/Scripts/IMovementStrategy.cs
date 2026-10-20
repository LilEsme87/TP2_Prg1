using UnityEngine;

// Patrón Strategy: cada estrategia define CÓMO se mueve el jugador.
public interface IMovementStrategy
{
    // input.x = eje Horizontal (A/D), input.y = eje Vertical (W/S) -> se mapea a X y Z
    void Move(Rigidbody rb, PlayerStats stats, Vector2 input);
}

// Movimiento directo: la velocidad cambia al instante.
public class SmoothMovement : IMovementStrategy
{
    public void Move(Rigidbody rb, PlayerStats stats, Vector2 input)
    {
        Vector3 target = new Vector3(input.x, 0f, input.y);
        if (target.sqrMagnitude > 1f) target.Normalize();
        target *= stats.Velocity;

        Vector3 current = rb.velocity;
        rb.velocity = new Vector3(target.x, current.y, target.z);
    }
}

// Movimiento con aceleración: acelera al presionar y frena al soltar.
public class AccelerateMovement : IMovementStrategy
{
    public void Move(Rigidbody rb, PlayerStats stats, Vector2 input)
    {
        Vector3 target = new Vector3(input.x, 0f, input.y);
        if (target.sqrMagnitude > 1f) target.Normalize();
        target *= stats.Velocity;

        Vector3 current = rb.velocity;
        Vector3 horizontal = new Vector3(current.x, 0f, current.z);

        horizontal = Vector3.MoveTowards(horizontal, target, stats.Acceleration * Time.fixedDeltaTime);

        // Se conserva la velocidad vertical (gravedad / salto)
        rb.velocity = new Vector3(horizontal.x, current.y, horizontal.z);
    }
}

// Datos del jugador.
// - Velocity: la modifican los efectos (barro, power-ups) y ResetVelocity() la restaura.
// - Invisible: la consulta la IA enemiga; si es true, el jugador es indetectable.
public class PlayerStats
{
    private readonly float baseVelocity;
    private float velocity;
    private float acceleration;
    private bool invisible;

    public PlayerStats(float velocity, float acceleration)
    {
        baseVelocity = velocity;
        this.velocity = velocity;
        this.acceleration = acceleration;
        invisible = false;
    }

    public float BaseVelocity => baseVelocity;
    public float Velocity { get => velocity; set => velocity = value; }
    public float Acceleration { get => acceleration; set => acceleration = value; }
    public bool Invisible { get => invisible; set => invisible = value; }

    public void ResetVelocity()
    {
        velocity = baseVelocity;
    }
}