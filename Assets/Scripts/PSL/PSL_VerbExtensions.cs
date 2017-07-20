using System.Linq;

public static class PSL_VerbExtensions
{
    public static float GetMinRange(this PSL_Verbs verb)
    {
        var fieldInfo = verb.GetType().GetField(verb.ToString());

        var attributes = (PSL_VerbAttribute[])fieldInfo.GetCustomAttributes(typeof(PSL_VerbAttribute), false);

        return attributes.Any() ? attributes.First().RangeMin : 0f;
    }

    public static bool GetUsesSocial(this PSL_Verbs verb)
    {
        var fieldInfo = verb.GetType().GetField(verb.ToString());

        var attributes = (PSL_VerbAttribute[])fieldInfo.GetCustomAttributes(typeof(PSL_VerbAttribute), false);

        return attributes.Any() && attributes.First().UsesSocial;
    }
}
