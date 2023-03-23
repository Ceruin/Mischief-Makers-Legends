using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float cameraSensitivity = 10f;

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        PlayerActions playerActions = new PlayerActions();
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

        // Multiply the input by the camera sensitivity
        input *= cameraSensitivity;

        // Rotate the camera horizontally and vertically around the player
        var brain = virtualCamera;
        if (brain != null)
        {
            var vcam = brain.VirtualCameraGameObject.GetComponent<CinemachineFreeLook>();
            if (vcam != null)
            {
                // Rotate the camera horizontally and vertically around the player
                vcam.m_XAxis.Value += input.x * rotationSpeed;
                vcam.m_YAxis.Value += input.y * rotationSpeed;
            }
        }
    }
}
