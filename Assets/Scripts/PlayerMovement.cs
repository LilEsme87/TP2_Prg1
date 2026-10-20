using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private float velocity;
    private float acceleration;
    private float jumpForce;
    private float groundCheckDistance;
    private float turnSpeed;
    private LayerMask groundMask;

    private Vector3 startPosition;
    private float fallLimit;

    private Rigidbody rb;
    private Collider col;
    private Transform cameraTransform;
    private PlayerStats stats;
    private IMovementStrategy movementStrategy;

    private Vector2 moveInput;
    private bool jumpRequested;

    // Acceso para power-ups y zonas (cambian stats.Velocity o stats.Invisible)
    public PlayerStats Stats => stats;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; // la rotación la controlamos por código

        if (Camera.main != null)
            cameraTransform = Camera.main.transform;

        velocity = 5f;
        acceleration = 30f;
        jumpForce = 6f;
        groundCheckDistance = 0.2f;  // largo del rayo hacia el suelo, medido desde los pies
        turnSpeed = 720f;            // grados por segundo al girar hacia donde camina

        // Solo cuentan como suelo los objetos de la capa "Ground"
        groundMask = LayerMask.GetMask("Ground");
        if (groundMask == 0)
        {
            Debug.LogWarning("PlayerMovement: no existe la capa Ground. Crearla y asignarla al terreno. Mientras tanto se usan todas las capas.");
            groundMask = ~0;
        }

        startPosition = transform.position;   // punto de reinicio
        fallLimit = startPosition.y - 15f;    // si cae más abajo, reinicia

        stats = new PlayerStats(velocity, acceleration);
        SetMovementStrategy(new AccelerateMovement());
    }

    public void SetMovementStrategy(IMovementStrategy strategy)
    {
        movementStrategy = strategy;
    }

    // Vuelve al punto de inicio
    public void ResetToStart()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
    }

    // Los inputs se leen en Update...
    private void Update()
    {
        if (Time.timeScale == 0f) return; // juego en pausa: no se procesa input

        // W A S D explícitos: las flechas quedan libres para girar la cámara
        float x = 0f;
        float z = 0f;
        if (Input.GetKey(KeyCode.D)) x += 1f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.W)) z += 1f;
        if (Input.GetKey(KeyCode.S)) z -= 1f;

        moveInput = CameraRelative(new Vector2(x, z));

        if (Input.GetKeyDown(KeyCode.Space))
            jumpRequested = true;

        RotateTowardsMovement();
    }

    // ...y la física se aplica en FixedUpdate
    private void FixedUpdate()
    {
        movementStrategy.Move(rb, stats, moveInput);

        if (jumpRequested && IsGrounded())
        {
            Vector3 v = rb.velocity;
            rb.velocity = new Vector3(v.x, jumpForce, v.z);
        }
        jumpRequested = false;

        if (transform.position.y < fallLimit)
            ResetToStart();
    }

    // W avanza hacia donde mira la cámara, A y D van a los costados
    private Vector2 CameraRelative(Vector2 input)
    {
        if (cameraTransform == null) return input;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 direction = right * input.x + forward * input.y;
        return new Vector2(direction.x, direction.z);
    }

    // El jugador mira hacia donde camina (el Raycast frontal usa transform.forward)
    private void RotateTowardsMovement()
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion target = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    // Chequeo de suelo: rayo desde los pies hacia abajo, filtrado por LayerMask
    private bool IsGrounded()
    {
        Bounds b = col.bounds;
        Vector3 feet = new Vector3(b.center.x, b.min.y + 0.05f, b.center.z);
        float distance = 0.05f + groundCheckDistance;

        Debug.DrawRay(feet, Vector3.down * distance, Color.green); // se ve en la ventana Scene

        return Physics.Raycast(feet, Vector3.down, distance, groundMask, QueryTriggerInteraction.Ignore);
    }
}