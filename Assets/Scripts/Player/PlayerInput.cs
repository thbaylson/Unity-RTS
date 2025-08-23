using RTS.Commands;
using RTS.EventBus;
using RTS.Events;
using RTS.Units;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private ActionBase activeAction;
        private bool wasMouseDownOnUI;

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

            // UIActionButtons have their onClick listeners set to Raise an ActionSelectedEvent by ActionsUI.
            Bus<ActionSelectedEvent>.OnEvent += HandleActionSelected;
        }

        // Always unsubscribe from events to prevent memory leaks.
        private void OnDestroy()
        {
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitSpawnedEvent>.OnEvent -= HandleUnitSpawned;
            
            Bus<ActionSelectedEvent>.OnEvent -= HandleActionSelected;
        }

        private void HandleUnitSpawned(UnitSpawnedEvent evt) => aliveUnits.Add(evt.Unit);
        private void HandleUnitSelected(UnitSelectedEvent evt) => selectedUnits.Add(evt.Unit);
        private void HandleUnitDeselected(UnitDeselectedEvent evt) => selectedUnits.Remove(evt.Unit);
        private void HandleActionSelected(ActionSelectedEvent evt) => activeAction = evt.Action;

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

            wasMouseDownOnUI = EventSystem.current.IsPointerOverGameObject();
        }

        private void HandleMouseDrag()
        {
            if (activeAction != null || wasMouseDownOnUI) return;

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
            // If the active action is not null, then we probably want to perform that action, so don't clear the selected
            // units list. The second part enables shift-clicking to select multiple units.
            if (activeAction == null && !Keyboard.current.shiftKey.isPressed)
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
                    int unitIndex = 0;
                    foreach(AbstractUnit unit in selectedUnits.Where(x => x is AbstractUnit))
                    {
                        foreach (ICommand command in unit.AvailableCommands)
                        {
                            CommandContext ctx = new CommandContext(unit, hit, unitIndex++);
                            if (command.CanHandle(ctx))
                            {
                                command.Handle(ctx);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void HandleLeftClick()
        {
            if (camera == null) { Debug.LogError("Camera is not assigned in PlayerInput."); return; }

            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (activeAction == null
                && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayers)
                && hit.collider.TryGetComponent(out ISelectable selectable))
            {
                selectable.Select();
            }
            else if (activeAction != null
                && !EventSystem.current.IsPointerOverGameObject()
                && Physics.Raycast(cameraRay, out hit, float.MaxValue, floorLayers))
            {
                int unitIndex = 0;
                foreach (AbstractUnit unit in selectedUnits.Where(x => x is AbstractUnit))
                {
                    CommandContext ctx = new CommandContext(unit, hit, unitIndex++);
                    // The CanHandle method is being checked... somewhere else.
                    activeAction.Handle(ctx);
                }

                activeAction = null;
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