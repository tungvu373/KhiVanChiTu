using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Reusable line geometry for the six formation seals in the gameplay camera.</summary>
internal sealed class FormationGlyphVFX
{
    private static readonly Vector3[] Bolt =
    {
        new Vector3(-.32f, 0f, .52f), new Vector3(.13f, 0f, .17f),
        new Vector3(-.12f, 0f, .13f), new Vector3(.34f, 0f, -.42f)
    };

    private static readonly Vector3[] Trigram =
    {
        new Vector3(-.30f, 0f, .17f), new Vector3(.30f, 0f, .17f),
        new Vector3(.30f, 0f, 0f), new Vector3(-.30f, 0f, 0f),
        new Vector3(-.30f, 0f, -.17f), new Vector3(.30f, 0f, -.17f)
    };

    private static readonly Vector3[] Ice =
    {
        new Vector3(0f, 0f, -.64f), new Vector3(0f, 0f, .27f),
        new Vector3(-.26f, 0f, .03f), new Vector3(0f, 0f, .27f),
        new Vector3(.26f, 0f, .03f), new Vector3(0f, 0f, .27f), new Vector3(0f, 0f, .58f)
    };

    private static readonly Vector3[] Blade =
    {
        new Vector3(0f, 0f, -.58f), new Vector3(0f, 0f, -.21f),
        new Vector3(-.26f, 0f, -.21f), new Vector3(-.15f, 0f, -.08f),
        new Vector3(0f, 0f, .61f), new Vector3(.15f, 0f, -.08f),
        new Vector3(.26f, 0f, -.21f), new Vector3(0f, 0f, -.21f)
    };

    private readonly Transform root;
    private readonly LineRenderer border;
    private readonly LineRenderer inner;
    private readonly LineRenderer[] details = new LineRenderer[8];
    private FormationId? displayed;
    private bool spiritLit;

    public FormationGlyphVFX(Transform root, Material material)
    {
        this.root = root;
        border = CreateLine(root, "Outer seal", material);
        inner = CreateLine(root, "Inner seal", material);
        for (int i = 0; i < details.Length; i++)
            details[i] = CreateLine(root, "Seal mark " + i, material);
    }

    public void Show(FormationId id, bool spiritActive, Vector3 center)
    {
        bool bright = id == FormationId.Spirit && spiritActive;
        if (displayed != id || spiritLit != bright)
        {
            Rebuild(id, bright);
            displayed = id;
            spiritLit = bright;
        }
        root.position = center + Vector3.up * .12f;
        float spin = id == FormationId.Frost || id == FormationId.GoldenBell ? -12f : 20f;
        if (bright) spin = 39f;
        root.rotation = Quaternion.Euler(0f, Time.time * spin, 0f);
        root.localScale = Vector3.one * (.82f + Mathf.Sin(Time.time * 3.5f) * (bright ? .06f : .025f));
    }

    public static Color Palette(FormationId id)
    {
        switch (id)
        {
            case FormationId.Thunder: return new Color(.78f, .39f, 1f);
            case FormationId.Spirit: return new Color(.33f, 1f, .50f);
            case FormationId.Frost: return new Color(.42f, .86f, 1f);
            case FormationId.SwordRain: return new Color(.40f, 1f, .90f);
            case FormationId.BloodDrain: return new Color(1f, .18f, .32f);
            case FormationId.GoldenBell: return new Color(1f, .79f, .23f);
            default: return Color.white;
        }
    }

    private void Rebuild(FormationId id, bool bright)
    {
        Color color = Palette(id);
        color.a = id == FormationId.Spirit && !bright ? .65f : .95f;
        int sides = id == FormationId.Spirit ? 8 : id == FormationId.GoldenBell ? 6 : 32;
        SetRing(border, sides, 2.40f, 1f, color, bright ? .105f : .075f);
        SetRing(inner, id == FormationId.Frost ? 12 : id == FormationId.SwordRain ? 12 : sides,
            1.30f, id == FormationId.Frost ? .68f : 1f, color, .045f);
        int count = id == FormationId.Spirit ? 8 : id == FormationId.BloodDrain || id == FormationId.GoldenBell ? 3 : 6;
        for (int i = 0; i < details.Length; i++)
        {
            var line = details[i];
            line.enabled = i < count;
            if (!line.enabled) continue;
            float angle = i * 360f / count;
            SetStyle(line, color, bright ? .10f : .07f);
            switch (id)
            {
                case FormationId.Thunder: Stamp(line, Bolt, angle, 1.88f); break;
                case FormationId.Spirit: Stamp(line, Trigram, angle, 1.86f); break;
                case FormationId.Frost: Stamp(line, Ice, angle, 1.74f); break;
                case FormationId.SwordRain: Stamp(line, Blade, angle, 1.74f); break;
                case FormationId.BloodDrain: SetSpiral(line, angle); break;
                case FormationId.GoldenBell: SetBellRib(line, angle); break;
            }
        }
    }

    public static LineRenderer CreateLine(Transform parent, string name, Material material)
    {
        var line = new GameObject(name, typeof(LineRenderer)).GetComponent<LineRenderer>();
        line.transform.SetParent(parent, false);
        line.sharedMaterial = material;
        line.useWorldSpace = false;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.numCapVertices = 1;
        line.enabled = false;
        return line;
    }

    public static void SetRing(LineRenderer line, int sides, float radius, float oddRadiusScale, Color color, float width)
    {
        line.loop = true;
        line.positionCount = sides;
        for (int i = 0; i < sides; i++)
        {
            float angle = i * Mathf.PI * 2f / sides;
            float r = i % 2 == 0 ? radius : radius * oddRadiusScale;
            line.SetPosition(i, new Vector3(Mathf.Sin(angle) * r, 0f, Mathf.Cos(angle) * r));
        }
        SetStyle(line, color, width);
        line.enabled = true;
    }

    private static void Stamp(LineRenderer line, Vector3[] shape, float angle, float radius)
    {
        var turn = Quaternion.Euler(0f, angle, 0f);
        line.positionCount = shape.Length;
        for (int i = 0; i < shape.Length; i++)
            line.SetPosition(i, turn * (shape[i] + Vector3.forward * radius));
    }

    private static void SetSpiral(LineRenderer line, float phase)
    {
        line.positionCount = 16;
        for (int i = 0; i < line.positionCount; i++)
        {
            float t = i / (line.positionCount - 1f);
            float angle = (phase + t * 124f) * Mathf.Deg2Rad;
            float radius = Mathf.Lerp(.85f, 2.27f, t);
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }
    }

    private static void SetBellRib(LineRenderer line, float angle)
    {
        var turn = Quaternion.Euler(0f, angle, 0f);
        line.positionCount = 13;
        for (int i = 0; i < line.positionCount; i++)
        {
            float arc = i * Mathf.PI / (line.positionCount - 1f);
            line.SetPosition(i, turn * new Vector3(Mathf.Cos(arc) * 2.31f, Mathf.Sin(arc) * 2.40f, 0f));
        }
    }

    public static void SetStyle(LineRenderer line, Color color, float width)
    {
        line.startColor = line.endColor = color;
        line.startWidth = line.endWidth = width;
    }
}
