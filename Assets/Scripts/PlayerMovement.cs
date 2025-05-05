using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    private Vector2 moveInput;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Bu method, PlayerInput component'i üzerinde Move eylemine bağlanmalıdır
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        // Animator parametrelerini güncelle
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
        animator.SetBool("IsMoving", moveInput != Vector2.zero);
    }

    void Update()
    {
        // Karakteri hareket ettir
        Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0f) * speed * Time.deltaTime;
        transform.position += delta;
    }
}