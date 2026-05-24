using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerMotor : IPlayerMotor, IFixedTickable
{
    private readonly Rigidbody _playerRigidbody;
    private readonly CapsuleCollider _playerCollider;

    private readonly IPlatformMotionService _platformService;
    private readonly IKinematicMoveService _kinematicMoveSerivce;
    private readonly IGroundingService _groundingSerivce;
    private readonly ICapsuleDepenetration _capsuleDepen;
    private readonly ICeilingCheckService _ceilingCheckService;

    private readonly PlayerWriteContext _playerWriteContext;
    private readonly PlayerReadContext _PlayerReadMotorContext;

    private readonly IGizmosSphereService _gizmosSphere;

    private readonly PlayerMovementTuning _playerMovementTuning;

    private Vector3 _velocity;

    private Rigidbody _groundRb;
    private Vector3 _groundPointVelocity;
    private Vector3 _carryVelocity;

    private float _debugUntilTime;

    public Vector3 Velocity => _velocity + _carryVelocity;

    public PlayerMotor(Rigidbody playerRigidbody,
                       CapsuleCollider playerCollider,
                       IPlatformMotionService platformService,
                       IKinematicMoveService kinematicMoveService,
                       IGroundingService groundingService,
                       ICeilingCheckService ceilingCheckService,
                       ICapsuleDepenetration capsuleDepenetration,
                       PlayerWriteContext playerWriteContext,
                       PlayerReadContext PlayerReadMotorContext,
                       PlayerMovementTuning playerMovementTuning,
                       IGizmosSphereService gizmosSphere
                       )
    {
        _playerRigidbody = playerRigidbody;
        _playerCollider = playerCollider;

        _platformService = platformService;
        _kinematicMoveSerivce = kinematicMoveService;
        _groundingSerivce = groundingService;
        _ceilingCheckService = ceilingCheckService;
        _capsuleDepen = capsuleDepenetration;

        _playerWriteContext = playerWriteContext;
        _PlayerReadMotorContext = PlayerReadMotorContext;

        _playerMovementTuning = playerMovementTuning;

        _gizmosSphere = gizmosSphere;
    }

    public (Vector3, Vector3) GetPlayerDirs() => (_playerCollider.transform.forward, _playerCollider.transform.right);

    public void SetVelocity(Vector3 desiredVelocity)
    {
        _velocity = desiredVelocity;
    }

    public void FixedTick()
    {
        _gizmosSphere.Clear();
        float dt = Time.fixedDeltaTime;

        bool isGrounded = _PlayerReadMotorContext.IsGrounded;
        bool isLanded = _PlayerReadMotorContext.IsLanded;
        bool leftGround = _PlayerReadMotorContext.LeftGround;

        Vector3 playerPosition = _playerRigidbody.position;
        Vector3 preMovePos = playerPosition;

        playerPosition = _platformService.ApplyGroundVelocity(playerPosition);

        _groundPointVelocity = (isGrounded && _groundRb)
                               ? _platformService.ComputeGroundPointVelocity(playerPosition, dt)
                               : Vector3.zero;

        Vector3 delta = (_velocity + _carryVelocity) * dt;

        playerPosition = _kinematicMoveSerivce.MoveWithCollisions(playerPosition, 
                                                                  delta
                                                                  );


        playerPosition = _capsuleDepen.DepenetrateFromColliders(playerPosition);

        _playerWriteContext.SetGroundedInPrevFrame(isGrounded);

        Rigidbody prevGroundRb = _groundRb;


        _groundingSerivce.ProbeGround(playerPosition,
                                     out _groundRb);

        isGrounded = _PlayerReadMotorContext.IsGrounded;
        isLanded = _PlayerReadMotorContext.IsLanded;
        leftGround = _PlayerReadMotorContext.LeftGround;

        if(leftGround && prevGroundRb)
        {
            _carryVelocity = new(_groundPointVelocity.x, 0f, _groundPointVelocity.z);
            _groundRb = null;
        }

        _platformService.EndTick(_groundRb);

        if (isGrounded && Vector3.Dot(_velocity, _PlayerReadMotorContext.GroundNormal) <= 0.001f)
        {
            Vector3 beforeSnap = playerPosition;
            playerPosition = _groundingSerivce.SoftSnapToGround(playerPosition,
                                                                Velocity);
            playerPosition = _capsuleDepen.DepenetrateFromColliders(playerPosition);
        }

        if (isLanded)
        {
            _carryVelocity = Vector3.zero;
        }
        else
        {
            float k = Mathf.Exp(-_playerMovementTuning.CARRY_DAMP * dt);
            _carryVelocity *= k;
        }


        _ceilingCheckService.ProbeCeiling(playerPosition);

        _playerRigidbody.MovePosition(playerPosition);
    }
}
