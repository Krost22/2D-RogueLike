using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private bool isExplosive;
    private float explosionRadius;
    private Vector3 direction;

    public void Initialize(Vector3 dir, float dmg, float spd, bool explosive, float radius)
    {
        direction = dir;
        damage = dmg;
        speed = spd;
        isExplosive = explosive;
        explosionRadius = radius;
        
        PoolManager.Instance.Despawn(gameObject, 5f); // Lifetime
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Hit(other.gameObject);
        }
    }

    void Hit(GameObject target)
    {
        if (isExplosive)
        {
            Explode();
        }
        else
        {
            // Deal single target damage
            EnemyController enemy = target.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage);
            }
        }
        PoolManager.Instance.Despawn(gameObject);
    }

    void Explode()
    {
        // Visual effect would go here
        Debug.Log("Rocket Exploded!");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            // Damage Enemies
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage);
            }

            // Damage Player (Friendly Fire)
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage((int)(damage / 2)); // Half damage to self? Or full?
            }
        }
    }
}
