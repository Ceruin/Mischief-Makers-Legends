using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public float moveSpeed = 10f;
    public float rotationSpeed = 360f;
    public Transform playerModel;
    private Vector2 moveInput;
    private Rigidbody rb;
    private Vector2 movementInput;
    private Vector3 movement;

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

    public float turnSpeed = 10f; // the speed of turning left/right
    public float backwardSpeedRatio = 0.5f; // the speed ratio of moving backwards compared to forwards

    private void Move()
    {
        movement = new Vector3(moveInput.x, 0f, moveInput.y);

        // Move the player using Rigidbody
        rb.velocity = new Vector3(movement.x * moveSpeed, rb.velocity.y, movement.z * moveSpeed);

        // Rotate the player model based on movement direction
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            playerModel.rotation = Quaternion.RotateTowards(playerModel.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    public float groundDashSpeed = 20f;
    public float airDashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;

    private bool isDashing = false;
    private float timeSinceLastDash = 0f;

    private IEnumerator Dash(Vector3 dashDirection)
    {
        if (isDashing || Time.time - timeSinceLastDash < dashCooldown) yield break;

        isDashing = true;
        timeSinceLastDash = Time.time;

        float dashEndTime = Time.time + dashDuration;
        float currentDashSpeed = IsGrounded() ? groundDashSpeed : airDashSpeed;

        while (Time.time < dashEndTime)
        {
            rb.velocity = new Vector3(dashDirection.x * currentDashSpeed, dashDirection.y * currentDashSpeed, dashDirection.z * currentDashSpeed);
            yield return null;
        }

        isDashing = false;
    }

    public float grabDistance = 2f;
    public float throwForce = 10f;

    private GameObject grabbedObject;

    private void GrabObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerModel.position, playerModel.forward, out hit, grabDistance) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Grabbable"))
        {
            grabbedObject = hit.collider.gameObject;
            grabbedObject.transform.SetParent(playerModel);
            grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void ReleaseObject()
    {
        if (grabbedObject == null) return;

        grabbedObject.transform.SetParent(null);
        var rbObj = grabbedObject.GetComponent<Rigidbody>();
        rbObj.isKinematic = false;
        rbObj.velocity = playerModel.forward * throwForce;
        grabbedObject = null;
    }

    public float shakeDuration = 1f;
    public float shakeAngle = 15f;
    public float shakeSpeed = 50f;

    private IEnumerator ShakeObject()
    {
        if (grabbedObject == null) yield break;

        float shakeEndTime = Time.time + shakeDuration;
        float direction = 1;
        while (Time.time < shakeEndTime)
        {
            grabbedObject.transform.Rotate(0, 0, shakeAngle * direction * Time.deltaTime * shakeSpeed);
            direction = -direction;
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void OnMovementInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnMovementInputCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    public LayerMask groundLayer;

    public float groundCheckDistance = 0.2f;
    public Vector3 groundCheckOffset = new Vector3(0, -1, 0);

    private bool IsGrounded()
    {
        Vector3 checkPosition = transform.position + groundCheckOffset;
        return Physics.CheckSphere(checkPosition, groundCheckDistance, groundLayer);
    }


    public float jumpForce = 10f;

    private void Jump()
    {
        if (IsGrounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Jump();
        }
    }

    public void OnDashInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector3 dashDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            Dash(dashDirection);
        }
    }
}