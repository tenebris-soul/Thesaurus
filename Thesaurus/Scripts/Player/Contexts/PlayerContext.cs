using UnityEngine;

public class PlayerContext : PlayerReadContext, PlayerWriteContext
{
    // private jumping options
    private bool _requestJumping;

    // private grounding states
    private bool _isGrounded;
    private bool _wasGroundedInPrevFrame;
    private bool _isSteepGround;
    private Vector3 _groundNormal;
    private Collider _groundCollider;

    // private ceiling states
    private bool _isHittingCeiling;

    // public jumping options
    public bool RequestJumping => _requestJumping;

    // public grounding states
    public bool IsGrounded => _isGrounded;
    public bool WasGroundedInPrevFrame => _wasGroundedInPrevFrame;
    public bool IsSteepGround => _isSteepGround;
    public Vector3 GroundNormal => _groundNormal;
    public Collider GroundCollider => _groundCollider;

    // public ceiling states
    public bool IsHittingCeiling => _isHittingCeiling;

    // setters for jumping
    public void SetRequestJumping(bool requested) => _requestJumping = requested;

    // motor's events (not pattern)
    public bool IsLanded => (!_wasGroundedInPrevFrame && _isGrounded);
    public bool LeftGround => (_wasGroundedInPrevFrame && !_isGrounded);


    // setters for grounding options
    public void SetGroundedState(bool isGrounded) => _isGrounded = isGrounded;
    public void SetGroundedInPrevFrame(bool wasGrounded) => _wasGroundedInPrevFrame = wasGrounded;
    public void SetSteepGround(bool isSteepGround) => _isSteepGround = isSteepGround;
    public void SetGroundNormal(Vector3 normal) => _groundNormal = normal;
    public void SetGroundCollider(Collider groundCollider) => _groundCollider = groundCollider;

    // setters for ceiling options
    public void SetCeilingHitting(bool isHittingCeiling) => _isHittingCeiling = isHittingCeiling;

}
