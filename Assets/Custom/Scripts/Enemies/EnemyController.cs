using System; // Necesario para usar Action
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType
    {
        Chaser,
        Kiter,
        Turret
    }

    [Header("Enemy Configuration")]
    public EnemyType enemyType;

    [Header("Movement Stats")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 5f; // For Kiter
    public float retreatDistance = 3f;  // For Kiter

    [Header("Combat Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int contactDamage = 1;

    [Header("Shooting Stats")]
    public GameObject projectilePrefab;
    public float fireRate = 1f;
    public float projectileSpeed = 5f;
    public int projectileDamage = 1;
    public Sprite projectileSprite;

    private Transform player;
    private float nextFireTime;
    private bool isDead = false;

    // Definimos el evento estático para cuando golpea al jugador
    public static event Action OnPlayerHit;
    // Evento estático para cuando muere un enemigo (para el HordeManager)
    public static event Action<EnemyController> OnEnemyDeath;

    private void OnEnable()
    {
        currentHealth = maxHealth;
        isDead = false;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Start()
    {
        // Initialization moved to OnEnable for pooling support
    }

    private void Update()
    {
        if (player == null) return;

        switch (enemyType)
        {
            case EnemyType.Chaser:
                HandleChaserBehavior();
                break;
            case EnemyType.Kiter:
                HandleKiterBehavior();
                break;
            case EnemyType.Turret:
                HandleTurretBehavior();
                break;
        }
    }

    private void HandleChaserBehavior()
    {
        // Move towards player
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }

    private void HandleKiterBehavior()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > stoppingDistance)
        {
            // Approach player
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
        else if (distanceToPlayer < retreatDistance)
        {
            // Retreat from player
            transform.position = Vector2.MoveTowards(transform.position, player.position, -moveSpeed * Time.deltaTime);
        }
        else
        {
            // Stay put
            transform.position = this.transform.position;
        }

        // Kiter also shoots
        AttemptToShoot();
    }

    private void HandleTurretBehavior()
    {
        // Turret doesn't move, just shoots
        AttemptToShoot();
    }

    private void AttemptToShoot()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null) return;

        GameObject projObj = PoolManager.Instance.Spawn(projectilePrefab, transform.position, Quaternion.identity);
        EnemyProjectile projectile = projObj.GetComponent<EnemyProjectile>();
        
        if (projectile != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            projectile.Initialize(direction, projectileSpeed, projectileDamage, projectileSprite);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        OnEnemyDeath?.Invoke(this);
        PoolManager.Instance.Despawn(gameObject);
    }

    public static void NotifyPlayerHit()
    {
        OnPlayerHit?.Invoke();
    }

    // Se llama cuando este objeto colisiona con otro (ambos deben tener Collider2D, uno Rigidbody2D)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificamos si chocamos con el Jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Invocamos el evento.
            OnPlayerHit?.Invoke();
            Debug.Log($"Enemy dealt {contactDamage} contact damage to Player.");
            
            // Optional: Enemy could take damage or die on impact? 
            // For now, let's just deal damage to player.
            // If we want the player to take damage properly, we should access PlayerController
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(contactDamage);
            }
        }
    }
}
