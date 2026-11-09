using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class FreeCamera : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 10f;
    [SerializeField, Min(1f)] private float fastMultiplier = 3f;

    [Header("Look")]
    [SerializeField, Min(0f)] private float lookSensitivity = 0.15f;

    private float yaw;
    private float pitch;

    public bool IsCaptured => Cursor.lockState == CursorLockMode.Locked;

    public string MovementKeys
    {
        get
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return "Flèches";

            return $"{keyboard.wKey.displayName} · {keyboard.aKey.displayName} · " +
                   $"{keyboard.sKey.displayName} · {keyboard.dKey.displayName}";
        }
    }

    private void OnEnable()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = Mathf.DeltaAngle(0f, angles.x);
        ReleaseCursor();
    }

    private void OnDisable()
    {
        ReleaseCursor();
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
            ReleaseCursor();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (!Application.isFocused)
            return;

        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            ReleaseCursor();
            return;
        }

        if (!IsCaptured)
        {
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            return;
        }

        if (mouse != null)
            Look(mouse.delta.ReadValue());

        if (keyboard != null)
            Move(keyboard);
    }

    private void Look(Vector2 mouseDelta)
    {
        yaw = Mathf.Repeat(yaw + mouseDelta.x * lookSensitivity, 360f);
        pitch = Mathf.Clamp(pitch - mouseDelta.y * lookSensitivity, -89f, 89f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void Move(Keyboard keyboard)
    {
        // These controls represent physical key positions, regardless of the active layout.
        int forward = Axis(
            keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed,
            keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed);
        int sideways = Axis(
            keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed,
            keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed);
        int vertical = Axis(keyboard.spaceKey.isPressed, keyboard.ctrlKey.isPressed);

        // Only yaw affects travel on the ground plane. Space and Ctrl control altitude.
        Vector3 input = Vector3.ClampMagnitude(new Vector3(sideways, vertical, forward), 1f);
        Vector3 direction = Quaternion.Euler(0f, yaw, 0f) * input;
        float speed = moveSpeed;
        if (keyboard.shiftKey.isPressed)
            speed *= fastMultiplier;

        transform.position += direction * speed * Time.unscaledDeltaTime;
    }

    private static int Axis(bool positive, bool negative)
    {
        return (positive ? 1 : 0) - (negative ? 1 : 0);
    }

    private static void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
