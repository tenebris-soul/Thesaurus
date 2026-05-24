using UnityEngine;

[CreateAssetMenu(menuName = "Configs/PlayerMovementTuningConfig")]
public class PlayerMovementTuning : ScriptableObject
{
    public int MOVE_MAX_ITER = 3;
    public float SKIN_WIDTH = 0.02f;

    public float CARRY_DAMP = 15f;

    public float PROBE_UP_EXTENT = 0.01f;
    public float PROBE_DOWN = 0.15f;

    public float PROBE_UP = 0.01f;

    public float SLOPE_ANGLE_EPS = 0.5f;

    public float STEP_OFFSET = 0.3f;

    public int DEPEN_MAX_ITER = 3;
    public float DEPEN_EPS = 0.001f;

    public float MAX_SLOPE_ANGLE = 45f;

    public float PUSH_FORCE = 2f;

    public QueryTriggerInteraction QTI = QueryTriggerInteraction.Ignore;
}
