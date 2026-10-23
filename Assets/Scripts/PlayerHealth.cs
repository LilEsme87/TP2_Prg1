using System;
using UnityEngine;

// Salud del jugador. No decide qué pasa al morir: avisa con eventos
// y el GameManager reacciona (así el jugador no se carga de lógica de juego).
public class PlayerHealth : MonoBehaviour
{
    private int maxHealth;
    private int currentHealth;
    private float invulnerableTime;   // segundos sin recibir daño tras un golpe
    private float lastHitTime;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    public event Action<int> OnHealthChanged;   // se dispara con la vida actual
    public event Action OnDied;

    private void Awake()
    {
        maxHealth = 100;
        currentHealth = maxHealth;
        invulnerableTime = 1f;
        lastHitTime = -999f;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        if (Time.time - lastHitTime < invulnerableTime) return;
        lastHitTime = Time.time;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log("Vida: " + currentHealth + "/" + maxHealth);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth == 0)
        {
            Debug.Log("El jugador fue derrotado");
            OnDied?.Invoke();
        }
    }
}