using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float keyboardPanSpeed = 5f;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float minZoomDistance = 7.5f;

    private CinemachineFollow cinemachineFollow;
    // This is used to determine the furthest the camera can zoom out.
    private Vector3 startingFollowOffset;
    private float zoomStartTime;

    private void Awake()
    {
        if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
        {
            Debug.LogError("CinemachineCamera does not have a CinemachineFollow component.");
            return;
        }

        startingFollowOffset = cinemachineFollow.FollowOffset;
    }

    void Update()
    {
        HandlePanning();
        HandleZooming();
    }

    private void HandleZooming()
    {
        if (ShouldSetZoomStartTime())
        {
            zoomStartTime = Time.time;
        }

        // We clamp here to be safe since we are using Time.time
        float zoomTime = Mathf.Clamp01((Time.time - zoomStartTime) * zoomSpeed);
        Vector3 targetFollowOffset;

        // If the end key is pressed, we zoom in from where we are towards the minimum distance.
        if (Keyboard.current.endKey.isPressed)
        {
            targetFollowOffset = new Vector3(
                cinemachineFollow.FollowOffset.x,
                minZoomDistance,
                cinemachineFollow.FollowOffset.z
            );
        }
        // If the end key is released, we zoom out from where we are towards the starting offset.
        else
        {
            targetFollowOffset = new Vector3(
                cinemachineFollow.FollowOffset.x,
                startingFollowOffset.y,
                cinemachineFollow.FollowOffset.z
            );
        }

        // Slerp = Spherical Linear Interpolation. Gives a more natural zooming effect.
        cinemachineFollow.FollowOffset = Vector3.Slerp(
            cinemachineFollow.FollowOffset,
            targetFollowOffset,
            zoomTime
        );
    }

    private static bool ShouldSetZoomStartTime() => Keyboard.current.endKey.wasPressedThisFrame
        || Keyboard.current.endKey.wasReleasedThisFrame;

    private void HandlePanning()
    {
        Vector2 moveAmount = Vector2.zero;
        // TODO: There's gotta be a better way. Is there a device-independent way to get input?
        if (Keyboard.current.upArrowKey.isPressed)
        {
            moveAmount.y += keyboardPanSpeed;
        }
        if (Keyboard.current.downArrowKey.isPressed)
        {
            moveAmount.y -= keyboardPanSpeed;
        }
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            moveAmount.x += keyboardPanSpeed;
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            moveAmount.x -= keyboardPanSpeed;
        }

        if (moveAmount != Vector2.zero)
        {
            cameraTarget.position += new Vector3(moveAmount.x, 0f, moveAmount.y) * Time.deltaTime;
        }
    }
}
