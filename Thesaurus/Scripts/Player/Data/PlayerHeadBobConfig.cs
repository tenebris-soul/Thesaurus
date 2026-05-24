using UnityEngine;

[CreateAssetMenu(menuName = "Configs/PlayerHeadMoveConfig")]
public class PlayerHeadMoveConfig : ScriptableObject
{
    public float BobX = 0.05f;
    public float BobY = 0.05f;
    public AnimationCurve BobCurve = AnimationCurve.EaseInOut(0f, -1f, 1f, 1f);

    public float LandingDuration = 0.3f;
    public AnimationCurve LandingCurve = AnimationCurve.EaseInOut(0f, -1f, 1f, 1f);
}
