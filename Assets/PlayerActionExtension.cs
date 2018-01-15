using System.Linq;

public static class PlayerActionExtension
{
    public static bool GetAlwaysTracked(this PlayerAction action)
    {
        var fieldInfo = action.GetType().GetField(action.ToString());

        var attributes = (PlayerActionAttribute[])fieldInfo.GetCustomAttributes(typeof(PlayerActionAttribute), false);

        return attributes.Any() && attributes.First().AlwaysTracked;
    }

    public static PlayerAction GetCancelAction(this PlayerAction action)
    {
        var fieldInfo = action.GetType().GetField(action.ToString());

        var attributes = (PlayerActionAttribute[])fieldInfo.GetCustomAttributes(typeof(PlayerActionAttribute), false);

        return attributes.Any() ? attributes.First().CancelAction : PlayerAction.None;
    }
}
