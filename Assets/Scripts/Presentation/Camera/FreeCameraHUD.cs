using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(FreeCamera))]
public class FreeCameraHUD : MonoBehaviour
{
    private FreeCamera cameraControls;
    private GUIStyle headingStyle;
    private GUIStyle textStyle;
    private bool showHelp = true;

    private void Awake()
    {
        cameraControls = GetComponent<FreeCamera>();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            showHelp = !showHelp;
    }

    private void OnGUI()
    {
        if (Event.current.type != EventType.Repaint || !cameraControls.isActiveAndEnabled)
            return;

        if (!showHelp && cameraControls.IsCaptured)
            return;

        if (headingStyle == null)
            CreateStyles();

        float width = Mathf.Min(392f, Screen.width - 24f);
        Rect panel = new Rect(12f, 12f, width, showHelp ? 122f : 46f);
        Color previousColor = GUI.color;

        GUI.color = new Color(0.08f, 0.11f, 0.13f, 0.92f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = new Color(0.58f, 0.85f, 0.66f);
        GUI.DrawTexture(new Rect(panel.x, panel.y, 3f, panel.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        string title = cameraControls.IsCaptured ? "Caméra libre" : "Cliquer dans la vue pour piloter";
        float x = panel.x + 14f;
        float contentWidth = panel.width - 26f;
        GUI.Label(new Rect(x, panel.y + 9f, contentWidth, 27f), title, headingStyle);

        if (showHelp)
        {
            GUI.Label(new Rect(x, panel.y + 38f, contentWidth, 22f),
                $"{cameraControls.MovementKeys} ou flèches   Déplacement", textStyle);
            GUI.Label(new Rect(x, panel.y + 63f, contentWidth, 22f),
                "Espace / Ctrl   Monter / descendre     Maj   Accélérer", textStyle);
            GUI.Label(new Rect(x, panel.y + 88f, contentWidth, 22f),
                "Souris   Regarder     Échap   Libérer     F1   Aide", textStyle);
        }

        GUI.color = previousColor;
    }

    private void CreateStyles()
    {
        headingStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold };
        headingStyle.normal.textColor = Color.white;
        textStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
        textStyle.normal.textColor = new Color(0.86f, 0.9f, 0.89f);
    }
}
