using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimulationSpeed : MonoBehaviour
{
    private float previousTimeScale;
    private float pendingTextSpeed;
    private Keyboard keyboard;

    private void OnEnable()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        ListenTo(Keyboard.current);
    }

    private void OnDisable()
    {
        ListenTo(null);
        Time.timeScale = previousTimeScale;
    }

    private void Update()
    {
        ListenTo(Keyboard.current);
        if (keyboard == null || !Application.isFocused)
        {
            pendingTextSpeed = 0f;
            return;
        }

        if (pendingTextSpeed > 0f)
        {
            Time.timeScale = pendingTextSpeed;
            pendingTextSpeed = 0f;
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            Time.timeScale = 1f;
        else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            Time.timeScale = 2f;
        else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            Time.timeScale = 4f;
        else if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame)
            Time.timeScale = 8f;
    }

    private void ListenTo(Keyboard current)
    {
        if (keyboard == current)
            return;

        if (keyboard != null)
            keyboard.onTextInput -= OnTextInput;

        pendingTextSpeed = 0f;
        keyboard = current;
        if (keyboard != null)
            keyboard.onTextInput += OnTextInput;
    }

    private void OnTextInput(char character)
    {
        int index = GetSpeedKeyIndex(character);
        if (index >= 0)
            pendingTextSpeed = 1 << index;
    }

    private int GetSpeedKeyIndex(char character)
    {
        if (character >= '1' && character <= '4')
            return character - '1';

        // Prefer the active layout's own labels. macOS may deliver text without
        // a corresponding Digit key press.
        if (Matches(character, keyboard.digit1Key.displayName)) return 0;
        if (Matches(character, keyboard.digit2Key.displayName)) return 1;
        if (Matches(character, keyboard.digit3Key.displayName)) return 2;
        if (Matches(character, keyboard.digit4Key.displayName)) return 3;

        // Some layouts report generic digit labels instead of the characters.
        string layout = keyboard.keyboardLayout ?? string.Empty;
        if (Contains(layout, "bepo") || Contains(layout, "bépo") ||
            (keyboard.digit2Key.displayName == "«" && keyboard.digit3Key.displayName == "»"))
            return "\"«»(".IndexOf(character);

        bool isAzerty = Contains(layout, "azerty") ||
                        string.Equals(keyboard.aKey.displayName, "Q", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(keyboard.wKey.displayName, "Z", StringComparison.OrdinalIgnoreCase) ||
                        IsFrenchMacLayout(layout);
        return isAzerty ? "&é\"'".IndexOf(character == '’' ? '\'' : character) : -1;
    }

    private static bool Matches(char character, string label)
    {
        return label != null && label.Length == 1 && label[0] == character;
    }

    private static bool Contains(string layout, string fragment)
    {
        return layout.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsFrenchMacLayout(string layout)
    {
        const string applePrefix = "com.apple.keylayout.";
        if (layout.StartsWith(applePrefix, StringComparison.OrdinalIgnoreCase))
            layout = layout.Substring(applePrefix.Length);

        return string.Equals(layout, "French", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(layout, "Français", StringComparison.OrdinalIgnoreCase) ||
               layout.StartsWith("French - ", StringComparison.OrdinalIgnoreCase) ||
               layout.StartsWith("Français - ", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(layout, "French-PC", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(layout, "FrenchNumerical", StringComparison.OrdinalIgnoreCase);
    }
}
