using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RTS.Player
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private Rigidbody cameraTarget;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private CameraConfig cameraConfig;

        private CinemachineFollow cinemachineFollow;
        // This is used to determine the furthest the camera can zoom out.
        private Vector3 startingFollowOffset;
        private float zoomStartTime;
        private float rotationStartTime;
        private float maxRotationAmount;

        private void Awake()
        {
            if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
            {
                Debug.LogError("CinemachineCamera does not have a CinemachineFollow component.");
                return;
            }

            startingFollowOffset = cinemachineFollow.FollowOffset;
            maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);
        }

        void Update()
        {
            HandlePanning();
            HandleZooming();
            HandleRotating();
        }

        private void HandlePanning()
        {
            Vector2 moveAmount = GetKeyboardMoveAmount();
            moveAmount += GetMouseMoveAmount();

            cameraTarget.linearVelocity = new Vector3(moveAmount.x, 0f, moveAmount.y);
        }

        private Vector2 GetKeyboardMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;

            // TODO: There's gotta be a better way. Is there a device-independent way to get input?
            if (Keyboard.current.upArrowKey.isPressed)
            {
                moveAmount.y += cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.downArrowKey.isPressed)
            {
                moveAmount.y -= cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveAmount.x += cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.leftArrowKey.isPressed)
            {
                moveAmount.x -= cameraConfig.KeyboardPanSpeed;
            }

            return moveAmount;
        }

        private Vector2 GetMouseMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;
            if (!cameraConfig.EnableEdgePan) { return moveAmount; }

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 screenDimensions = new Vector2(Screen.width, Screen.height);

            // X-Axis Edge Panning
            if (mousePosition.x <= cameraConfig.EdgePanSize)
            {
                moveAmount.x -= cameraConfig.MousePanSpeed;
            }            
            else if (mousePosition.x >= screenDimensions.x - cameraConfig.EdgePanSize)
            {
                moveAmount.x += cameraConfig.MousePanSpeed;
            }

            // Y-Axis Edge Panning
            if (mousePosition.y <= cameraConfig.EdgePanSize)
            {
                moveAmount.y -= cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.y >= screenDimensions.y - cameraConfig.EdgePanSize)
            {
                moveAmount.y += cameraConfig.MousePanSpeed;
            }

            return moveAmount;
        }

        private void HandleZooming()
        {
            if (ShouldSetZoomStartTime())
            {
                zoomStartTime = Time.time;
            }

            // We clamp here to be safe since we are using Time.time
            float zoomTime = Mathf.Clamp01((Time.time - zoomStartTime) * cameraConfig.ZoomSpeed);
            Vector3 targetFollowOffset;

            // If the end key is pressed, we zoom in from where we are towards the minimum distance.
            if (Keyboard.current.endKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    cinemachineFollow.FollowOffset.x,
                    cameraConfig.MinZoomDistance,
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

        private void HandleRotating()
        {
            if (ShouldSetRotationStartTime())
            {
                rotationStartTime = Time.time;
            }

            float rotationTime = Mathf.Clamp01((Time.time - rotationStartTime) * cameraConfig.RotationSpeed);
            Vector3 targetFollowOffset;
            if (Keyboard.current.pageDownKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    maxRotationAmount,
                    cinemachineFollow.FollowOffset.y,
                    0f
                );
            }
            else if (Keyboard.current.pageUpKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    -maxRotationAmount,
                    cinemachineFollow.FollowOffset.y,
                    0f
                );
            }
            else
            {
                targetFollowOffset = new Vector3(
                    startingFollowOffset.x,
                    cinemachineFollow.FollowOffset.y,
                    startingFollowOffset.z
                );
            }

            cinemachineFollow.FollowOffset = Vector3.Slerp(
                cinemachineFollow.FollowOffset,
                targetFollowOffset,
                rotationTime
            );
        }

        private static bool ShouldSetRotationStartTime() => Keyboard.current.pageUpKey.wasPressedThisFrame
            || Keyboard.current.pageDownKey.wasPressedThisFrame
            || Keyboard.current.pageUpKey.wasReleasedThisFrame
            || Keyboard.current.pageDownKey.wasReleasedThisFrame;
    }
}