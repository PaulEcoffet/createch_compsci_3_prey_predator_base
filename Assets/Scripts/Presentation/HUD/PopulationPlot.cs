using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PopulationPlot : MonoBehaviour
{
    [SerializeField, Min(5f)] private float windowSeconds = 90f;
    [SerializeField] private bool visible = true;

    private static readonly Color SheepColor = new Color(0.58f, 0.79f, 1f);
    private static readonly Color WolfColor = new Color(1f, 0.68f, 0.47f);
    private static readonly Color GrassColor = new Color(0.60f, 0.87f, 0.62f);
    private static readonly Color MutedColor = new Color(0.65f, 0.72f, 0.73f);

    private readonly List<Sample> samples = new List<Sample>();
    private GUIStyle titleStyle;
    private GUIStyle labelStyle;
    private GUIStyle axisStyle;
    private GUIStyle grassAxisStyle;

    private readonly struct Sample
    {
        public readonly double Time;
        public readonly int Sheep;
        public readonly int Wolves;
        public readonly int Grass;

        public Sample(double time, int sheep, int wolves, int grass)
        {
            Time = time;
            Sheep = Mathf.Max(0, sheep);
            Wolves = Mathf.Max(0, wolves);
            Grass = Mathf.Max(0, grass);
        }
    }

    /// <summary>Records one sample of all three populations at the current simulation time.</summary>
    public void RecordPopulation(int sheep, int wolves, int grass)
    {
        double now = Time.timeAsDouble;
        Sample sample = new Sample(now, sheep, wolves, grass);

        int last = samples.Count - 1;
        if (last >= 0 && samples[last].Time == now)
            samples[last] = sample;
        else
            samples.Add(sample);

        TrimHistory(now - windowSeconds);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.f2Key.wasPressedThisFrame)
            visible = !visible;

        TrimHistory(Time.timeAsDouble - windowSeconds);
    }

    private void TrimHistory(double start)
    {
        int expired = 0;
        // Keep one preceding point to draw a line up to the left edge.
        while (expired + 1 < samples.Count && samples[expired + 1].Time < start)
            expired++;

        if (expired > 0)
            samples.RemoveRange(0, expired);
    }

    private void OnGUI()
    {
        if (Event.current.type != EventType.Repaint)
            return;

        if (titleStyle == null)
        {
            titleStyle = MakeStyle(15, Color.white, FontStyle.Bold);
            labelStyle = MakeStyle(12, Color.white);
            axisStyle = MakeStyle(11, MutedColor);
            grassAxisStyle = MakeStyle(11, GrassColor);
        }

        string speedLabel = $"×{Time.timeScale:0.##}";

        if (!visible)
        {
            Rect badge = new Rect(Screen.width - 182f, Screen.height - 42f, 170f, 30f);
            Fill(badge, new Color(0.08f, 0.11f, 0.13f, 0.94f));
            GUI.Label(new Rect(badge.x + 9f, badge.y + 4f, 154f, 22f),
                $"F2  ·  Populations     {speedLabel}", labelStyle);
            return;
        }

        float width = Mathf.Min(450f, Screen.width - 24f);
        float height = Mathf.Min(238f, Screen.height - 24f);
        if (width < 220f || height < 120f)
            return;

        Rect panel = new Rect(Screen.width - width - 12f, Screen.height - height - 12f, width, height);
        Rect plot = new Rect(panel.x + 48f, panel.y + 60f, panel.width - 96f, panel.height - 88f);
        Fill(panel, new Color(0.08f, 0.11f, 0.13f, 0.96f));
        GUI.Label(new Rect(panel.x + 14f, panel.y + 8f, 155f, 24f), $"Populations  ·  {speedLabel}", titleStyle);
        GUI.Label(new Rect(panel.xMax - 96f, panel.y + 10f, 85f, 22f), "F2  ·  Masquer", axisStyle);

        double now = Time.timeAsDouble;
        double start = now - windowSeconds;
        bool hasData = samples.Count > 0 && samples[samples.Count - 1].Time >= start;
        Sample latest = hasData ? samples[samples.Count - 1] : default;
        DrawLegend(panel, hasData, latest);

        (long animalCeiling, long grassCeiling) = hasData ? GetAxisCeilings() : (4L, 4L);
        DrawGrid(plot, animalCeiling, grassCeiling, hasData);

        if (!hasData)
        {
            string message = samples.Count == 0 ? "En attente du premier relevé" : "Aucun relevé récent";
            GUI.Label(new Rect(plot.x + 10f, plot.center.y - 11f, plot.width - 20f, 22f), message, labelStyle);
            return;
        }

        DrawSeries(plot, start, animalCeiling, grassCeiling);
    }

    private void DrawLegend(Rect panel, bool hasData, Sample latest)
    {
        float x = panel.x + 14f;
        float width = (panel.width - 28f) / 3f;
        DrawLegendItem(x, panel.y + 34f, width, "Moutons", hasData, latest.Sheep, SheepColor);
        DrawLegendItem(x + width, panel.y + 34f, width, "Loups", hasData, latest.Wolves, WolfColor);
        DrawLegendItem(x + 2f * width, panel.y + 34f, width, "Herbe", hasData, latest.Grass, GrassColor);
    }

    private void DrawLegendItem(float x, float y, float width, string name, bool hasData, int count, Color color)
    {
        Fill(new Rect(x, y + 7f, 6f, 6f), color);
        string value = hasData ? count.ToString() : "–";
        GUI.Label(new Rect(x + 9f, y, width - 9f, 21f), $"{name} {value}", labelStyle);
    }

    private (long animals, long grass) GetAxisCeilings()
    {
        int animals = 4;
        int grass = 4;
        foreach (Sample sample in samples)
        {
            animals = Mathf.Max(animals, Mathf.Max(sample.Sheep, sample.Wolves));
            grass = Mathf.Max(grass, sample.Grass);
        }

        // Round each scale up to four evenly spaced integer ticks.
        return (((animals + 3L) / 4) * 4, ((grass + 3L) / 4) * 4);
    }

    private void DrawGrid(Rect plot, long animalCeiling, long grassCeiling, bool hasData)
    {
        Fill(plot, new Color(0.045f, 0.067f, 0.075f));
        Color gridColor = new Color(0.25f, 0.31f, 0.32f);
        for (int tick = 0; tick <= 4; tick++)
        {
            float fraction = tick / 4f;
            float y = Mathf.Lerp(plot.yMax, plot.yMin, fraction);
            Fill(new Rect(plot.x, y, plot.width, 1f), gridColor);
            if (hasData)
            {
                GUI.Label(new Rect(plot.x - 42f, y - 9f, 39f, 18f),
                    (animalCeiling / 4 * tick).ToString(), axisStyle);
                GUI.Label(new Rect(plot.xMax + 5f, y - 9f, 39f, 18f),
                    (grassCeiling / 4 * tick).ToString(), grassAxisStyle);
            }
        }

        GUI.Label(new Rect(plot.x, plot.yMax + 4f, 68f, 19f), $"−{windowSeconds:0.#} s", axisStyle);
        GUI.Label(new Rect(plot.center.x - 25f, plot.yMax + 4f, 61f, 19f), $"−{windowSeconds / 2f:0.#} s", axisStyle);
        GUI.Label(new Rect(plot.xMax - 22f, plot.yMax + 4f, 25f, 19f), "0 s", axisStyle);
    }

    private void DrawSeries(Rect plot, double start, long animalCeiling, long grassCeiling)
    {
        for (int i = 1; i < samples.Count; i++)
        {
            Sample a = samples[i - 1];
            Sample b = samples[i];
            float edge = a.Time < start ? (float)((start - a.Time) / (b.Time - a.Time)) : 0f;
            float left = X(plot, start, a.Time);
            float right = X(plot, start, b.Time);

            DrawLine(plot, animalCeiling, left, right, Mathf.Lerp(a.Sheep, b.Sheep, edge), b.Sheep, SheepColor);
            DrawLine(plot, animalCeiling, left, right, Mathf.Lerp(a.Wolves, b.Wolves, edge), b.Wolves, WolfColor);
            DrawLine(plot, grassCeiling, left, right, Mathf.Lerp(a.Grass, b.Grass, edge), b.Grass, GrassColor);
        }

        Sample latest = samples[samples.Count - 1];
        float x = X(plot, start, latest.Time);
        DrawDot(plot, animalCeiling, x, latest.Sheep, SheepColor);
        DrawDot(plot, animalCeiling, x, latest.Wolves, WolfColor);
        DrawDot(plot, grassCeiling, x, latest.Grass, GrassColor);
    }

    private float X(Rect plot, double start, double time)
    {
        return Mathf.Lerp(plot.xMin, plot.xMax, (float)((time - start) / windowSeconds));
    }

    private static float Y(Rect plot, long ceiling, float value)
    {
        return Mathf.Lerp(plot.yMax, plot.yMin, value / ceiling);
    }

    private static void DrawDot(Rect plot, long ceiling, float x, int value, Color color)
    {
        Fill(new Rect(x - 2f, Y(plot, ceiling, value) - 2f, 4f, 4f), color);
    }

    private static void DrawLine(Rect plot, long ceiling, float left, float right, float from, float to, Color color)
    {
        Vector2 a = new Vector2(left, Y(plot, ceiling, from));
        Vector2 b = new Vector2(right, Y(plot, ceiling, to));
        Vector2 delta = b - a;
        Matrix4x4 previousMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, a);
        Fill(new Rect(a.x, a.y - 1f, delta.magnitude, 2f), color);
        GUI.matrix = previousMatrix;
    }

    private static void Fill(Rect rect, Color color)
    {
        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private static GUIStyle MakeStyle(int size, Color color, FontStyle fontStyle = FontStyle.Normal)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = fontStyle };
        style.normal.textColor = color;
        return style;
    }
}
