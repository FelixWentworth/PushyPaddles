using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Controls : MonoBehaviour
{
    protected UnityAction _left { get; set; }
    protected UnityAction _right { get; set; }
    protected UnityAction _up { get; set; }
    protected UnityAction _down { get; set; }
    protected UnityAction _stop { get; set; }
    protected UnityAction _interact { get; set; }

    public void ListenTo(UnityAction left = null, UnityAction right = null, UnityAction up = null,
        UnityAction down = null, UnityAction stop = null, UnityAction interact = null)
    {
        _left = left;
        _right = right;
        _up = up;
        _down = down;
        _stop = stop;
        _interact = interact;
    }

    protected void MoveLeft()
    {
        if (_left != null)
            _left();
    }
    protected void MoveRight()
    {
        if (_right != null)
            _right();
    }
    protected void MoveUp()
    {
        if (_up != null)
            _up();
    }
    protected void MoveDown()
    {
        if (_down != null)
            _down();
    }

    protected void StopMoving()
    {
        if (_stop != null)
            _stop();
    }
    protected void Interact()
    {
        if (_interact != null)
            _interact();
    }
}
