using System;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;

public class SlidingSoundService : IInitializable, IDisposable
{
    private readonly SignalBus _bus;

    private readonly AudioSource _source;    

    private PlayerSoundsConfig _config;

    private CancellationTokenSource _fadeCts;

    public SlidingSoundService(SignalBus bus,
                               AudioSource source,
                               PlayerSoundsConfig config)
    {
        _bus = bus;
        _source = source;
        _config = config;
    }

    public void Initialize()
    {
        _bus.Subscribe<SlidingSignal>(PlaySlidingSound);
        _bus.Subscribe<SlidingEndedSignal>(StopSlidingSound);
    }

    public void Dispose()
    {
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
        _fadeCts = null;

        _bus.Unsubscribe<SlidingSignal>(PlaySlidingSound);
        _bus.Unsubscribe<SlidingEndedSignal>(StopSlidingSound);
    }

    private void PlaySlidingSound()
    {
        var slidingSound = _config.SlidingSound;

        if(_source.isPlaying) return;

        _source.PlayOneShot(slidingSound);
    }

    private void StopSlidingSound()
    {
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
        _fadeCts = new CancellationTokenSource();

        FadeOutSlidingSound(_fadeCts.Token).Forget();
    }

    private async UniTaskVoid FadeOutSlidingSound(CancellationToken token)
    {
        float startVolume = _source.volume;
        float duration = 0.15f;
        float t = 0f;

        try
        {
            while(t < duration)
            {
                token.ThrowIfCancellationRequested();

                t += Time.deltaTime;
                _source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
                await UniTask.Yield();
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _source.Stop();
            _source.volume = startVolume;
        }     
    }
}