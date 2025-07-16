using RTS.EventBus;
using RTS.Events;
using RTS.Units;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RTS.Player
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private Rigidbody cameraTarget;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        // The inherited member that we're hiding here is planned to be deprecated in the future.
        [SerializeField] private new Camera camera;
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private LayerMask selectableUnitsLayers;
        [SerializeField] private LayerMask floorLayers;
        [SerializeField] private RectTransform dragSelectBox;

        private Vector2 startingMousePosition;

        private CinemachineFollow cinemachineFollow;
        // This is used to determine the furthest the camera can zoom out.
        private Vector3 startingFollowOffset;
        private float zoomStartTime;
        private float rotationStartTime;
        private float maxRotationAmount;
        private ISelectable selectedUnit;

        private void Awake()
        {
            if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
            {
                Debug.LogError("CinemachineCamera does not have a CinemachineFollow component.");
                return;
            }

            startingFollowOffset = cinemachineFollow.FollowOffset;
            maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);

            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
        }

        // Always unsubscribe from events to prevent memory leaks.
        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
        }

        private void HandleUnitSelected(UnitSelectedEvent evt)
        {
            selectedUnit = evt.Unit;
        }

        private void HandleUnitDeselected(UnitDeselectedEvent evt)
        {
            selectedUnit = null;
        }

        void Update()
        {
            HandlePanning();
            HandleZooming();
            HandleRotating();
            HandleLeftClick();
            HandleRightClick();
            HandleDragSelect();
        }

        private void HandleDragSelect()
        {
            if (dragSelectBox == null) { Debug.LogError("Drag Select Box is not assigned in PlayerInput."); return; }

            // Drag Started
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                dragSelectBox.gameObject.SetActive(true);
                startingMousePosition = Mouse.current.position.ReadValue();

                // Note: This works because we anchored the Image to the bottom left of the screen.
                dragSelectBox.transform.position = startingMousePosition;
            }
            // Dragging
            else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame)
            {
                ResizeSelectionBox();
            }
            // Drag Ended
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                // Select all units within the drag box.
                // Deselect units that are not in the drag box.

                dragSelectBox.gameObject.SetActive(false);
            }
        }

        private void ResizeSelectionBox()
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            Vector2 dragBoxSize = currentMousePosition - startingMousePosition;

            // We use Mathf.Abs to ensure the size is always positive, regardless of drag direction.
            dragSelectBox.sizeDelta = new Vector2(Mathf.Abs(dragBoxSize.x), Mathf.Abs(dragBoxSize.y));
            // We use anchoredPosition because this is a UI element. This is essentially like transform.position.
            // The reason we half the size is because the pivot point defaults to the center of the RectTransform.
            dragSelectBox.anchoredPosition = startingMousePosition + dragBoxSize / 2f;
        }

        private void HandleRightClick()
        {
            // This lets us check if selectedUnit is IMoveable and declare a local variable at the same time.
            // For more, see https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns#declaration-and-type-patterns
            if (selectedUnit == null || selectedUnit is not IMoveable moveable) { return; }

            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers))
                {
                    moveable.MoveTo(hit.point);
                }
            }
        }

        private void HandleLeftClick()
        {
            if (camera == null) { Debug.LogError("Camera is not assigned in PlayerInput."); return; }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (selectedUnit != null)
                {
                    selectedUnit.Deselect();
                }

                Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayers)
                && hit.collider.TryGetComponent(out ISelectable selectable))
                {
                    selectable.Select();
                }
            }
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