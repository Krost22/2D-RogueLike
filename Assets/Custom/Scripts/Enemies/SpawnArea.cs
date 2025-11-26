using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SpawnArea : MonoBehaviour
{
    private BoxCollider2D _spawnCollider;

    private void Awake()
    {
        _spawnCollider = GetComponent<BoxCollider2D>();
        // Ensure the collider is a trigger so it doesn't physically block objects
        _spawnCollider.isTrigger = true;
    }

    /// <summary>
    /// Returns a random position within the bounds of the BoxCollider2D.
    /// </summary>
    public Vector2 GetRandomPosition()
    {
        Bounds bounds = _spawnCollider.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector2(x, y);
    }

    private void OnDrawGizmos()
    {
        // Draw a semi-transparent green box to visualize the spawn area in the editor
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        if (_spawnCollider != null)
        {
            Gizmos.DrawCube(_spawnCollider.bounds.center, _spawnCollider.bounds.size);
        }
        else
        {
            // Fallback if collider isn't cached yet (e.g. editor mode before play)
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                // Transform local bounds to world space roughly for visualization
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(col.offset, col.size);
            }
        }
    }
}
