using System;
using UnityEngine;
using Zenject;

public class HeadMoverHandler : IInitializable, IDisposable
{
    private readonly SignalBus _bus;

    private readonly HeadMoverService _headMoverService;

    public HeadMoverHandler(SignalBus bus, 
                            HeadMoverService headMoverService)
    {
        _bus = bus;
        _headMoverService = headMoverService;
    }

    public void Initialize()
    {
        _bus.Subscribe<HeadChangePositionSignal>(OnChangePosition);
    }

    public void Dispose()
    {
        _bus.Unsubscribe<HeadChangePositionSignal>(OnChangePosition);
    }

    private void OnChangePosition(HeadChangePositionSignal signal)
    {
        Vector3 pos = signal.NewPosition;
        HeadMove headMoveType = signal.HeadMoveType;

        _headMoverService.SetTargetPosition(headMoveType, pos);
    }
}
