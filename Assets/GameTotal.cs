using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTotal : MonoBehaviour
{

    [SerializeField] private Animation _animation;

    [SerializeField] private Text _expression;
    [SerializeField] private Text _total;
    [SerializeField] private Text _condition;

    public void Show(string expression, string total, bool victory)
    {
        _expression.text = expression;
        _total.text = total;
        _condition.text = victory ? "You Win" : "Try Again";

        _animation.Play();

    }

    public float AnimLength()
    {
        return _animation.clip.length;
    }
}
