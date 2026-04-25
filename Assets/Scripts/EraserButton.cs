using UnityEngine;
using DG.Tweening;

public class EraserButton : MonoBehaviour
{
    public PaintOnClick painter;
    public GameObject selectionRing;
    public static EraserButton activeEraser;

    public void ActivateEraser()
    {
        painter.isEraser = true;

    
        foreach (var picker in FindObjectsOfType<ColorPicker>())
        {
            if (picker.selectionRing != null)
                picker.selectionRing.SetActive(false);

            picker.transform.DOKill();
            picker.transform.DOScale(1f, 0.2f);
        }

        
        if (selectionRing != null)
            selectionRing.SetActive(true);

        
        transform.DOKill();
        transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
    }

    void Awake()
    {
        activeEraser = this;
    }
}