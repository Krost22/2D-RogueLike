using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 20f;
    public float deceleration = 30f;
    public float rotationSpeed = 10f; // Velocidad de rotación

    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Combat Settings")]
    public WeaponController weaponController;

    private Rigidbody2D rb;
    private Vector2 input;
    private Vector3 baseScale;
    private Animator animator;

    // Eventos para UI u otros sistemas
    public event Action<int, int> OnHealthChanged; // current, max
    public static event Action OnPlayerDeath;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        baseScale = transform.localScale;
        
        currentHealth = maxHealth;
    }

    // Input System
    public void OnMove(InputValue value)
    {
        input = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 moveDirection = Vector2.zero;

        // Si hay input significativo, calculamos la dirección snappeada a 8 direcciones
        if (input.sqrMagnitude > 0.01f)
        {
            // Calcular el ángulo del input en grados
            float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;

            // Snappear al incremento de 45 grados más cercano
            // 0, 45, 90, 135, 180, -135, -90, -45
            float snappedAngle = Mathf.Round(angle / 45f) * 45f;

            // Convertir de nuevo a vector dirección
            float rad = snappedAngle * Mathf.Deg2Rad;
            moveDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // Rotación instantánea
            transform.rotation = Quaternion.Euler(0, 0, snappedAngle);
        }

        // Velocidad objetivo usando la dirección snappeada
        Vector2 targetVelocity = moveDirection * moveSpeed;

        // Acelerar o frenar (mantenemos la física para el desplazamiento, pero la dirección es rígida)
        Vector2 currentVelocity = rb.linearVelocity;
        float rate = (input.sqrMagnitude > 0.01f) ? acceleration : deceleration;
        rb.linearVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);

        // Animaciones
        if (animator != null)
        {
            float speed = rb.linearVelocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        Debug.Log($"Player took {amount} damage. Current Health: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        Debug.Log($"Player healed {amount}. Current Health: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        OnPlayerDeath?.Invoke();
        // Lógica de Game Over aquí
    }

    // Stub para futuras mejoras
    public void ApplyUpgrade(string upgradeName)
    {
        Debug.Log($"Applying upgrade: {upgradeName}");
        // Implementar lógica de mejoras
    }
}
