using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float airHoverSpeed = 15f;
    public float backwardSpeedRatio = 0.5f;
    public float dashCooldown = 0.5f;
    public float dashDistance = 5f;

    // Cooldown between dashes
    public float dashDistanceMultiplier = 2f;

    public float dashDuration = 0.2f;
    public float dashForce = 20f; // Force applied during the dash

    // Duration of the dash Multiplier to increase the dash distance based on current momentum
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
    public float maxVerticalDashVelocity = 5f;
    public float moveSpeed = 10f;

    //public Transform playerModel;
    public float rotationSpeed = 360f;

    public float shakeAngle = 15f;
    public float shakeDuration = 1f;
    public float shakeSpeed = 50f;
    public float throwForce = 10f;
    public float turnSpeed = 10f;

    [SerializeField]
    private float angularDamping = 5.0f;

    private float currentFuel;
    private GameObject grabbedObject;
    private bool isDashing = false; // Is the player currently dashing
    private bool isHovering = false;
    private Vector2 moveInput;
    private Rigidbody rb;
    private float timeSinceLastDash; // Time since the last dash
    private float timeSinceLastHover = 0f;
    private float timeSinceLastJump;

    public void OnDashInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector3 horizontalDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            float verticalDirection = Mathf.Min(rb.velocity.y, maxVerticalDashVelocity);
            verticalDirection = verticalDirection > 0f ? verticalDirection : 0f;
            Vector3 dashDirection = horizontalDirection + new Vector3(0f, verticalDirection, 0f);
            StartCoroutine(Dash(dashDirection.normalized * dashDistance));
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
        //_cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        rb = GetComponent<Rigidbody>();
        currentFuel = maxFuel;
    }

    private IEnumerator Dash(Vector3 dashVelocity)
    {
        if (isDashing || Time.time - timeSinceLastDash < dashCooldown) yield break;

        isDashing = true;
        timeSinceLastDash = Time.time;

        float dashEndTime = Time.time + dashDuration;
        rb.AddForce(dashVelocity, ForceMode.VelocityChange);

        while (Time.time < dashEndTime)
        {
            yield return null;
        }

        isDashing = false;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void GrabObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(rb.position, rb.transform.forward, out hit, grabDistance) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Grabbable"))
        {
            grabbedObject = hit.collider.gameObject;
            grabbedObject.transform.SetParent(rb.transform);
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
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        rb.angularDrag = angularDamping;

        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        Vector3 desiredDirection = cameraForward * moveDirection.z + cameraRight * moveDirection.x;
        rb.velocity = desiredDirection * moveSpeed;

        // Update the rotation
        if (desiredDirection.magnitude > 0.1f)
        {
            var angle = Vector3.SignedAngle(rb.rotation * Vector3.forward, desiredDirection, Vector3.up) * Mathf.Deg2Rad / Time.deltaTime;
            rb.angularVelocity = Vector3.up * angle;
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
        rbObj.velocity = rb.transform.forward * throwForce;
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
        //Rotate();
        if (IsGrounded())
        {
            GetComponent<Renderer>().material.color = Color.green;
            RegenerateFuel();
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
    }
}