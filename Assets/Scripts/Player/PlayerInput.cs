using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float keyboardPanSpeed = 5f;

    void Update()
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
