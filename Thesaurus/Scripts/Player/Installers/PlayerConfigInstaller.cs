using Unity.VisualScripting;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "PlayerConfigInstaller", menuName = "Installers/PlayerConfigInstaller")]
public class PlayerConfigInstaller : ScriptableObjectInstaller<PlayerConfigInstaller>
{
    [SerializeField] private PlayerMovementConfig _moveConfig;
    [SerializeField] private PlayerMovementTuning _moveTuning;
    [SerializeField] private PlayerHeadMoveConfig _headMoveConfig;
    [SerializeField] private PlayerSoundsConfig _footstepConfig;
    public override void InstallBindings()
    {
        this.Container
            .Bind<PlayerMovementConfig>()
            .FromInstance(_moveConfig)
            .AsSingle();

        this.Container
            .Bind<PlayerMovementTuning>()
            .FromInstance(_moveTuning)
            .AsSingle();

        this.Container
            .Bind<PlayerHeadMoveConfig>()
            .FromInstance(_headMoveConfig)
            .AsSingle();

        this.Container
            .Bind<PlayerSoundsConfig>()
            .FromInstance(_footstepConfig)
            .AsSingle();
    }
}