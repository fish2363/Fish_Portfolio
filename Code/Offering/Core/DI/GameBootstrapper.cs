using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using VContainer.Unity;

public sealed class GameBootstrapper : IAsyncStartable, IDisposable
{
    private readonly IResourceService resourceService;
    private readonly IUIService uiService;
    private readonly IGameNetworkService gameNetworkService;
    private readonly IInputService inputService;
    private readonly IVolumeService volumeService;

    public GameBootstrapper(
        IResourceService resourceService,
        IUIService uiService,
        IGameNetworkService gameNetworkService,IInputService inputService, IVolumeService volumeService)
    {
        this.resourceService = resourceService;
        this.uiService = uiService;
        this.gameNetworkService = gameNetworkService;
        this.inputService = inputService;
        this.volumeService = volumeService;
    }

    public async UniTask StartAsync(CancellationToken cancellation = default)
    {
        DOTween.Init(true, true).SetCapacity(2000, 100);

        await resourceService.InitAsync().AttachExternalCancellation(cancellation);
        await uiService.InitAsync().AttachExternalCancellation(cancellation);
        await gameNetworkService.InitAsync().AttachExternalCancellation(cancellation);
        await inputService.InitAsync().AttachExternalCancellation(cancellation);
        await volumeService.InitAsync().AttachExternalCancellation(cancellation);
    }

    public void Dispose()
    {
        gameNetworkService.Release();
        uiService.Release();
        resourceService.Release();
        inputService.Release();
    }
}
