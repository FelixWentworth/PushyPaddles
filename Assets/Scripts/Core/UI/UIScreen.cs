using System.Reflection.Emit;
using UnityEngine;

public class UIScreen : MonoBehaviour {

    public bool IsShowing
    {
        get { return GetComponent<CanvasGroup>().alpha == 1f; }
        set { IsShowing = value; }
    }

    protected CanvasGroup CanvasGroup;

    public virtual void Show()
    {
        if (CanvasGroup == null)
        {
            CanvasGroup = this.GetComponent<CanvasGroup>();
        }
        CanvasGroup.alpha = 1f;
        CanvasGroup.blocksRaycasts = true;
    }

    public virtual void Hide()
    {
        if (CanvasGroup == null)
        {
            CanvasGroup = this.GetComponent<CanvasGroup>();
        }
        CanvasGroup.alpha = 0f;
        CanvasGroup.blocksRaycasts = false;
    }
}
