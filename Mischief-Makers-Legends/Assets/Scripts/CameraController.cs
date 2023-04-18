using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using static UnityEngine.InputSystem.DefaultInputActions;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform player;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float cameraSensitivity = 10f;

    private PlayerActions playerActions;

    private void Awake()
    {
        playerActions = new PlayerActions();
        playerActions.Movement.Rotate.performed += OnRotateAction;
        playerActions.Enable();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // No need to do anything here
    }

    private void OnRotateAction(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        Debug.Log($"OnRotateAction: input: {input}");

        // Multiply the input by the camera sensitivity
        input *= cameraSensitivity;

        if (virtualCamera != null && player != null)
        {
            Debug.Log("OnRotateAction: virtualCamera and player found");

            player.transform.rotation *= Quaternion.AngleAxis(input.y * rotationSpeed, Vector3.right);
            var angles = player.transform.localEulerAngles;
            angles.z = 0;
            var angle = player.transform.localEulerAngles.x;
            if (angle > 180 && angle < 340)
            {
                angles.x = 340;
            }
            else if (angle < 180 && angle > 40)
            {
                angles.x = 40;
            }

            player.transform.localEulerAngles = angles;
        }
        else
        {
            Debug.LogError("OnRotateAction: virtualCamera or player not found");
        }
    }

    private void OnDestroy()
    {
        playerActions.Disable();
    }
}