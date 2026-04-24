using UnityEngine;

public class ColorPicker : MonoBehaviour
{
    // reference to the script that applies the selected color
    public PaintOnClick painter;

    // color assigned to this picker
    public Color selectedColor;

public void SelectColor()
{
    painter.paintColor = selectedColor;

    PlayerPrefs.SetFloat("R", selectedColor.r);
    PlayerPrefs.SetFloat("G", selectedColor.g);
    PlayerPrefs.SetFloat("B", selectedColor.b);
    PlayerPrefs.Save();

 
    // transform.localScale = Vector3.one * 1.2f;

    Debug.Log("selected color: " + selectedColor);
}
}
