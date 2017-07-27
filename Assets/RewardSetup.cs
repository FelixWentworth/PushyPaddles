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
    }
}
