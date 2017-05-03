using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScreen : MonoBehaviour {

    public bool IsShowing { get; set; }

    protected CanvasGroup CanvasGroup;

    public virtual void Show()
    {
        if (CanvasGroup == null)
        {
            CanvasGroup = this.GetComponent<CanvasGroup>();
        }
        CanvasGroup.alpha = 1f;
        IsShowing = true;
    }

    public void Hide()
    {
        if (CanvasGroup == null)
        {
            CanvasGroup = this.GetComponent<CanvasGroup>();
        }
        CanvasGroup.alpha = 0f;
        IsShowing = false;
    }
}
