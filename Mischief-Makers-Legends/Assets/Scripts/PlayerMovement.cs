using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private Vector3 moveDirection;
    private float lastDashTime = -Mathf.Infinity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        PlayerActions actions = new PlayerActions();

        actions.Movement.Movement.performed += OnMovementInput;
        actions.Movement.Movement.canceled += OnMovementInputCanceled;
        actions.Movement.Jump.performed += OnJumpInput;
        actions.Movement.Run.performed += OnDashInput;

        actions.Enable();
    }


    private void FixedUpdate()
    {
        Move();
    }
    private Vector3 previousMoveDirection;
    private void Move()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        if (moveDirection != Vector3.zero)
        {
            previousMoveDirection = moveDirection;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
        }
        else
        {
            Quaternion targetRotation = Quaternion.LookRotation(previousMoveDirection);
            transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
        }

        rb.velocity = new Vector3(moveDirection.x, rb.velocity.y, moveDirection.z);
    }

    public void OnMovementInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnMovementInputCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    public void OnDashInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (Time.time > lastDashTime + dashCooldown)
            {
                lastDashTime = Time.time;
                Vector3 dashDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
                Vector3 dashForce = dashDirection * dashDistance / dashDuration;
                rb.AddForce(dashForce, ForceMode.VelocityChange);
            }
        }
    }
}
