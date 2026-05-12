using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagingSystem;
using Map.Managers;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField] private StopInfoSO _stopInfoSO;
    [SerializeField] private EGameState defalutState = EGameState.MainMenu;
    public delegate void UnityEventListener();
    public event UnityEventListener OnStartEvent = null;

    public bool InPause { get; set; }
    private bool isCombat = false;
    protected override void Awake()
    {
        base.Awake();

        DOTween.Init(true, true, LogBehaviour.Verbose).SetCapacity(2000, 100);

        InitManagers().Forget();
    }
    private async UniTask InitManagers()
    {
        await ResourceManager.Instance.InitAsync();
        await DataManager.Instance.InitAsync();

        await UniTask.WhenAll(
                UIManager.Instance.InitAsync(),
                AudioManager.Instance.InitAsync(),
                InputManager.Instance.InitAsync(),
                TimelineManager.Instance.InitAsync(),
                VolumeManager.Instance.InitAsync(),
                CursorManager.Instance.InitAsync()
            );

        GameFlowManager.Instance.ChangeState(defalutState);

        OnStartEvent?.Invoke();
    }

    public void Pause()
    {
        InPause = true;
        StopManager.Instance.GenerateStop(_stopInfoSO);

        InputManager.Instance.SetEnableInputOnly(EInputCategory.UI, true);
    }

    public void Resume(bool stateSettingSelf = true)
    {
        if (!InPause) return;

        if (stateSettingSelf)
        {
            InPause = false;
        }

        StopManager.Instance.ReleaseStop(StopChannel.UI);

        InputManager.Instance.SetEnableInputOnly(EInputCategory.Player, true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}