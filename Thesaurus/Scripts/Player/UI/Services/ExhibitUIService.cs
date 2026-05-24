using System;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public sealed class ExhibitUIService : IInitializable, IDisposable
{
    private readonly UIDocument _ui;
    private readonly SignalBus _bus;

    private VisualElement _root;
    private VisualElement _exhibitContainer;
    private VisualElement _exhibitDataWrapper;
    private ScrollView _descriptionScrollView;
    private Label _titleLabel;
    private Label _descriptionLabel;
    private Button _closeButton;

    private const string EXHIBIT_UI_DISABLED_CLASS = "exhibitData__wrapper_disabled";
    private const string EXHIBIT_CLOSE_BUTTON_HIDDEN_CLASS = "exhibitData__closeButton_disabled";

    private bool _hideTransitionCallbackRegistered = false;

    public ExhibitUIService(UIDocument ui,
                            SignalBus bus)
    {
        _ui = ui;
        _bus = bus;
    }

    public void Initialize()
    {
        _root = _ui.rootVisualElement;
        _exhibitContainer = _root.Q<VisualElement>("ExhibitContainer");
        var exhibit = _exhibitContainer.Q<VisualElement>("Exhibit");

        _exhibitDataWrapper = exhibit.Q<VisualElement>("ExhibitDataWrapper");
        _closeButton = exhibit.Q<Button>("ExhibitDataCloseButton");

        _titleLabel = _exhibitDataWrapper.Q<Label>("ExhibitDataNameText");
        _descriptionLabel = _exhibitDataWrapper.Q<Label>("ExhibitDataDescriptionText");

        _descriptionScrollView = _exhibitDataWrapper.Q<ScrollView>("ExhibitDataDescriptionScrollView");

        _descriptionLabel.enableRichText = true;
        _descriptionLabel.style.unityTextAlign = TextAnchor.UpperLeft;

        _closeButton.clicked += OnCloseButtonClicked;

        SetCloseButtonPicking(false);

        _bus.Subscribe<InspectModeSignal>(ToggleExhibitUI);

        if (_exhibitDataWrapper.ClassListContains(EXHIBIT_UI_DISABLED_CLASS))
            _exhibitContainer.style.display = DisplayStyle.None;
    }

    public void Dispose()
    {
        _closeButton.clicked -= OnCloseButtonClicked;
        _bus.Unsubscribe<InspectModeSignal>(ToggleExhibitUI);

        if (_hideTransitionCallbackRegistered)
        {
            _exhibitContainer.UnregisterCallback<TransitionEndEvent>(OnExhibitHideTransitionEnd);
            _hideTransitionCallbackRegistered = false;
        }
    }

    private void ToggleExhibitUI(InspectModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if (isActive)
        {
            ExhibitData exhibitData = signal.Exhibit.ExhibitData;

            _titleLabel.text = exhibitData.Name;
            _descriptionLabel.text = $"<align=\"justified\">{exhibitData.Description}</align>";

            _descriptionScrollView.verticalScroller.value = 0;

            SetExhibitUIVisible(true);
        }
        else
        {
            SetExhibitUIVisible(false);
        }
    }

    private void SetExhibitUIVisible(bool visible)
    {
        if(visible)
            ShowAnimated();
        else
            HideAnimatedToDisplayNone();
    }

    private void ShowAnimated()
    {
        if (_hideTransitionCallbackRegistered)
        {
            _exhibitDataWrapper.UnregisterCallback<TransitionEndEvent>(OnExhibitHideTransitionEnd);
            _hideTransitionCallbackRegistered = false;
        }

        _exhibitContainer.style.display = DisplayStyle.Flex;

        _exhibitDataWrapper.AddToClassList(EXHIBIT_UI_DISABLED_CLASS);
        _closeButton.AddToClassList(EXHIBIT_CLOSE_BUTTON_HIDDEN_CLASS);
        SetCloseButtonPicking(false);

        _exhibitDataWrapper.schedule.Execute(() =>
        {
            _exhibitDataWrapper.RemoveFromClassList(EXHIBIT_UI_DISABLED_CLASS);
            _closeButton.RemoveFromClassList(EXHIBIT_CLOSE_BUTTON_HIDDEN_CLASS);
            SetCloseButtonPicking(true);
        });
    }

    private void HideAnimatedToDisplayNone()
    {
        if (_exhibitContainer.style.display.value == DisplayStyle.None)
            return;

        SetCloseButtonPicking(false);

        if (!_hideTransitionCallbackRegistered)
        {
            _exhibitDataWrapper.RegisterCallback<TransitionEndEvent>(OnExhibitHideTransitionEnd);
            _hideTransitionCallbackRegistered = true;
        }

        _exhibitDataWrapper.AddToClassList(EXHIBIT_UI_DISABLED_CLASS);
        _closeButton.AddToClassList(EXHIBIT_CLOSE_BUTTON_HIDDEN_CLASS);
    }

    private void OnExhibitHideTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target != _exhibitDataWrapper)
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

        _exhibitContainer.style.display = DisplayStyle.None;

        _exhibitDataWrapper.UnregisterCallback<TransitionEndEvent>(OnExhibitHideTransitionEnd);
        _hideTransitionCallbackRegistered = false;
    }

    private void SetCloseButtonPicking(bool enabled)
    {
        _closeButton.pickingMode = enabled ? PickingMode.Position : PickingMode.Ignore;

        for (int i = 0; i < _closeButton.childCount; i++)
        {
            var child = _closeButton.ElementAt(i);
            child.pickingMode = enabled ? PickingMode.Position : PickingMode.Ignore;
        }
    }

    private void OnCloseButtonClicked()
    {
        InspectModeSignal inspectSignal = new InspectModeSignal()
        {
            IsActive = false,
            Exhibit = null
        };

        SearchModeSignal searchSignal = new SearchModeSignal()
        {
            IsActive = true
        };

        _bus.Fire(inspectSignal);
        _bus.Fire(searchSignal);
    }
}
