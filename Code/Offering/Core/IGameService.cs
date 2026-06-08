using Cysharp.Threading.Tasks;

public interface IGameService
{
    bool IsInitialized { get; }
    UniTask InitAsync();
    void Release();
}

