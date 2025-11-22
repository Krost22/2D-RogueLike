using UnityEngine;
using MoreMountains.Feedbacks;

public class GameManager : MonoBehaviour
{
    [Header("Feedbacks")]
    public MMF_Player playerHitFeedback;

    // Se llama cuando el objeto se activa (ej. al empezar el juego o activarse en la jerarquía)
    private void OnEnable()
    {
        // Nos suscribimos al evento estático del EnemyController.
        // Cuando 'OnPlayerHit' ocurra, se ejecutará 'HandlePlayerHit'.
        EnemyController.OnPlayerHit += HandlePlayerHit;
    }

    // Se llama cuando el objeto se desactiva o destruye.
    // ES CRÍTICO desuscribirse para evitar errores de memoria o llamadas a objetos destruidos.
    private void OnDisable()
    {
        EnemyController.OnPlayerHit -= HandlePlayerHit;
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
}
