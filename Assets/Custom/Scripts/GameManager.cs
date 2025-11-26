using UnityEngine;
using MoreMountains.Feedbacks;

public class GameManager : MonoBehaviour
{
    [Header("Feedbacks")]
    public MMF_Player playerHitFeedback;
    public MMF_Player gameOverFeedback;

    // Se llama cuando el objeto se activa (ej. al empezar el juego o activarse en la jerarquía)
    private void OnEnable()
    {
        // Nos suscribimos al evento estático del EnemyController.
        EnemyController.OnPlayerHit += HandlePlayerHit;
        PlayerController.OnPlayerDeath += HandlePlayerDeath;
    }

    // Se llama cuando el objeto se desactiva o destruye.
    private void OnDisable()
    {
        EnemyController.OnPlayerHit -= HandlePlayerHit;
        PlayerController.OnPlayerDeath -= HandlePlayerDeath;
    }

    // Esta es la función que responde al evento.
    private void HandlePlayerHit()
    {
        Debug.Log("¡El jugador ha sido golpeado! (Mensaje desde GameManager)");
        
        // Reproducir feedback si está asignado
        if (playerHitFeedback != null)
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
}
