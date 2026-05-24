using UnityEngine;

public interface IStepUpService
{
    bool TryStepUp(ref Vector3 playerPosition,
                       Vector3 leftDir,
                       float leftDist);
    // bool TryProbeStepTop(Vector3 pos, out Vector3 normal, out float bestGap);
}
