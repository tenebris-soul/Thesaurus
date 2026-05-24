using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;

public class PlayerInstaller : MonoInstaller
{
    [SerializeField] private InputActionAsset _actionAsset;

    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private Transform _cameraAttach;
    [SerializeField] private Transform _cameraVisuals;
    [SerializeField] private PlayerInput _playerInput;

    [SerializeField] private CapsuleCollider _playerCollider;
    [SerializeField] private Rigidbody _playerRigidbody;
    [SerializeField] private LayerMask _worldMask;

    [SerializeField] private AudioSource _footAudioSource;

    [SerializeField] private UIDocument _ui;

    public override void InstallBindings()
    {
        // signals
        SignalBusInstaller.Install(this.Container);

        this.Container.DeclareSignal<WalkingSignal>();
        this.Container.DeclareSignal<StandingSignal>();

        this.Container.DeclareSignal<JumpingStartedSignal>();
        this.Container.DeclareSignal<LandingEndedSignal>();

        this.Container.DeclareSignal<StepEmitted>();
        this.Container.DeclareSignal<StepDone>();

        this.Container.DeclareSignal<SlidingSignal>();
        this.Container.DeclareSignal<SlidingEndedSignal>();

        this.Container.DeclareSignal<HeadChangePositionSignal>();

        this.Container.DeclareSignal<InteractableFoundSignal>();
        this.Container.DeclareSignal<InteractableLostSignal>();
        
        this.Container.DeclareSignal<SearchModeSignal>();
        this.Container.DeclareSignal<InspectModeSignal>();
        this.Container.DeclareSignal<ArtefactModeSignal>();
        this.Container.DeclareSignal<PaintingModeSignal>();

        this.Container.DeclareSignal<ArtefactScrollPointedSignal>();
        this.Container.DeclareSignal<ArtefactInterestPointFound>();

        // input system
        this.Container
            .BindInterfacesAndSelfTo<GameplayInputContext>()
            .AsSingle()
            .WithArguments(_actionAsset);

        this.Container
            .BindInterfacesAndSelfTo<InspectInputContext>()
            .AsSingle()
            .WithArguments(_actionAsset);

        this.Container
            .BindInterfacesAndSelfTo<ArtifactInputContext>()
            .AsSingle()
            .WithArguments(_actionAsset);

        this.Container
            .BindInterfacesAndSelfTo<PaintingInputContext>()
            .AsSingle()
            .WithArguments(_actionAsset);

        this.Container
            .BindInterfacesAndSelfTo<InputRouter>()
            .AsSingle()
            .NonLazy();

        // additional to player motor

        // contexts
        this.Container
            .BindInterfacesAndSelfTo<PlayerContext>()
            .AsSingle();

        // services
        this.Container
            .Bind<ICapsuleGeometryService>()
            .To<CapsuleGeometryService>()
            .AsSingle()
            .WithArguments(_playerRigidbody, _playerCollider);

        this.Container
            .Bind<ISlopeChecker>()
            .To<SlopeChecker>()
            .AsSingle();

        this.Container
            .Bind<IGroundingService>()
            .To<GroundingService>()
            .AsSingle()
            .WithArguments(_playerCollider, _worldMask);

        this.Container
            .Bind<ICeilingCheckService>()
            .To<CeilingCheckService>()
            .AsSingle()
            .WithArguments(_playerCollider, _worldMask);

        this.Container
            .Bind<IStepUpService>()
            .To<StepUpService>()
            .AsSingle()
            .WithArguments(_playerCollider, _worldMask);

        this.Container
            .Bind<IKinematicMoveService>()
            .To<KinematicMoveService>()
            .AsSingle()
            .WithArguments(_playerCollider, _worldMask);

        this.Container
            .BindInterfacesAndSelfTo<PlayerRotation>()
            .AsSingle()
            .WithArguments(_cameraAttach, _playerRigidbody)
            .NonLazy();

        this.Container
            .Bind<IPlatformMotionService>()
            .To<PlatformMotionService>()
            .AsSingle();

        this.Container
            .Bind<ICapsuleDepenetration>()
            .To<CapsuleDepenetration>()
            .AsSingle()
            .WithArguments(_playerCollider, _worldMask);

        // camera visuals
        this.Container
            .BindInterfacesAndSelfTo<HeadMoverService>()
            .AsSingle()
            .WithArguments(_cameraVisuals)
            .NonLazy();

        this.Container
            .BindInterfacesAndSelfTo<HeadMoverHandler>()
            .AsSingle()
            .NonLazy();
            
        this.Container
            .BindInterfacesAndSelfTo<HeadBobbingService>()
            .AsSingle()
            .WithArguments(_cameraVisuals)
            .NonLazy();

        this.Container
            .BindInterfacesAndSelfTo<HeadSpringService>()
            .AsSingle()
            .WithArguments(_cameraVisuals)
            .NonLazy();

        // sound effects
        this.Container
            .BindInterfacesAndSelfTo<FootstepEmitterService>()
            .AsSingle()
            .WithArguments(_footAudioSource)
            .NonLazy();

        this.Container
            .BindInterfacesAndSelfTo<SlidingSoundService>()
            .AsSingle()
            .WithArguments(_footAudioSource)
            .NonLazy();
        
        // camera additionals
        this.Container
            .Bind<ICameraRayProvider>()
            .To<CameraRayProvider>()
            .AsSingle()
            .WithArguments(_cameraRoot);

        this.Container
            .BindInterfacesAndSelfTo<CameraExhibitInteractor>()
            .AsSingle()
            .NonLazy();

        // camera motor
        this.Container
            .BindInterfacesAndSelfTo<CameraMotor>()
            .AsSingle()
            .WithArguments(_cameraRoot, _cameraAttach);

        // camera state machine
        this.Container
            .BindInterfacesAndSelfTo<PlayerCameraStateMachine>()
            .AsSingle()
            .NonLazy();

        // player motor
        this.Container
            .BindInterfacesAndSelfTo<PlayerMotor>()
            .AsSingle()
            .WithArguments(_playerRigidbody, 
                           _playerCollider
                          );

        // locomotion state machine
        this.Container
            .BindInterfacesAndSelfTo<PlayerLocomotionStateMachine>()
            .AsSingle()
            .NonLazy();

        // ui
        this.Container
            .BindInterfacesAndSelfTo<CrosshairUIService>()
            .AsSingle()
            .WithArguments(_ui)
            .NonLazy();

        this.Container
            .BindInterfacesAndSelfTo<ExhibitUIService>()
            .AsSingle()
            .WithArguments(_ui)
            .NonLazy();

        this.Container
            .BindInterfacesAndSelfTo<ArtefactUIService>()
            .AsSingle()
            .WithArguments(_ui)
            .NonLazy();

        this.Container
            .BindInterfacesAndSelfTo<PaintingUIService>()
            .AsSingle()
            .WithArguments(_ui)
            .NonLazy();
        
        // mode coordinator
        this.Container
            .BindInterfacesAndSelfTo<ModeCoordinator>()
            .AsSingle()
            .NonLazy();
            
        // modes
        this.Container
            .BindInterfacesAndSelfTo<ExhibitInspectionService>()
            .AsSingle()
            .WithArguments(_cameraAttach)
            .NonLazy();

        this.Container
            .BindInterfacesAndSelfTo<ArtefactInspectionService>()
            .AsSingle()
            .WithArguments(_cameraAttach, _camera, _worldMask)
            .NonLazy();

        this.Container
            .BindInterfacesAndSelfTo<PaintingInspectionService>()
            .AsSingle()
            .WithArguments(_cameraAttach, _camera)
            .NonLazy();
    }
}
