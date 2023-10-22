using UnityEngine;
using UnityEngine.UIElements;

public static class StylesheetUtils
{
    public static StyleSheet Load(string name)
    {
        return Resources.Load<StyleSheet>($"Stylesheets/{name}");
    }
}
