using System;

[AttributeUsage(AttributeTargets.Field)]
public class PSL_VerbAttribute : Attribute
{
    public float RangeMin { get; private set; }
    public bool UsesSocial { get; private set; }

    public PSL_VerbAttribute(float min, bool social)
    {
        RangeMin = min;
        UsesSocial = social;
    }
}