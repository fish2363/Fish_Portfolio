using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

public class MainSceneLifetimeScope : LifetimeScope
{
    protected override LifetimeScope FindParent()
    {
        return LifetimeScope.Find<GameLifetimeScope>();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<ScoreService>(Lifetime.Scoped)
            .As<IScoreService>();

        builder.RegisterComponentInHierarchy<PlayerSpawner>();
        builder.RegisterComponentInHierarchy<ScoreView>();
        builder.RegisterComponentInHierarchy<CameraManager>()
            .As<ICameraService>();

        builder.RegisterEntryPoint<MainSceneBootstrapper>();
    }
}

public sealed class MainSceneBootstrapper : IAsyncStartable
{
    private readonly IScoreService scoreService;

    public MainSceneBootstrapper(IScoreService scoreService)
    {
        this.scoreService = scoreService;
    }

    public async UniTask StartAsync(CancellationToken cancellation = default)
    {
        await scoreService.InitAsync().AttachExternalCancellation(cancellation);
    }
}
