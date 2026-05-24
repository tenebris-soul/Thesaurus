using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;

public class ArtefactUIService : IInitializable, IDisposable
{
    private readonly UIDocument _ui;
    private readonly SignalBus _bus;

    private VisualElement _root;
    private VisualElement _artefactContainer;

    private VisualElement _wrapper;

    private ScrollView _descriptionScrollView;
    private ScrollView _factsScrollView;

    private Label _titleLabel;
    private Label _descriptionLabel;
    private Label _factsLabel;
    private Button _closeButton;

    private const string ARTEFACT_UI_DISABLED_CLASS = "artefactData__wrapper_disabled";
    private const string ARTEFACT_CLOSE_BUTTON_HIDDEN_CLASS = "artefactData__closeButton_disabled";
    private const string ARTEFACT_FACT_HIDDEN = "artefactData__factText_hidden";

    private bool _hideTransitionCallbackRegistered = false;
    private bool _isPointed = false;
    private int _scrollHoverCount = 0;

    private ArtifactObject _currentArtifact;
    private VisualTreeAsset _factEntryTemplate;

    private const string ARTEFACT_FACTS_LABEL_TEXT = "Фактов открыто: ";
    private int _factsOpened = 0;

    public ArtefactUIService(UIDocument ui,
                             SignalBus bus)
    {
        _ui = ui;
        _bus = bus;
    }

    public void Initialize()
    {
        _root = _ui.rootVisualElement;
        _artefactContainer = _root.Q<VisualElement>("ArtefactContainer");

        _wrapper = _artefactContainer.Q<VisualElement>("ArtefactDataWrapper");
        _closeButton = _artefactContainer.Q<Button>("ArtefactDataCloseButton");

        _titleLabel = _wrapper.Q<Label>("ArtefactDataNameText");
        _descriptionLabel = _wrapper.Q<Label>("ArtefactDataDescriptionText");

        _descriptionScrollView = _wrapper.Q<ScrollView>("ArtefactDataDescriptionScrollView");
        _factsScrollView = _wrapper.Q<ScrollView>("ArtefactDataFactsScrollView");

        _factsLabel = _wrapper.Q<Label>("ArtefactDataFactsLabel");
        
        var factProto = _factsScrollView.Q<TemplateContainer>("ArtefactDataFactText");
        _factEntryTemplate = factProto.templateSource;
        factProto.style.display = DisplayStyle.None;

        _descriptionLabel.enableRichText = true;
        _descriptionLabel.style.unityTextAlign = TextAnchor.UpperLeft;

        _closeButton.clicked += OnCloseButtonClicked;

        SetCloseButtonPicking(false);

        RegisterScrollHoverCallbacks(_descriptionScrollView);
        RegisterScrollHoverCallbacks(_factsScrollView);

        _bus.Subscribe<ArtefactModeSignal>(ToggleArtefactUI);
        _bus.Subscribe<ArtefactInterestPointFound>(ShowFactByIndex);

        if (_wrapper.ClassListContains(ARTEFACT_UI_DISABLED_CLASS))
            _artefactContainer.style.display = DisplayStyle.None;
    }

    public void Dispose()
    {
        _closeButton.clicked -= OnCloseButtonClicked;

        _bus.Unsubscribe<ArtefactModeSignal>(ToggleArtefactUI);
    }

    private void ToggleArtefactUI(ArtefactModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(isActive)
        {
            _currentArtifact = signal.Artifact;
            ArtifactData data = _currentArtifact.ArtifactData;

            _titleLabel.text = data.Name;
            _descriptionLabel.text = $"<align=\"justified\">{data.Description}</align>";

            _descriptionScrollView.verticalScroller.value = 0f;
            _factsScrollView.verticalScroller.value = 0f;

            ClearAndInsertFacts();

            _factsLabel.text = ARTEFACT_FACTS_LABEL_TEXT + _factsOpened.ToString();

            SetArtefactUIVisible(true);
        }
        else
        {
            _factsOpened = 0;
            
            SetArtefactUIVisible(false);
        }
    }

