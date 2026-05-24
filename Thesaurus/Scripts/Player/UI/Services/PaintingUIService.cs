using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public sealed class PaintingUIService : IInitializable, IDisposable, ITickable
{
    private readonly UIDocument _ui;
    private readonly SignalBus _bus;

    private VisualElement _root;
    private VisualElement _paintingContainer;
    private VisualElement _paintingNameLabelContainer;
    private Label _paintingNameLabel;
    private VisualElement _factsPointsContainer;

    private VisualTreeAsset _interestPointTemplate;
    private readonly Dictionary<PaintingInterestPointObject, VisualElement> _instantiatedInterestPoints = new();
    private readonly Dictionary<VisualElement, VisualElement> _instantiatedFacts = new();
    private VisualTreeAsset _factTemplate;

    private Button _closeButton;

    private bool _hideTransitionCallbackRegistered = false;

    private const string PAINTING_LABEL_HIDDEN = "paintingObserve__nameContainer_hidden";
    private const string PAINTING_CLOSE_BUTTON_HIDDEN = "paintingObserve__closeButton_disabled";
    private const string PAINTING_INTEREST_POINT_HIDDEN = "paintingObserve__interestPoint_hidden";
    private const string PAINTING_INTEREST_POINT_CLASS = "paintingObserve__interestPointContainer";
    private const string PAINTING_FACT_HIDDEN = "paintingObserve__factContainer_hidden";
    private const string PAINTING_FACT_TEMPLATE_CLASS = "paintingObserve__factTemplate";

    private bool _isInspecting = false;

    public PaintingUIService(UIDocument ui,
                             SignalBus bus)
    {
        _ui = ui;
        _bus = bus;
    }

    public void Initialize()
    {
        _root = _ui.rootVisualElement;
        _paintingContainer = _root.Q<VisualElement>("PaintingContainer");
        
        var painting = _paintingContainer.Q<VisualElement>("Painting");
        
        _paintingNameLabelContainer = painting.Q<VisualElement>("PaintingObserveNameContainer");
        _paintingNameLabel = _paintingNameLabelContainer.Q<Label>("PaintingObserveNameText");

        _closeButton = painting.Q<Button>("PaintingObserveCloseButton");
        _closeButton.clicked += OnCloseButtonClicked;
        
        SetCloseButtonPicking(false);

        var interestPointProto = _paintingContainer.Q<TemplateContainer>("PaintingObserveInterestPoint");
        _interestPointTemplate = interestPointProto.templateSource;
        interestPointProto.style.display = DisplayStyle.None;

        var factProto = _paintingContainer.Q<TemplateContainer>("PaintingObserveFactContainer");
        _factTemplate = factProto.templateSource;
        factProto.style.display = DisplayStyle.None;

        _factsPointsContainer = painting.Q<VisualElement>("PaintingFactsAndPointsContainer");

        _bus.Subscribe<PaintingModeSignal>(TogglePaintingUI);

        if(_paintingNameLabelContainer.ClassListContains(PAINTING_LABEL_HIDDEN))
            _paintingContainer.style.display = DisplayStyle.None;

        _root.RegisterCallback<PointerDownEvent>(OnGlobalPointerDown, TrickleDown.TrickleDown);
    }

    public void Dispose()
    {
        _closeButton.clicked -= OnCloseButtonClicked;
        _bus.Unsubscribe<PaintingModeSignal>(TogglePaintingUI);

        if (_hideTransitionCallbackRegistered)
        {
            _paintingNameLabelContainer.UnregisterCallback<TransitionEndEvent>(OnPaintingHideTransitionEnd);
            _hideTransitionCallbackRegistered = false;
        }
    }

    public void Tick()
    {
        if(!_isInspecting)
            return;

        foreach(var kv in _instantiatedInterestPoints)
        {
            UpdateInterestPointPosition(kv.Key.Collider, kv.Value);
        }

        foreach (var kv in _instantiatedFacts)
        {
            UpdateFactPosition(kv.Key, kv.Value, 16f);
        }
    }

    private void TogglePaintingUI(PaintingModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(isActive)
        {
            PaintingObject painting = signal.Painting;

            _paintingNameLabel.text = painting.PaintingData.Name;

            ClearInterestPoints();
            InstantiateInterestPoints(painting.InterestPoints);

            SetPaintingUIVisible(true);

            _isInspecting = true;
        }
        else
        {
            SetPaintingUIVisible(false);

            _isInspecting = false;
        }
    }

    private void InstantiateInterestPoints(PaintingInterestPointObject[] interestPoints)
    {
        for(int i = 0; i < interestPoints.Length; i++)
        {
            var interestPointData = interestPoints[i];

            var interestPoint = _interestPointTemplate.Instantiate();
            interestPoint.style.position = Position.Absolute;
            _factsPointsContainer.Add(interestPoint);

            interestPoint.RegisterCallback<GeometryChangedEvent>(OnGeo);
            void OnGeo(GeometryChangedEvent _)
            {
                interestPoint.UnregisterCallback<GeometryChangedEvent>(OnGeo);
                UpdateInterestPointPosition(interestPointData.Collider, interestPoint);
            }

            _instantiatedInterestPoints.Add(interestPointData, interestPoint);

            interestPoint.AddToClassList(PAINTING_INTEREST_POINT_CLASS);

            var interestPointContainer = interestPoint.Q<VisualElement>("PaintingObserveInterestPoint");
            interestPointContainer.RemoveFromClassList(PAINTING_INTEREST_POINT_HIDDEN);

            int idx = i;
            interestPointContainer.AddManipulator(new Clickable(() => InstantiateFact(interestPointData)));
        }
    }

    private void ClearInterestPoints()
    {
        foreach (var kv in _instantiatedInterestPoints)
            kv.Value.RemoveFromHierarchy();

        _instantiatedInterestPoints.Clear();
    }

    private void UpdateInterestPointPosition(Collider col, VisualElement point)
    {
        if (col == null) { point.style.display = DisplayStyle.None; return; }

        var cam = Camera.main;
        if (cam == null) { point.style.display = DisplayStyle.None; return; }

        var panel = point.panel ?? _ui.rootVisualElement.panel;
        if (panel == null) return;

        Vector2 panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(panel, col.bounds.center, cam); 
        Vector2 local = _factsPointsContainer.WorldToLocal(panelPos);

        var size = point.worldBound.size;
        point.style.left = local.x - size.x * 0.5f;
        point.style.top  = local.y - size.y * 0.5f;

        point.style.display = DisplayStyle.Flex;
    }

    private void InstantiateFact(PaintingInterestPointObject interestPointData)
    {
        var pointElm = _instantiatedInterestPoints[interestPointData];

        if(_instantiatedFacts.ContainsKey(pointElm))
            return;

        var fact = _factTemplate.Instantiate();
        _instantiatedFacts.Add(pointElm, fact);

        var factContainer = fact.Q<VisualElement>("PaintingObserveFactContainer");
        fact.Q<Label>("PaintingObserveFactText").text = interestPointData.InterestData.Description;

        fact.style.position = Position.Absolute;
        _factsPointsContainer.Add(fact);

        fact.RegisterCallback<GeometryChangedEvent>(OnGeo);
        void OnGeo(GeometryChangedEvent _)
        {
            fact.UnregisterCallback<GeometryChangedEvent>(OnGeo);

            var centerLocal = _factsPointsContainer.WorldToLocal(pointElm.worldBound.center);
            var bottomLocalY = _factsPointsContainer.WorldToLocal(pointElm.worldBound.position).y + pointElm.worldBound.height;

            float factW = fact.worldBound.width;
            fact.style.left = centerLocal.x - factW * 0.5f;
            fact.style.top  = bottomLocalY + 16f;

            factContainer.AddToClassList(PAINTING_FACT_HIDDEN);

            fact.schedule.Execute(() => factContainer.RemoveFromClassList(PAINTING_FACT_HIDDEN));
        }
    }

    private bool IsInsideFact(VisualElement ve)
    {
        for (var e = ve; e != null; e = e.parent)
        {
            if (e.ClassListContains(PAINTING_FACT_TEMPLATE_CLASS))
                return true;
        }
        return false;
    }

    private void OnGlobalPointerDown(PointerDownEvent evt)
    {
        if (evt.target is VisualElement ve && IsInsideFact(ve))
            return; 

        ClearFacts();
    }

    private void ClearFacts()
    {
        var snapshot = new List<KeyValuePair<VisualElement, VisualElement>>(_instantiatedFacts);

        foreach (var kv in snapshot)
            HideAndRemoveFact(kv.Key, kv.Value);
    }

    private void UpdateFactPosition(VisualElement pointElm, VisualElement fact, float yOffset = 16f)
    {
        if (pointElm == null || fact == null)
            return;

        if (pointElm.resolvedStyle.display == DisplayStyle.None)
        {
            fact.style.display = DisplayStyle.None;
            return;
        }

        fact.style.position = Position.Absolute;

        var centerLocal = _factsPointsContainer.WorldToLocal(pointElm.worldBound.center);

        var bottomLocalY = _factsPointsContainer.WorldToLocal(pointElm.worldBound.position).y + pointElm.worldBound.height;

        float factW = fact.worldBound.width;
        if (factW <= 0f) factW = fact.resolvedStyle.width; 

        fact.style.left = centerLocal.x - factW * 0.5f;
        fact.style.top  = bottomLocalY + yOffset;

        fact.style.display = DisplayStyle.Flex;
}


    private void HideAndRemoveFact(VisualElement key, VisualElement fact)
    {
        if (fact == null)
        {
            _instantiatedFacts.Remove(key);
            return;
        }

        var animTarget = fact.Q<VisualElement>("PaintingObserveFactContainer") ?? fact;

        if (animTarget.ClassListContains(PAINTING_FACT_HIDDEN))
            return;

        void OnEnd(TransitionEndEvent evt)
        {
            if (evt.target != animTarget) return;

            bool opacityEnded = false;
            foreach (var p in evt.stylePropertyNames)
            {
                if (p.ToString() == "opacity")
                {
                    opacityEnded = true;
                    break;
                }
            }
            if (!opacityEnded) return;

            animTarget.UnregisterCallback<TransitionEndEvent>(OnEnd);
            fact.RemoveFromHierarchy();
            _instantiatedFacts.Remove(key);
        }

        animTarget.RegisterCallback<TransitionEndEvent>(OnEnd);
        animTarget.AddToClassList(PAINTING_FACT_HIDDEN);
    }


    private void SetPaintingUIVisible(bool visible)
    {
        if(visible)
            ShowAnimated();
        else
            HideAnimatedToDisplayNone();
    }

    private void ShowAnimated()
    {
        if(_hideTransitionCallbackRegistered)
        {
            _paintingNameLabelContainer.UnregisterCallback<TransitionEndEvent>(OnPaintingHideTransitionEnd);
            _hideTransitionCallbackRegistered = false;
        }

        _paintingContainer.style.display = DisplayStyle.Flex;

        _paintingNameLabelContainer.AddToClassList(PAINTING_LABEL_HIDDEN);
        _closeButton.AddToClassList(PAINTING_CLOSE_BUTTON_HIDDEN);
        
        _paintingContainer.schedule.Execute(() =>
        {
            _paintingNameLabelContainer.RemoveFromClassList(PAINTING_LABEL_HIDDEN);
            _closeButton.RemoveFromClassList(PAINTING_CLOSE_BUTTON_HIDDEN);
            SetCloseButtonPicking(true);
        });
    }

    private void HideAnimatedToDisplayNone()
    {
        if(_paintingContainer.style.display == DisplayStyle.None)
            return;

        SetCloseButtonPicking(false);

        if(!_hideTransitionCallbackRegistered)
        {
            _paintingNameLabelContainer.RegisterCallback<TransitionEndEvent>(OnPaintingHideTransitionEnd);
            _hideTransitionCallbackRegistered = true;
        }

        _paintingNameLabelContainer.AddToClassList(PAINTING_LABEL_HIDDEN);
        _closeButton.AddToClassList(PAINTING_CLOSE_BUTTON_HIDDEN);

        var point = _instantiatedInterestPoints.Values;
        foreach(var p in point)
        {
            p.AddToClassList(PAINTING_INTEREST_POINT_HIDDEN);
        }
    }

    private void OnPaintingHideTransitionEnd(TransitionEndEvent evt)
    {
        if(evt.target != _paintingNameLabelContainer)
            return;

        bool opacityEnded = false;
        foreach(var p in evt.stylePropertyNames)
        {
            if(p.ToString() == "opacity")
            {
                opacityEnded = true;
                break;
            }
        }
        if(!opacityEnded)
            return;

        _paintingContainer.style.display = DisplayStyle.None;
        _paintingNameLabelContainer.UnregisterCallback<TransitionEndEvent>(OnPaintingHideTransitionEnd);
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
        PaintingModeSignal paintSignal = new PaintingModeSignal()
        {
            IsActive = false,
            Painting = null
        };

        SearchModeSignal searchSignal = new SearchModeSignal()
        {
            IsActive = true
        };

        _bus.Fire(paintSignal);
        _bus.Fire(searchSignal);
    }
}