using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private float rotationSpeed = 1f;

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        PlayerActions rotateAction = new PlayerActions();
        rotateAction.Movement.Rotate.performed += OnRotateAction;
        rotateAction.Enable();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnRotateAction(InputAction.CallbackContext context)
    {
        // Get the input vector
        Vector2 input = context.ReadValue<Vector2>();
        Debug.Log(input);
        // Get the current camera state
        CinemachineStateDrivenCamera stateDrivenCamera = virtualCamera.gameObject.GetComponent<CinemachineStateDrivenCamera>();
        if (stateDrivenCamera != null)
        {
            if (stateDrivenCamera.IsBlending)
            {
                // Do not update the camera if there is an active blend
                return;
            }
        }

        // Rotate the virtual camera around the y-axis based on the input vector
        transform.Rotate(Vector3.up, input.x * rotationSpeed, Space.World);

        // Rotate the virtual camera around the x-axis based on the input vector
        transform.Rotate(Vector3.right, -input.y * rotationSpeed, Space.Self);
    }
}
