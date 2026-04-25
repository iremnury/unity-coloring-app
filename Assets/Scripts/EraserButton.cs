using UnityEngine;
using DG.Tweening;

public class EraserButton : MonoBehaviour
{
    public PaintOnClick painter;
    public GameObject selectionRing;

    // keep a direct reference so color buttons can turn this off
    public static EraserButton activeEraser;

    public void ActivateEraser()
    {
        // switch painting into erase mode
        painter.isEraser = true;

        // clear the active state from every color swatch
        foreach (var picker in FindObjectsOfType<ColorPicker>())
        {
            if (picker.selectionRing != null)
                picker.selectionRing.SetActive(false);

            picker.transform.DOKill();
            picker.transform.DOScale(1f, 0.2f);
        }

        if (selectionRing != null)
            selectionRing.SetActive(true);

        // add the same bounce used by the color buttons
        transform.DOKill();
        transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
    }

    void Awake()
    {
        // cache the single eraser button instance in the scene
        activeEraser = this;
    }
}
