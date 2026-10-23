using UnityEngine;
using UnityEngine.AI;

// Dron centinela: patrulla un área y persigue al jugador al detectarlo.
// Necesita un NavMeshAgent y estar sobre el NavMesh horneado.
[RequireComponent(typeof(NavMeshAgent))]
public class SentinelAI : MonoBehaviour
{
    private enum State { Patrol, Chase }

    private NavMeshAgent agent;
    private PlayerMovement playerMovement;
    private Transform player;
    private PlayerHealth playerHealth;

    private State state;
    private Vector3 homePosition;
    private bool warnedNoNavMesh;

    // Patrulla
    private float patrolRadius;
    private float patrolSpeed;
    private float waitAtPoint;
    private float waitTimer;

    // Detección y persecución
    private float detectionRadius;   // a qué distancia ve al jugador
    private float loseRadius;        // a qué distancia lo pierde
    private float fieldOfView;       // ángulo de visión (grados)
    private float eyeHeight;
    private float chaseSpeed;
    private float repathInterval;
    private float repathTimer;

    // Ataque por contacto
    private int contactDamage;

    private void Start()
    {
        patrolRadius = 12f;
        patrolSpeed = 2f;
        waitAtPoint = 1.5f;

        detectionRadius = 10f;
        loseRadius = 14f;
        fieldOfView = 120f;
        eyeHeight = 1f;
        chaseSpeed = 4.5f;      // menor que la velocidad del jugador (5): se puede escapar
        repathInterval = 0.2f;

        contactDamage = 10;

        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 1.2f;

        // Si el dron no quedó sobre el NavMesh, lo acomoda en el punto caminable más cercano
        if (!agent.isOnNavMesh)
        {
            NavMeshHit placeHit;
            if (NavMesh.SamplePosition(transform.position, out placeHit, 10f, NavMesh.AllAreas))
                agent.Warp(placeHit.position);
        }

        playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        homePosition = transform.position;
        StartPatrol();
    }

    private void Update()
    {
        if (player == null) return;

        // Sin NavMesh debajo no se puede navegar: avisa una sola vez en vez de llenar la Console
        if (!agent.isOnNavMesh)
        {
            if (!warnedNoNavMesh)
            {
                warnedNoNavMesh = true;
                Debug.LogWarning("SentinelAI: el dron " + name + " no está sobre el NavMesh. Hornear el NavMesh o moverlo a una zona celeste.");
            }
            return;
        }

        if (state == State.Patrol) UpdatePatrol();
        else UpdateChase();
    }

    // ---------- PATRULLA ----------
    private void StartPatrol()
    {
        state = State.Patrol;
        agent.speed = patrolSpeed;
        waitTimer = 0f;
        GoToRandomPoint();
    }

    private void UpdatePatrol()
    {
        if (CanSeePlayer())
        {
            StartChase();
            return;
        }

        // Al llegar al punto, espera un momento y elige otro
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitAtPoint)
            {
                waitTimer = 0f;
                GoToRandomPoint();
            }
        }
    }

    // Elige un punto al azar dentro del área de patrulla, sobre el NavMesh
    private void GoToRandomPoint()
    {
        if (!agent.isOnNavMesh) return;

        Vector2 circle = Random.insideUnitCircle * patrolRadius;
        Vector3 candidate = homePosition + new Vector3(circle.x, 0f, circle.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidate, out hit, 5f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // ---------- PERSECUCIÓN ----------
    private void StartChase()
    {
        state = State.Chase;
        agent.speed = chaseSpeed;
        repathTimer = 0f;
    }

    private void UpdateChase()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // Pierde al jugador si se alejó demasiado o se volvió invisible
        if (distance > loseRadius || playerMovement.Stats.Invisible)
        {
            StartPatrol();
            return;
        }

        // Actualiza el destino cada cierto tiempo (no en cada frame)
        repathTimer += Time.deltaTime;
        if (repathTimer >= repathInterval)
        {
            repathTimer = 0f;
            agent.SetDestination(player.position);
        }
    }

    // ---------- DETECCIÓN ----------
    // Lo ve si no es invisible, está cerca, dentro del cono de visión
    // y no hay nada (terreno, paredes) en el medio.
    private bool CanSeePlayer()
    {
        if (playerMovement.Stats.Invisible) return false;

        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 toPlayer = (player.position + Vector3.up * 0.5f) - eye;

        if (toPlayer.magnitude > detectionRadius) return false;
        if (Vector3.Angle(transform.forward, toPlayer) > fieldOfView * 0.5f) return false;

        RaycastHit hit;
        if (Physics.Raycast(eye, toPlayer.normalized, out hit, detectionRadius, ~0, QueryTriggerInteraction.Ignore))
            return hit.transform.IsChildOf(player);

        return false;
    }

    // ---------- ATAQUE ----------
    // El jugador pierde salud al tocar al centinela
    private void OnCollisionStay(Collision collision)
    {
        if (playerHealth == null) return;

        if (collision.collider.transform.IsChildOf(player))
            playerHealth.TakeDamage(contactDamage);
    }

    // Dibuja el radio de detección y el de pérdida al seleccionar el dron (solo en Scene)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 10f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 14f);
    }
}