using UnityEngine;

[CreateAssetMenu(menuName = "Configs/PlayerConfig")]
public class PlayerMovementConfig : ScriptableObject
{
    public float ForwardSpeed = 3f;
    public float BackSpeed = 1.5f;

    public float JumpHeight = 0.75f;
    public float Gravity = 9.81f;

    public float AirControlSpeed = 2f;
    public float AirAcceleration = 6f;
    public float AirTurnRateDegrees = 120f;
    public float TurnSlowdown = 0.12f;

    public float SlideGravityMul = 1.0f;
    public float SlideDrag = 0.6f;
    public float SlideMaxSpeed = 18f;

    public float SlideStrafeMaxSpeed = 4.5f;
    public float SlideStrafeAccel = 25f;
}