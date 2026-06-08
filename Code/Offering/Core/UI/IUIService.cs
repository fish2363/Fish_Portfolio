using Cysharp.Threading.Tasks;

public interface IUIService : IGameService
{
    public void Hide<T>(params object[] param) where T : UIBase;
    public T Get<T>() where T : UIBase;
    public bool IsOpened<T>() where T : UIBase;
    public UniTask<T> Show<T>(params object[] param) where T : UIBase;
}

