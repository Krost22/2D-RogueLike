using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

public class GameManager : MonoBehaviour
{
    [Header("Feedbacks")]
    public MMF_Player playerHitFeedback;
    public MMF_Player gameOverFeedback;
    public MMF_Player projectilePlayerWallFeedback;
    public MMProgressBar lifeBar;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private PlayerController playerController;

    // Se llama cuando el objeto se activa (ej. al empezar el juego o activarse en la jerarquía)
    private void OnEnable()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        // Nos suscribimos al evento estático del EnemyController.
        EnemyController.OnPlayerHit += HandlePlayerHit;
        PlayerController.OnPlayerDeath += HandlePlayerDeath;

        if (playerController != null)
        {
            playerController.OnHealthChanged += HandleHealthChanged;
            // Inicializar barra de vida
            if (lifeBar != null)
            {
                lifeBar.UpdateBar(playerController.currentHealth=playerController.maxHealth, 0f, playerController.maxHealth);
            }
        }
    }

    // Se llama cuando el objeto se desactiva o destruye.
    private void OnDisable()
    {
        EnemyController.OnPlayerHit -= HandlePlayerHit;
        PlayerController.OnPlayerDeath -= HandlePlayerDeath;

        if (playerController != null)
        {
            playerController.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (lifeBar != null)
        {
            lifeBar.UpdateBar(current, 0f, max);
        }
    }

    // Esta es la función que responde al evento.
    private void HandlePlayerHit()
    {
        // Reproducir feedback si está asignado
        if (playerHitFeedback != null && playerController.isInvulnerable == false)
        {
            playerHitFeedback.PlayFeedbacks();
        }
    }

    private void HandlePlayerDeath()
    {
        Debug.Log("GAME OVER! (Mensaje desde GameManager)");

        if (gameOverFeedback != null)
        {
            gameOverFeedback.PlayFeedbacks();
        }
    }

    public void HandlePlayerProjectileWallHit(Vector3 position)
    {
        if (projectilePlayerWallFeedback != null)
        {
            projectilePlayerWallFeedback.transform.position = position;
            projectilePlayerWallFeedback.PlayFeedbacks();
        }
    }
}
