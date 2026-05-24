using System;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;

public class HeadSpringService : IInitializable, IDisposable
{
    private readonly SignalBus _bus;
    private readonly PlayerHeadMoveConfig _config;

    private Transform _cameraVisualsParent;

    private CancellationTokenSource _springCts;

    public HeadSpringService(SignalBus bus, PlayerHeadMoveConfig config, Transform cameraVisualsTransform)
    {
        _bus = bus;
        _config = config;
        _cameraVisualsParent = cameraVisualsTransform.parent;
    }

    public void Initialize()
    {
        _bus.Subscribe<LandingEndedSignal>(PlayLandingBounce);
    }

    public void Dispose()
    {
        _springCts?.Cancel();
        _springCts?.Dispose();
        _springCts = null;

        _bus.Unsubscribe<LandingEndedSignal>(PlayLandingBounce);
    }

    private void PlayLandingBounce(LandingEndedSignal signal)
    {
        _springCts?.Cancel();
        _springCts?.Dispose();
        _springCts = new CancellationTokenSource();

        MakeCameraSpring(_springCts.Token).Forget();
    }

    private async UniTaskVoid MakeCameraSpring(CancellationToken token)
    {
        Vector3 localUp = _cameraVisualsParent.InverseTransformDirection(Vector3.up);
        
        float duration = _config.LandingDuration;
        AnimationCurve curve = _config.LandingCurve;

        float t = 0f;
        
        try
        {
            while(t < duration)
            {
                token.ThrowIfCancellationRequested();

                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / duration);

                float f = curve.Evaluate(n);
                
                Vector3 newPos = localUp * f;

                HeadChangePositionSignal changeSignal = new HeadChangePositionSignal() 
                { 
                    NewPosition = newPos, 
                    HeadMoveType = HeadMove.Landing
                };

                _bus.Fire(changeSignal);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation if needed
        }
        finally
        {
            HeadChangePositionSignal resetSignal = new HeadChangePositionSignal() 
            { 
                NewPosition = Vector3.zero, 
                HeadMoveType = HeadMove.Landing
            };

            _bus.Fire(resetSignal);
        }
    }
}
