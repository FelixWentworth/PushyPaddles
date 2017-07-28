public enum PlayerAction
{
    [PlayerAction(false, None)] None = 0,

    [PlayerAction(false, None)] Pushed,
    [PlayerAction(false, None)] HitObstacle,
    [PlayerAction(false, None)] GotCollectible,
    [PlayerAction(true, None)] ReachedChest,
    [PlayerAction(true, None)] ReachedChestSuccess,
    [PlayerAction(true, None)] ReachedChestFail,
    [PlayerAction(false, None)] PickedUpPlatform,
    [PlayerAction(false, PickedUpPlatform)] PlacedPlatform,
    [PlayerAction(false, None)] Idle,
    [PlayerAction(true, None)] GaveRewardSelf,
    [PlayerAction(true, None)] GaveRewardOther

}
