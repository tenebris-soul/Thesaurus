using UnityEngine;

public struct WalkingSignal
{
    public float HorizontalSpeed;
}
public struct StandingSignal
{
}

public struct JumpingStartedSignal
{
}
public struct LandingEndedSignal
{
}

public struct HeadChangePositionSignal
{
    public Vector3 NewPosition;
    public HeadMove HeadMoveType;
}