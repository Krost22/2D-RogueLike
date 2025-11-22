using System; // Necesario para usar Action
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // Definimos el evento estático.
    // 'static' significa que pertenece a la clase, no a una instancia específica.
    // Esto facilita que el GameManager se suscriba sin tener que buscar al enemigo concreto.
    public static event Action OnPlayerHit;

    // Se llama cuando este objeto colisiona con otro (ambos deben tener Collider2D, uno Rigidbody2D)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificamos si chocamos con el Jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Invocamos el evento.
            // El operador ?. comprueba si hay alguien escuchando antes de llamar a Invoke().
            // Si nadie escucha, no pasa nada y no da error.
            OnPlayerHit?.Invoke();
        }
    }
}
