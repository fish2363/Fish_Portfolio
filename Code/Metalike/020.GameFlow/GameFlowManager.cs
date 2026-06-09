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

    // 게임 상태를 NotifyValue로 관리한다.
    // 상태가 바뀌면 이를 구독한 시스템들이 (prev, next)를 받아 자동으로 반응한다.
    public NotifyValue<EGameState> State { get; } = new NotifyValue<EGameState>(EGameState.None);

    // 외부 읽기 편의용
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
        // BGM도 상태 변경에 반응하는 '하나의 구독자'로 처리.
        // 자기 자신의 State를 자기 메서드로 구독하므로 별도 해제는 불필요(수명 동일).
        State.OnValueChanged += HandleStateChanged;
        // 상태 진입(ChangeState)은 Awake가 아니라 모든 Manager 초기화가 끝난 뒤
        // GameManager.InitManagers에서 호출한다.
    }

    public void ChangeState(EGameState newState)
    {
        // 같은 값이면 NotifyValue가 이벤트를 발생시키지 않는다.
        State.Value = newState;
    }

    public bool CheckCurrentState(EGameState state)
    {
        return State.Value == state;
    }

    // 상태 변경 반응 (BGM 전환)
    private void HandleStateChanged(EGameState prev, EGameState next)
    {
        ChangeBGM(next);
    }

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