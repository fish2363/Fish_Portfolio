using Ami.BroAudio;
using Cysharp.Threading.Tasks;
using ManagingSystem;
using UnityEngine;

public enum EGameState
{
    None = 0,
    MainMenu = 1,
    Tutorial = 2,
    StartRoom = 3,
    Loading = 4,
    Exploration = 5,
    Combat = 6,
    EventRoom = 7,
    BossRoom = 8,
    Cinematic = 9,
    GameOver = 10,
    Error = 11        // 초기화 실패 등 비정상 상태
}

public class GameFlowManager : BaseManager<GameFlowManager>
{
    [field: SerializeField] public EGameState DefaultState { get; private set; }

    public NotifyValue<EGameState> State { get; } = new NotifyValue<EGameState>(EGameState.None);
    public EGameState CurrentState => State.Value;

    [Header("BGM")]
    [SerializeField] private SoundID mainMenuBGM;
    [SerializeField] private SoundID tutorialBGM;
    [SerializeField] private SoundID combatBGM;
    [SerializeField] private SoundID eventRoomBGM;
    [SerializeField] private SoundID bossBGM;

    private SoundID _currentBGM = SoundID.Invalid;

    protected override void Awake()
    {
        base.Awake();
        
        State.OnValueChanged += HandleStateChanged;
    }

    public bool CheckCurrentState(EGameState state)
    {
        return State.Value == state;
    }

    private void HandleStateChanged(EGameState prev, EGameState next)
        => ChangeBGM(next);

    public void ChangeState(EGameState newState)
        => State.Value = newState;

    private void ChangeBGM(EGameState state)
    {
        SoundID nextBGM = GetBGM(state);
        if (_currentBGM.Equals(nextBGM)) return;

        StopCurrentBGM();

        if (nextBGM.IsValid())
            BroAudio.Play(nextBGM);

        _currentBGM = nextBGM;
    }

    private SoundID GetBGM(EGameState state) => state switch
    {
        EGameState.MainMenu => mainMenuBGM,
        EGameState.Tutorial => tutorialBGM,
        EGameState.Combat => combatBGM,
        EGameState.EventRoom => eventRoomBGM,
        EGameState.BossRoom => bossBGM,
        _ => SoundID.Invalid
    };

    private void StopCurrentBGM()
    {
        if (_currentBGM.IsValid())
            BroAudio.Stop(_currentBGM);
    }
}