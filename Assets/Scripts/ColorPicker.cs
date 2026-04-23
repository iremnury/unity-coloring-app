using UnityEngine;

public class ColorPicker : MonoBehaviour
{
    // reference to the script that applies the selected color
    public PaintOnClick painter;

    // color assigned to this picker
    public Color selectedColor;

    public void SelectColor()
    {
        // send this picker color to the painter
        painter.paintColor = selectedColor;
        Debug.Log("selected color: " + selectedColor);
    }
}
