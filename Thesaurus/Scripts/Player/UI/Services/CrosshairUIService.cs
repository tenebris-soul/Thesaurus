using System;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class CrosshairUIService : IInitializable, IDisposable
{
    private readonly UIDocument _ui;
    private readonly SignalBus _bus;

    private VisualElement _crosshairContainer;
    private Image _crosshairImage;
    private Image _crosshairGradientImage;
    private Label _hintLabel;

    private const string CROSSHAIR_INTERACTABLE_CLASS = "crosshair_found";
    private const string CROSSHAIR_GRADIENT_INTERACTABLE_CLASS = "crosshair__gradient_found";
    private const string CROSSHAIR_CONTAINER_HIDDEN_CLASS = "crosshair__container_disabled";
    private const string HINT_SHOWED = "hint_showed";

    private bool _hideTransitionCallbackRegistered = false;

    public CrosshairUIService(UIDocument ui,
                            SignalBus bus)
    {
        _ui = ui;
        _bus = bus;
    }

    public void Initialize()
    {
        var root = _ui.rootVisualElement;
        var hudContainer = root.Q<VisualElement>("HUDContainer");
        var hud = hudContainer.Q<VisualElement>("HUD");

        _crosshairContainer = hud.Q<VisualElement>("CrosshairContainer");
        _crosshairImage = _crosshairContainer.Q<Image>("Crosshair");
        _crosshairGradientImage = _crosshairContainer.Q<Image>("CrosshairGradient");

        _hintLabel = hud.Q<VisualElement>("HintContainer")
                         .Q<Label>("Hint");

        _bus.Subscribe<InteractableFoundSignal>(ChangeCrosshairIfInteractable);
        _bus.Subscribe<InteractableLostSignal>(ResetCrosshair);

        _bus.Subscribe<InspectModeSignal>(ToggleCrosshair);
        _bus.Subscribe<ArtefactModeSignal>(ToggleCrosshair);
        _bus.Subscribe<PaintingModeSignal>(ToggleCrosshair);

        if (_crosshairContainer.ClassListContains(CROSSHAIR_CONTAINER_HIDDEN_CLASS))
            _crosshairContainer.style.display = DisplayStyle.None;
    }

    public void Dispose()
    {
        _bus.Unsubscribe<InteractableFoundSignal>(ChangeCrosshairIfInteractable);
        _bus.Unsubscribe<InteractableLostSignal>(ResetCrosshair);
        
        _bus.Unsubscribe<InspectModeSignal>(ToggleCrosshair);
        _bus.Unsubscribe<ArtefactModeSignal>(ToggleCrosshair);
        _bus.Unsubscribe<PaintingModeSignal>(ToggleCrosshair);

        if (_hideTransitionCallbackRegistered)
        {
            _crosshairContainer.UnregisterCallback<TransitionEndEvent>(OnCrosshairHideTransitionEnd);
            _hideTransitionCallbackRegistered = false;
        }
    }

    private void ChangeCrosshairIfInteractable(InteractableFoundSignal signal)
    {
        _crosshairImage.AddToClassList(CROSSHAIR_INTERACTABLE_CLASS);
        _crosshairGradientImage.AddToClassList(CROSSHAIR_GRADIENT_INTERACTABLE_CLASS);

        string hintText = "Wow how did you find it?";

        hintText = signal.Type switch
        {
            InteractableTypes.Exhibit => "Нажмите У, чтобы осмотреть экспонат",
            InteractableTypes.Artifact => "Нажмите У, чтобы осмотреть артефакт",
            InteractableTypes.Painting => "Нажмите У, чтобы осмотреть картину",
            _ => "Хмм... Что-то странное...",
        };

        _hintLabel.text = hintText;

        _hintLabel.AddToClassList(HINT_SHOWED);
    }

    private void ResetCrosshair()
    {
        _crosshairImage.RemoveFromClassList(CROSSHAIR_INTERACTABLE_CLASS);
        _crosshairGradientImage.RemoveFromClassList(CROSSHAIR_GRADIENT_INTERACTABLE_CLASS);

        _hintLabel.RemoveFromClassList(HINT_SHOWED);
    }

    private void ToggleCrosshair(InspectModeSignal signal)
    {
        SetCrosshairVisible(!signal.IsActive); 
    }

    private void ToggleCrosshair(ArtefactModeSignal signal)
    {
        SetCrosshairVisible(!signal.IsActive); 
    }
    private void ToggleCrosshair(PaintingModeSignal signal)
    {
        SetCrosshairVisible(!signal.IsActive); 
    }

    private void SetCrosshairVisible(bool visible)
    {
        if (visible)
            ShowAnimated();
        else
            HideAnimatedToDisplayNone();
    }

    private void ShowAnimated()
    {
        if (_hideTransitionCallbackRegistered)
        {
            _crosshairContainer.UnregisterCallback<TransitionEndEvent>(OnCrosshairHideTransitionEnd);
            _hideTransitionCallbackRegistered = false;
        }

        _crosshairContainer.style.display = DisplayStyle.Flex;
        _crosshairContainer.pickingMode = PickingMode.Position;

        _crosshairContainer.schedule.Execute(() =>
        {
            _crosshairContainer.RemoveFromClassList(CROSSHAIR_CONTAINER_HIDDEN_CLASS);
        });
    }

    private void HideAnimatedToDisplayNone()
    {
        if (_crosshairContainer.style.display.value == DisplayStyle.None)
            return;

        _crosshairContainer.pickingMode = PickingMode.Ignore;

        if (!_hideTransitionCallbackRegistered)
        {
            _crosshairContainer.RegisterCallback<TransitionEndEvent>(OnCrosshairHideTransitionEnd);
            _hideTransitionCallbackRegistered = true;
        }

        _crosshairContainer.AddToClassList(CROSSHAIR_CONTAINER_HIDDEN_CLASS);
    }

    private void OnCrosshairHideTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target != _crosshairContainer)
            return;

        bool opacityEnded = false;
        foreach (var p in evt.stylePropertyNames)
        {
            if (p.ToString() == "opacity")
            {
                opacityEnded = true;
                break;
            }
        }
        if (!opacityEnded)
            return;

        _crosshairContainer.style.display = DisplayStyle.None;

        _crosshairContainer.UnregisterCallback<TransitionEndEvent>(OnCrosshairHideTransitionEnd);
        _hideTransitionCallbackRegistered = false;
    }
}
