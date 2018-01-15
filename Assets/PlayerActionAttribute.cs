using System;

[AttributeUsage(AttributeTargets.Field)]
public class PlayerActionAttribute : Attribute
{
    // If the action can be removed over time
    public bool AlwaysTracked { get; private set; }
    // an action that will be cancelled when the action is taken (eg. place action cancels pickup as the player is not currently holding)
    public PlayerAction CancelAction { get; private set; }

    public PlayerActionAttribute(bool alwaysTracked, PlayerAction cancelAction)
    {
        AlwaysTracked = alwaysTracked;
        CancelAction = cancelAction;
    }
}
