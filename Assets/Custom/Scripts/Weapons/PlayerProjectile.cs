using System;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public string wallTag = "Wall";
    public event Action OnWallHit;

    private float damage;
    private float speed;
    private bool isExplosive;
    private float explosionRadius;
    private Vector3 direction;

    private bool hasHit;
    private Collider2D lastHitCollider;

    public void Initialize(Vector3 dir, float dmg, float spd, bool explosive, float radius)
    {
        direction = dir;
        damage = dmg;
        speed = spd;
        isExplosive = explosive;
        explosionRadius = radius;
        
        hasHit = false;
        lastHitCollider = null;

        PoolManager.Instance.Despawn(gameObject, 5f); // Lifetime
    }

    void Update()
    {
        Vector3 previousPosition = transform.position;
        float moveDistance = speed * Time.deltaTime;
        transform.position += direction * moveDistance;

        // Raycast to prevent tunneling
        // Check for both Enemy and Wall layers (assuming default layer interaction or specific layer masks if needed but simple raycast works for now)
        RaycastHit2D hit = Physics2D.Raycast(previousPosition, direction, moveDistance);
        if (hit.collider != null)
        {
            HandleCollision(hit.collider);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }

    void HandleCollision(Collider2D other)
    {
        if (hasHit || other == lastHitCollider) return;

        if (other.CompareTag("Enemy"))
        {
            Hit(other.gameObject);
        }
        else if (other.CompareTag(wallTag))
        {
            hasHit = true;
            lastHitCollider = other;
            OnWallHit?.Invoke();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.HandlePlayerProjectileWallHit(transform.position);
            }
            
            if (isExplosive)
            {
                Explode();
            }
            PoolManager.Instance.Despawn(gameObject);
        }
    }

    void Hit(GameObject target)
    {
        hasHit = true;
        
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
