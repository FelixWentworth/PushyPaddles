using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controls_JoyStick : Controls
{
    public RectTransform Boundaries;
    public RectTransform Knob;

    private Vector3 _startPosition;
    private float _width;
    private float _height;

    private bool _horizontal;
    private float _x;
    private float _y;

    private float _minX
    {
        get { return Boundaries.transform.position.x - (_width / 2); }
    }

    private float _maxX
    {
        get { return Boundaries.transform.position.x + (_width / 2); }
    }

    private float _minY
    {
        get { return Boundaries.transform.position.y - (_height/ 2); }
    }

    private float _maxY
    {
        get { return Boundaries.transform.position.y + (_height / 2); }
    }

    void Start()
    {
        _width = Knob.rect.width;
        _height = Knob.rect.height;

        _startPosition = Knob.transform.position;
    }

    public void Drag()
    {
        var draggedPosition = Input.mousePosition - _startPosition;
        // clamp to borders
        var x = Mathf.Clamp(draggedPosition.x, _minX, _maxX);
        var y = Mathf.Clamp(draggedPosition.y, _minY, _maxY);
        Knob.anchoredPosition = new Vector2(x, y);
        Move(x, y);
    }

    public void Drop()
    {
        Knob.anchoredPosition = new Vector2(0f, 0f);
        Move(0f, 0f);
    }

    public void InteractPressed()
    {
        Interact();
    }

    private void Move(float x, float y)
    {
        _horizontal = Mathf.Abs(x) > Mathf.Abs(y);
        _x = Mathf.Clamp(Mathf.Round(x), -1, 1);
        _y = Mathf.Clamp(Mathf.Round(y), -1, 1);
    }

    void Update()
    {
        if (_horizontal && _x != 0)
        {
            if (_x == -1)
                MoveLeft();
            else
                MoveRight();
        }
        if (!_horizontal && _y != 0)
        {
            if (_y == -1)
                MoveDown();
            else
                MoveUp();
        }
        if (_x == 0 && _y == 0)
        {
            StopMoving();
        }
    }
}
