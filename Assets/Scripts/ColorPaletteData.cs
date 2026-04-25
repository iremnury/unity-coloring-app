using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Game/Color Palette")]
public class ColorPaletteData : ScriptableObject
{
    // hold the preset palette used by the color buttons
    public Color[] colors;
}
