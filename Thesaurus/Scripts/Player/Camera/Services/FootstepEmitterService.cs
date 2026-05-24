using System;
using UnityEngine;
using Zenject;

public class FootstepEmitterService : IInitializable, IDisposable
{
    private SignalBus _bus;

    private AudioSource _source;

    private PlayerSoundsConfig _config;
    private PlayerReadContext _playerReadContext;

    private int _playedBefore = -1;

    private bool _alreadyStepped = false;

    public FootstepEmitterService(SignalBus bus,
                                  AudioSource source,
                                  PlayerSoundsConfig config,
                                  PlayerReadContext playerReadContext)
    {
        _bus = bus;
        _source = source;
        _config = config;
        _playerReadContext = playerReadContext;
    }

    public void Initialize()
    {
        _bus.Subscribe<StepEmitted>(PlayFootstepSound);
        _bus.Subscribe<StepDone>(ResetStep);

        _bus.Subscribe<JumpingStartedSignal>(PlayFootstepSound);
        _bus.Subscribe<JumpingStartedSignal>(ResetStep);

        _bus.Subscribe<LandingEndedSignal>(PlayFootstepSound);
        _bus.Subscribe<LandingEndedSignal>(ResetStep);
    }
    public void Dispose()
    {
        _bus.Unsubscribe<StepEmitted>(PlayFootstepSound);
        _bus.Unsubscribe<StepDone>(ResetStep);

        _bus.Unsubscribe<JumpingStartedSignal>(PlayFootstepSound);
        _bus.Unsubscribe<JumpingStartedSignal>(ResetStep);

        _bus.Unsubscribe<LandingEndedSignal>(PlayFootstepSound);
        _bus.Unsubscribe<LandingEndedSignal>(ResetStep);
    }

    private void PlayFootstepSound()
    {
        if(_alreadyStepped) return;

        AudioClip[] sounds;

        var groundCollider = _playerReadContext.GroundCollider;

        string surfaceName = null;
        if (groundCollider != null && SurfaceRegistry.TryGetSurface(groundCollider, out SurfaceObject surface) && surface != null)
        {
            surfaceName = surface.SurfaceName;
        }

        if (!_config.TryGetSurfaceSounds(surfaceName, out sounds))
        {
            return;
        }

        if(sounds.Length == 0) {
            Debug.LogWarning("FootstepEmitterService: No footstep sounds available to play.");
            return;
        }

        int chosen = -1;

        while(chosen == _playedBefore || chosen == -1)
        {
            chosen = UnityEngine.Random.Range(0, sounds.Length);
        }

        _playedBefore = chosen;
        _source.PlayOneShot(sounds[chosen]);

        _alreadyStepped = true;
    }

    private void ResetStep() => _alreadyStepped = false;
}
