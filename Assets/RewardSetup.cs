using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardSetup : MonoBehaviour
{

    public SpriteRenderer IconFront;
    public SpriteRenderer IconBack;
    public TextMesh RewardText;

    public void Setup(string name, Sprite icon)
    {
        IconBack.sprite = IconFront.sprite = icon;
        RewardText.text = name;
        SetTextWidth();
    }

    private void SetTextWidth()
    {
        var textWidth = RewardText.gameObject.GetComponent<MeshRenderer>().bounds.size.x;
        while (textWidth > IconBack.bounds.size.x * 2.25)
        {
            RewardText.fontSize -= 1;
            textWidth = RewardText.gameObject.GetComponent<MeshRenderer>().bounds.size.x;
        }

    }
}
