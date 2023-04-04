using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float airHoverSpeed = 15f;
    public float backwardSpeedRatio = 0.5f;
    public float dashForce = 20f; // Force applied during the dash
    public float dashDuration = 0.2f; // Duration of the dash
    public float dashCooldown = 0.5f; // Cooldown between dashes
    public float dashDistanceMultiplier = 2f; // Multiplier to increase the dash distance based on current momentum
    public float fuelConsumptionRate = 1f; // Fuel consumption rate per second
    public float fuelRegenerationRate = 0.5f; // Fuel regeneration rate per second
    public float grabDistance = 2f;
    public float groundCheckDistance = 0.2f;
    public Vector3 groundCheckOffset = new Vector3(0, -1, 0);
    public float groundHoverSpeed = 20f;
    public LayerMask groundLayer;
    public float hoverDuration = 0.1f;
    public float hoverForce = 5f;
    public float jumpForce = 10f;
    public float jumpHoverDelay = 0.5f;
    public float maxFuel = 3f; // Maximum fuel for the jetpack
    public float moveSpeed = 10f;
    public Transform playerModel;
    public float rotationSpeed = 360f;
    public float shakeAngle = 15f;
    public float shakeDuration = 1f;
    public float shakeSpeed = 50f;
    public float throwForce = 10f;
    public float turnSpeed = 10f;

    private bool isDashing = false; // Is the player currently dashing
    private float currentFuel;
    private GameObject grabbedObject;
    private bool isHovering = false;
    private Vector2 moveInput;
    private Rigidbody rb;
    private float timeSinceLastDash; // Time since the last dash
    private float timeSinceLastHover = 0f;
    private float timeSinceLastJump;

    private IEnumerator Dash(Vector3 dashDirection)
    {
        if (isDashing || Time.time - timeSinceLastDash < dashCooldown) yield break;

        isDashing = true;
        timeSinceLastDash = Time.time;

        float dashEndTime = Time.time + dashDuration;
        rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange);

        while (Time.time < dashEndTime)
        {
            yield return null;
        }

        isDashing = false;
    }

    public void OnDashInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector3 dashDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            StartCoroutine(Dash(dashDirection));
        }
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (IsGrounded())
            {
                Jump();
            }
            else
            {
                if (Time.time - timeSinceLastJump >= jumpHoverDelay)
                {
                    StartCoroutine(Hover());
                }
            }
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentFuel = maxFuel;
    }

    private void FixedUpdate()
    {
        Move();
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

    private IEnumerator Hover()
    {
        if (isHovering || Time.time - timeSinceLastJump < jumpHoverDelay || currentFuel <= 0) yield break;

        isHovering = true;
        timeSinceLastHover = Time.time;

        float hoverEndTime = Time.time + hoverDuration;
        currentFuel -= fuelConsumptionRate * hoverDuration;

        while (Time.time < hoverEndTime)
        {
            rb.AddForce(new Vector3(0f, hoverForce, 0f), ForceMode.Acceleration);
            yield return null;
        }

        isHovering = false;
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
            timeSinceLastJump = Time.time;
        }
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

    private void RegenerateFuel()
    {
        if (!isHovering && currentFuel < maxFuel)
        {
            currentFuel += fuelRegenerationRate * Time.deltaTime;
            currentFuel = Mathf.Clamp(currentFuel, 0, maxFuel);
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
            RegenerateFuel();
        }
        else
        {
            playerModel.GetComponent<Renderer>().material.color = Color.red;
        }
    }
}