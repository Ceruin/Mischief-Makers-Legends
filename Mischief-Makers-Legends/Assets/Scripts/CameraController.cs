using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private float rotationSpeed = 1f;
    public float cameraSensitivity = 10f;

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        PlayerActions rotateAction = new PlayerActions();
        rotateAction.Movement.Rotate.performed += OnRotateAction;
        rotateAction.Enable();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //virtualCamera.transform.rotation = virtualCamera.target.rotation;
    }

    private void OnRotateAction(InputAction.CallbackContext context)
    {
        //// Get the input vector
        //Vector2 input = context.ReadValue<Vector2>();
        //Debug.Log(input);

        //input = input * cameraSensitivity;

        //transform.Rotate(Vector3.up, input.x, Space.World);
        //transform.Rotate(Vector3.right, input.y, Space.World);
    }
}