    private void ClearAndInsertFacts()
    {
        _factsScrollView.Clear();

        int factsLength = _currentArtifact.ArtifactData.Facts.Count;
        for(int i = 0; i < factsLength; i++)
        {
            var fact = _currentArtifact.ArtifactData.Facts[i];

            var factEntry = _factEntryTemplate.Instantiate();
            factEntry.Q<Label>("ArtefactDataFactText").text = fact;
            _factsScrollView.contentContainer.Add(factEntry);

            if(_currentArtifact.InterestPointsFound[i])
            {
                var child = factEntry.ElementAt(0);
                child.RemoveFromClassList(ARTEFACT_FACT_HIDDEN);

                _factsOpened++;
            }
        }
    }

    private void ShowFactByIndex(ArtefactInterestPointFound signal)
    {
        int idx = signal.Index;
        var child = _factsScrollView.ElementAt(idx).ElementAt(0);
        child.RemoveFromClassList(ARTEFACT_FACT_HIDDEN);

        _factsOpened++;
        _factsLabel.text = ARTEFACT_FACTS_LABEL_TEXT + _factsOpened.ToString();;

        _currentArtifact.FindInterestPoint(idx);
    }

    private void SetArtefactUIVisible(bool visible)
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
            _wrapper.UnregisterCallback<TransitionEndEvent>(OnArtefactHideTransitionEnd);
            _hideTransitionCallbackRegistered = false;
        }

        _artefactContainer.style.display = DisplayStyle.Flex;

        _wrapper.AddToClassList(ARTEFACT_UI_DISABLED_CLASS);
        _closeButton.AddToClassList(ARTEFACT_CLOSE_BUTTON_HIDDEN_CLASS);
        SetCloseButtonPicking(false);

        _wrapper.schedule.Execute(() =>
        {
            _wrapper.RemoveFromClassList(ARTEFACT_UI_DISABLED_CLASS);
            _closeButton.RemoveFromClassList(ARTEFACT_CLOSE_BUTTON_HIDDEN_CLASS);
            SetCloseButtonPicking(true);
        });
    }

    private void HideAnimatedToDisplayNone()
    {
        if (_artefactContainer.style.display.value == DisplayStyle.None)
            return;

        SetCloseButtonPicking(false);

        if (!_hideTransitionCallbackRegistered)
        {
            _wrapper.RegisterCallback<TransitionEndEvent>(OnArtefactHideTransitionEnd);
            _hideTransitionCallbackRegistered = true;
        }

        _scrollHoverCount = 0;
        if (_isPointed)
        {
            _bus.Fire(new ArtefactScrollPointedSignal(false));
            _isPointed = false;
        }

        _wrapper.AddToClassList(ARTEFACT_UI_DISABLED_CLASS);
        _closeButton.AddToClassList(ARTEFACT_CLOSE_BUTTON_HIDDEN_CLASS);
    }

    private void OnArtefactHideTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target != _wrapper)
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

        _artefactContainer.style.display = DisplayStyle.None;

        _wrapper.UnregisterCallback<TransitionEndEvent>(OnArtefactHideTransitionEnd);
        _hideTransitionCallbackRegistered = false;
    }

    private void OnCloseButtonClicked()
    {
        ArtefactModeSignal artefactSignal = new()
        {
            IsActive = false,
            Artifact = null
        };

        SearchModeSignal searchSignal = new()
        {
            IsActive = true
        };

        _bus.Fire(artefactSignal);
        _bus.Fire(searchSignal);
    }

    private void RegisterScrollHoverCallbacks(ScrollView sv)
    {
        sv.RegisterCallback<PointerEnterEvent>(OnScrollEnter, TrickleDown.TrickleDown);
        sv.RegisterCallback<PointerLeaveEvent>(OnScrollLeave, TrickleDown.TrickleDown);

        // sv.RegisterCallback<WheelEvent>(e => e.StopImmediatePropagation(), TrickleDown.TrickleDown);
    }

    private void OnScrollEnter(PointerEnterEvent _)
    {
        _scrollHoverCount++;
        if (_scrollHoverCount == 1)
        {
            _bus.Fire(new ArtefactScrollPointedSignal(true));
            _isPointed = true;
        }
    }

    private void OnScrollLeave(PointerLeaveEvent _)
    {
        _scrollHoverCount = Mathf.Max(0, _scrollHoverCount - 1);
        if (_scrollHoverCount == 0)
        {
            _bus.Fire(new ArtefactScrollPointedSignal(false));
            _isPointed = false;
        }
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
}