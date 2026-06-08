using VContainer;
using VContainer.Unity;
using UnityEngine;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<ResourceService>(Lifetime.Singleton)
            .As<IResourceService>();

        builder.RegisterComponentInHierarchy<PoolManagerMono>()
        .As<IPoolService>();

        builder.RegisterComponentInHierarchy<SoundManager>();
        builder.RegisterComponentInHierarchy<UIManager>()
            .As<IUIService>();
        builder.RegisterComponentInHierarchy<InputService>()
    .As<IInputService>();
        builder.RegisterComponentInHierarchy<VolumeManager>()
    .As<IVolumeService>();

        builder.Register<GameNetworkManager>(Lifetime.Singleton)
            .As<IGameNetworkService>();

        builder.RegisterEntryPoint<GameBootstrapper>();
    }
}
