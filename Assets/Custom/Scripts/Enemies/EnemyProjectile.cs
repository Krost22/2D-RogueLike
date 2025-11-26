using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float speed;
    private int damage = 1;
    private Vector3 direction;
    private float lifetime = 5f; // Destroy after 5 seconds to prevent clutter

    public void Initialize(Vector3 moveDirection, float projectileSpeed, int projectileDamage, Sprite projectileSprite)
    {
        direction = moveDirection.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;

        if (projectileSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = projectileSprite;
        }

        // Rotate projectile to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        PoolManager.Instance.Despawn(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Logic to damage player would go here. 
            // For now we can use the existing event system or just log it.
            // Since the user didn't ask for a full player health system yet, 
            // we will just log and destroy.
            Debug.Log($"Projectile hit Player for {damage} damage!");
            EnemyController.NotifyPlayerHit();
            
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            PoolManager.Instance.Despawn(gameObject);
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
             PoolManager.Instance.Despawn(gameObject);
        }
    }
}
