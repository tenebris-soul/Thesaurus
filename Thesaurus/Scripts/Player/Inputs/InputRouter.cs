using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public sealed class InputRouter : IInputRouter, ITickable
{
    private readonly Dictionary<InputMode, IInputContext> _contexts;

    private struct Entry
    {
        public InputMode Mode;
        public object Owner;
        public int Id;
    }
    private readonly List<Entry> _modeStack = new();
    private int _nextId = 1;

    private InputMode _activeMode;
    private bool _hasActiveMode;


    public InputRouter(GameplayInputContext gameplay, 
                       InspectInputContext inspect,
                       ArtifactInputContext artifact,
                       PaintingInputContext painting)
    {
        _contexts = new()
        {
            { InputMode.Gameplay, gameplay },
            { InputMode.InspectExhibit, inspect },
            { InputMode.InspectArtefact, artifact },
            { InputMode.InspectPainting, painting }
        };

        // _modeStack.Add(InputMode.Gameplay);
        _modeStack.Add(new Entry { Mode = InputMode.Gameplay, Id = 0, Owner = this});
        SetMode(InputMode.Gameplay); 
    }

    public void Tick()
    {
        if (!_hasActiveMode) return;
        _contexts[_activeMode].ReadAndDispatch();
    }


    public void Subscribe<T>(InputMode mode, T receiver)
    {
        if(_contexts[mode] is IInputContext<T> context)
        {
            context.Subscribe(receiver);
        }
        else
        {
            throw new ArgumentException(
                $"Context for mode {mode} doesn't accept {typeof(T).Name}");
        }
    }

    public void Unsubscribe<T>(InputMode mode, T receiver)
    {
        if(_contexts[mode] is IInputContext<T> context)
        {
            context.Unsubscribe(receiver);
        }
        else
        {
            throw new ArgumentException(
                $"Context for mode {mode} doesn't accept {typeof(T).Name}");
        }
    }

    private void SetMode(InputMode mode)
    {
        if (_hasActiveMode && _activeMode == mode) return;

        if (_hasActiveMode)
            _contexts[_activeMode].Disable();

        _activeMode = mode;
        _contexts[_activeMode].Enable();
        _hasActiveMode = true;
    }

    public ModeLease PushMode(InputMode mode, object owner)
    {
        if (owner == null) throw new ArgumentNullException(nameof(owner));

        if (_modeStack.Count > 0 && _modeStack[^1].Mode == mode)
            throw new InvalidOperationException($"Mode {mode} already on top");

        var entry = new Entry
        {
            Mode = mode,
            Id = _nextId++,
            Owner = owner
        };

        _modeStack.Add(entry);
        SetMode(mode);

        return new ModeLease(this, entry.Id, entry.Mode, entry.Owner);
    }

    private void PopLease(ModeLease lease)
    {
        if (_modeStack.Count <= 1)
            throw new InvalidOperationException("Can't pop base input mode");

        var top = _modeStack[^1];

        // 1) лейз от этого роутера?
        if (!ReferenceEquals(lease.Router, this))
            throw new InvalidOperationException("Lease belongs to another router");

        // 2) строгий порядок: попнуть можно только верх
        if (top.Id != lease.Id)
            throw new InvalidOperationException($"Pop out of order. Trying {lease.Mode}, but top is {top.Mode}");

        // 3) владелец совпадает?
        if (!ReferenceEquals(top.Owner, lease.Owner))
            throw new InvalidOperationException("Owner mismatch for lease");

        _modeStack.RemoveAt(_modeStack.Count - 1);
        SetMode(_modeStack[^1].Mode);
    }


    public sealed class ModeLease : IDisposable
    {
        internal readonly InputRouter Router;
        internal readonly int Id;
        internal readonly InputMode Mode;
        internal readonly object Owner;
        private bool _disposed;

        internal ModeLease(InputRouter router, int id, InputMode mode, object owner)
        {
            Router = router;
            Id = id;
            Mode = mode;
            Owner = owner;
        }

        public void Dispose()
        {
            if(_disposed) return;
            _disposed = true;
            Router.PopLease(this);
        }
    }
}