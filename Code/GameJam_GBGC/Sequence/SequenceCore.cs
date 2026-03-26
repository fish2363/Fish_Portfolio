using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ami.BroAudio;

public class SequenceCore : MonoBehaviour
{
    [Header("Actors & Environment")]
    [SerializeField] private List<SequenceActorBase> actors = new();
    [SerializeField] private List<Trofical> troficals = new();

    [Header("Selection Setting")]
    [SerializeField] private int maxSelectCount = 5;
    [SerializeField] private SoundID click;

    [Header("UI & Balloons")]
    [SerializeField] private Dialogue[] defaultBalloon;
    [SerializeField] private TaskText[] taskTexts;
    [SerializeField] private GameEventChannelSO inGameEvent;

    // 상태 및 리스트
    public static bool IsTutorial;
    private readonly List<CharacterBtn> selectedButtons = new();
    private readonly List<ESequenceCharacter> selectedOrder = new();
    private Dictionary<ESequenceCharacter, SequenceActorBase> actorLookup;
    private readonly List<SequenceActorBase> executedActors = new();
    private bool isClear = false;

    private void Awake()
    {
        InitializeActors();
    }

    public void GameStart()
    {
        ComeUpCardUIEvent comeUpCardUI = InGameEvent.GameStartEvent;
        inGameEvent?.RaiseEvent(comeUpCardUI);

        ShowDefaultBalloons(true);
        ResetTaskUI();
    }

    private void OnEnable()
    {
        inGameEvent.AddListener<TaskCompleteEvent>(OnTaskCompleteReceived);
    }

    private void OnDisable()
    {
        inGameEvent.RemoveListener<TaskCompleteEvent>(OnTaskCompleteReceived);
    }

    private void InitializeActors()
    {
        actorLookup = new Dictionary<ESequenceCharacter, SequenceActorBase>();
        foreach (var actor in actors)
        {
            if (actor == null) continue;
            if (actorLookup.ContainsKey(actor.characterId)) continue;
            actorLookup[actor.characterId] = actor;
        }
    }

    // ========== 버튼 클릭 및 UI 제어 ==========

    public void OnButtonClicked(CharacterBtn btn)
    {
        bool isSelected = selectedButtons.Contains(btn);

        if (selectedButtons.Count >= maxSelectCount && !isSelected)
        {
            Debug.LogWarning("최대 선택 수 초과");
            return;
        }

        btn.Flip(isSelected);

        if (isSelected)
        {
            selectedButtons.Remove(btn);
            selectedOrder.Remove(btn.characterId);
            btn.OnDeselected();
        }
        else
        {
            selectedButtons.Add(btn);
            selectedOrder.Add(btn.characterId);
            btn.OnSelected();

            // 튜토리얼 연동
            if (selectedButtons.Count == maxSelectCount && IsTutorial && TutorialBlock.Instance != null)
            {
                TutorialBlock.Instance.StartSecond();
            }
        }

        for (int i = 0; i < selectedButtons.Count; i++)
            selectedButtons[i].SetOrder(i + 1);
    }

    public void Confirm()
    {
        if (selectedOrder.Count != maxSelectCount)
        {
            Debug.LogWarning("선택 개수가 부족함");
            return;
        }

        BroAudio.Play(click);
        ShowDefaultBalloons(false);


        var orderCopy = new List<ESequenceCharacter>(selectedOrder);
        StartCoroutine(RunSequenceRoutine(orderCopy));
    }

    public void ResetSelection()
    {
        BroAudio.Play(click);

        foreach (CharacterBtn btn in selectedButtons)
        {
            if (btn != null)
            {
                btn.outlinable.enabled = false;
                btn.Flip(true);
            }
        }
        ResetSelectionUI();
    }

    private void ResetSelectionUI()
    {
        ResetTaskUI();
        foreach (var btn in selectedButtons)
        {
            if (btn != null) btn.OnDeselected();
        }
        selectedButtons.Clear();
        selectedOrder.Clear();
    }

    private void OnTaskCompleteReceived(TaskCompleteEvent evt)
    {
        foreach (TaskText tt in taskTexts)
        {
            if (tt.task == evt.TaskType) tt.text.color = evt.TargetColor;
        }
    }

