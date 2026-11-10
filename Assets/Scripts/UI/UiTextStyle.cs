using System.Text.RegularExpressions;
using UnityEngine;

public static class UiTextStyle
{
    private static readonly Regex Icons = new Regex(
        @"(?:[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u27BF]|[\u25A0-\u25FF]|[\u2300-\u23FF]|[\u2B50-\u2B55]|[\u200D\uFE0F\u25C6\u2713\u2715\u2716\u274C\u2705])[ \t]*");

    private static Font bodyFont;

    public static Font BodyFont
    {
        get
        {
            if (bodyFont == null)
            {
                bodyFont = Resources.Load<Font>("SourceSansPro-Regular");
                if (bodyFont == null)
                    bodyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return bodyFont;
        }
    }

    public static string RemoveIcons(string value) => Icons.Replace(value, "").Trim();
}

