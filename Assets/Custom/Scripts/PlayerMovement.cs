using UnityEngine;
using UnityEngine.InputSystem; // nuevo Input System

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float acceleration = 20f;
    public float deceleration = 30f;

    private Rigidbody2D rb;
    private Vector2 input;
    private Vector3 baseScale;
    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        baseScale = transform.localScale;
    }

    // Este método lo llama automáticamente el componente Player Input
    // cuando se actualiza la acción "Move"
    public void OnMove(InputValue value)
    {
        input = value.Get<Vector2>();   // valor de la acción Move
    }

    void FixedUpdate()
    {
        // Velocidad objetivo según el input
        Vector2 targetVelocity = input * moveSpeed;

        // Acelerar o frenar suavemente
        Vector2 currentVelocity = rb.linearVelocity;
        float rate = (input.sqrMagnitude > 0.01f) ? acceleration : deceleration;
        rb.linearVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);

        // Flip del sprite según la dirección horizontal
        if (input.x > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(baseScale.x), baseScale.y, baseScale.z);
        }
        else if (input.x < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(baseScale.x), baseScale.y, baseScale.z);
        }

        // Pasar velocidad al Animator para animaciones de caminar/idle
        if (animator != null)
        {
            float speed = rb.linearVelocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
    }
}