    private void ResetTaskUI()
    {
        if (taskTexts == null) return;
        foreach (TaskText tt in taskTexts) tt.text.color = Color.white;
    }

    private void ShowDefaultBalloons(bool show)
    {
        if (defaultBalloon == null) return;
        foreach (var balloon in defaultBalloon)
        {
            if (balloon != null) balloon.gameObject.SetActive(show);
        }
    }

    private IEnumerator RunSequenceRoutine(List<ESequenceCharacter> order)
    {
        SequenceState currentState = new SequenceState(order, HandleClear);
        currentState.RegisterActors(actorLookup);

        var btnSnapshot = new List<CharacterBtn>(this.selectedButtons);

        executedActors.Clear();
        isClear = false;

        SequencePhaseEvent phaseEvent = InGameEvent.SequencePhaseEvent;
        phaseEvent.Initialize(Phase.Play);
        inGameEvent?.RaiseEvent(phaseEvent);

        CinemaUIEvent cinemaUI = InGameEvent.CinemaUIEvent;
        cinemaUI.Initialize(true);

        for (int i = 0; i < order.Count; i++)
        {
            currentState.IsTropical = (i == 2 || i == 3);
            foreach (var trofical in troficals) { trofical.ChangeLight(currentState.IsTropical); }

            var who = order[i];
            if (!actorLookup.TryGetValue(who, out var actor)) continue;

            executedActors.Add(actor);
            inGameEvent?.RaiseEvent(new ActorExecutingEvent { ActorIndex = i });

            for (int j = 0; j < btnSnapshot.Count; j++)
            {
                btnSnapshot[j].outlinable.enabled = (j == i);
                if (j == i) btnSnapshot[i].Flip(true);
            }

            yield return StartCoroutine(actor.Execute(currentState));
            yield return new WaitForSeconds(1.5f);
        }

        // 클리어 판정 시
        if (isClear)
        {
            inGameEvent?.RaiseEvent(new SequencePhaseEvent { CurrentPhase = Phase.Clear });
            yield break;
        }

        Debug.Log("정방향 시퀀스 끝! (실패)");

        if (!IsTutorial && TutorialBlock.Instance != null)
        {
            TutorialBlock.Instance.StartThird();
        }

        yield return new WaitForSeconds(2f);

        ComeDownCardUIEvent comeDownCardUI = InGameEvent.GameEndEvent;
        inGameEvent?.RaiseEvent(comeDownCardUI);

        yield return StartCoroutine(RewindAllRoutine(currentState));

        ComeUpCardUIEvent comeUpCardUI = InGameEvent.GameStartEvent;
        inGameEvent?.RaiseEvent(comeUpCardUI);

        ShowDefaultBalloons(true);
        ResetSelectionUI();

        SequencePhaseEvent sequencePhase = InGameEvent.SequencePhaseEvent;
        sequencePhase.Initialize(Phase.End);
        inGameEvent?.RaiseEvent(sequencePhase);

        CinemaUIEvent cinemaUIEvent = InGameEvent.CinemaUIEvent;
        cinemaUIEvent.Initialize(false);
        inGameEvent?.RaiseEvent(cinemaUIEvent);
    }

    private IEnumerator RewindAllRoutine(SequenceState ctx)
    {
        inGameEvent?.RaiseEvent(new SequencePhaseEvent { CurrentPhase = Phase.Rewind });

        for (int i = executedActors.Count - 1; i >= 0; i--)
        {
            var actor = executedActors[i];
            if (actor == null) continue;

            yield return StartCoroutine(actor.Rewind(ctx));
            yield return new WaitForSeconds(1f);
        }

        executedActors.Clear();
    }

    private void HandleClear()
    {
        Debug.Log("SequenceCore: 클리어 조건 달성!");
        ComeDownCardUIEvent comeDownCardUI = InGameEvent.GameEndEvent;
        inGameEvent?.RaiseEvent(comeDownCardUI);
        inGameEvent?.RaiseEvent(new CinemaUIEvent().Initialize(false));
        isClear = true;
    }
}