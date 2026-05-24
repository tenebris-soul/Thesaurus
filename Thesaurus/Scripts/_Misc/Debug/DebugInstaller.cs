using Zenject;

public class DebugGizmosInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<GizmosSphereService>().AsSingle();
    }
}
