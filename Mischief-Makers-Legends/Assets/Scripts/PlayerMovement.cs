using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float backwardSpeedRatio = 0.5f;
    [SerializeField] private float groundDashSpeed = 20f;
    [SerializeField] private float airDashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float grabDistance = 2f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float shakeDuration = 1f;
    [SerializeField] private float shakeAngle = 15f;
    [SerializeField] private float shakeSpeed = 50f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, -1, 0);
    [SerializeField] private Transform playerModel;

    private Rigidbody rb;
    private Vector2 moveInput;


    private bool isDashing = false;
    private float timeSinceLastDash = 0f;
    private GameObject grabbedObject;

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

    private void Update()
    {
        if (IsGrounded())
        {
            playerModel.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            playerModel.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);

        rb.velocity = new Vector3(movement.x * moveSpeed, rb.velocity.y, movement.z * moveSpeed);

        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            playerModel.rotation = Quaternion.RotateTowards(playerModel.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

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
        Rigidbody rbObj = grabbedObject.GetComponent<Rigidbody>();
        rbObj.isKinematic = false;
        rbObj.velocity = playerModel.forward * throwForce;
        grabbedObject = null;
    }

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

    private bool IsGrounded()
    {
        Vector3 checkPosition = transform.position + groundCheckOffset;
        return Physics.CheckSphere(checkPosition, groundCheckDistance, groundLayer);
    }

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
            StartCoroutine(Dash(dashDirection));
        }
    }
}