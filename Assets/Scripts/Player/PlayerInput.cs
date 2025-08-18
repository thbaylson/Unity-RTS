using RTS.Commands;
using RTS.EventBus;
using RTS.Events;
using RTS.Units;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

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

        // Keep track of alive units to be sure we don't select any dead units.
        // We're using HashSets here because the index/position/order of the units doesn't matter.
        private HashSet<AbstractUnit> aliveUnits = new(100);
        // addedUnits keeps track of units that are being selected during the drag event.
        private HashSet<AbstractUnit> addedUnits = new(24);
        // This is the actual list of selected units that we will use for actions, eg: movement.
        private List<ISelectable> selectedUnits = new(12);

        private void Awake()
        {
            if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
            {
                Debug.LogError("CinemachineCamera does not have a CinemachineFollow component.");
                return;
            }

            startingFollowOffset = cinemachineFollow.FollowOffset;
            maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);

            Bus<UnitSpawnedEvent>.OnEvent += HandleUnitSpawned;
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
        }

        // Always unsubscribe from events to prevent memory leaks.
        private void OnDestroy()
        {
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitSpawnedEvent>.OnEvent -= HandleUnitSpawned;
        }

        private void HandleUnitSpawned(UnitSpawnedEvent evt) => aliveUnits.Add(evt.Unit);
        private void HandleUnitSelected(UnitSelectedEvent evt) => selectedUnits.Add(evt.Unit);
        private void HandleUnitDeselected(UnitDeselectedEvent evt) => selectedUnits.Remove(evt.Unit);

        void Update()
        {
            HandlePanning();
            HandleZooming();
            HandleRotating();
            HandleRightClick();
            HandleDragSelect();
        }

        private void HandleDragSelect()
        {
            if (dragSelectBox == null) { Debug.LogError("Drag Select Box is not assigned in PlayerInput."); return; }

            // Drag Started
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleMouseDown();
            }
            // Dragging
            else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleMouseDrag();
            }
            // Drag Ended
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleMouseUp();
            }
        }

        private void HandleMouseDown()
        {
            dragSelectBox.sizeDelta = Vector2.zero;
            dragSelectBox.gameObject.SetActive(true);
            startingMousePosition = Mouse.current.position.ReadValue();
            addedUnits.Clear();
        }

        private void HandleMouseDrag()
        {
            // This Bounds object was calculated using the mouse position, so it'll be in screen space.
            Bounds selectionBoxBounds = ResizeSelectionBox();
            foreach (AbstractUnit unit in aliveUnits)
            {
                Vector2 unitPosition = camera.WorldToScreenPoint(unit.transform.position);
                if (selectionBoxBounds.Contains(unitPosition))
                {
                    addedUnits.Add(unit);
                }
            }
        }

        private void HandleMouseUp()
        {
            // This enables shift-clicking to select multiple units.
            if (!Keyboard.current.shiftKey.isPressed)
            {
                // Deselect units that are not in the drag box.
                DeselectAllUnits();
            }

            // This will cover cases where the player was not dragging, but just clicked.
            HandleLeftClick();

            // Select all units within the drag box.
            foreach (AbstractUnit unit in addedUnits)
            {
                unit.Select();
            }

            dragSelectBox.gameObject.SetActive(false);
        }

        private void DeselectAllUnits()
        {
            // Remember that Deselect() changes the selectedUnits list and we can't modify it while iterating over it.
            ISelectable[] currentlySelectedUnits = selectedUnits.ToArray();
            foreach (ISelectable selectable in currentlySelectedUnits)
            {
                selectable.Deselect();
            }
        }

        private Bounds ResizeSelectionBox()
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            Vector2 dragBoxSize = currentMousePosition - startingMousePosition;

            // We use Mathf.Abs to ensure the size is always positive, regardless of drag direction.
            dragSelectBox.sizeDelta = new Vector2(Mathf.Abs(dragBoxSize.x), Mathf.Abs(dragBoxSize.y));
            // We use anchoredPosition because this is a UI element. This is essentially like transform.position.
            // The reason we half the size is because the pivot point defaults to the center of the RectTransform.
            dragSelectBox.anchoredPosition = startingMousePosition + dragBoxSize / 2f;

            return new Bounds(dragSelectBox.anchoredPosition, dragSelectBox.sizeDelta);
        }

        private void HandleRightClick()
        {
            if (selectedUnits.Count == 0) { return; }

            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers))
                {
                    // When we have multiple units moving to the same location, we need to prevent
                    // the units from bumping into each other and pushing each other around, ie "Unit Dancing."
                    // To do this, we're going to make concentric rings with radi based on how many units
                    // need to be placed. We'll use degrees on the Unit Circle to figure out how to equally space 
                    // the units on a given layer.
                    List<AbstractUnit> abstractUnits = new(selectedUnits.Count);
                    foreach(ISelectable selectable in selectedUnits)
                    {
                        if(selectable is AbstractUnit unit)
                        {
                            abstractUnits.Add(unit);
                        }
                    }

                    int unitsOnLayer = 0;
                    int maxUnitsOnLayer = 1;
                    float circleRadius = 0f;
                    float radialOffset = 0f;

                    foreach(AbstractUnit unit in abstractUnits)
                    {
                        foreach(ICommand command in unit.AvailableCommands)
                        {
                            if(command.CanHandle(unit, hit))
                            {
                                command.Handle(unit, hit);
                            }
                        }

                        //// Find where the unit should be on its layer of the concentric circles.
                        //Vector3 targetPosition = new(
                        //    hit.point.x + circleRadius * Mathf.Cos(radialOffset * unitsOnLayer),
                        //    hit.point.y,
                        //    hit.point.z + circleRadius * Mathf.Sin(radialOffset * unitsOnLayer)
                        //);

                        //unit.MoveTo(targetPosition);
                        //unitsOnLayer++;

                        //if(unitsOnLayer > maxUnitsOnLayer)
                        //{
                        //    // Once we fill up a layer, calculate the next layer's radius and reset the count.
                        //    circleRadius += unit.AgentRadius * 3.5f;// TODO: Make this layer spacing offset a variable.
                        //    unitsOnLayer = 0;
                            
                        //    // The circumfrence of the circle divided by (the radius of our units * unit spacing offset).
                        //    maxUnitsOnLayer = Mathf.FloorToInt(2 * Mathf.PI * circleRadius / (unit.AgentRadius * 2));
                        //    // 2 * Mathf.PI is the radians of a full circle, then divide by max units to get the radial offset.
                        //    // TODO: Units on the last layer are not spaced evenly. This formula assumes we'll always have the max number
                        //    // on each layer. We could space them out better if we could predict how many units will be on the last layer.
                        //    radialOffset = 2 * Mathf.PI / maxUnitsOnLayer;
                        //}
                    }
                }
            }
        }

        private void HandleLeftClick()
        {
            if (camera == null) { Debug.LogError("Camera is not assigned in PlayerInput."); return; }

            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayers)
                && hit.collider.TryGetComponent(out ISelectable selectable))
            {
                selectable.Select();
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