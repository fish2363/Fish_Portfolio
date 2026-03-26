using Ami.BroAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    #region Init
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
    #endregion
    #region Sequence (핵심)
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

        if (isClear)
        {
            inGameEvent?.RaiseEvent(new SequencePhaseEvent { CurrentPhase = Phase.Clear });
            yield break;
        }

        Debug.Log("정방향 끝");

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
    #endregion

    #region Claer
    private void HandleClear()
    {
        Debug.Log("SequenceCore: 클리어 조건 달성!");
        ComeDownCardUIEvent comeDownCardUI = InGameEvent.GameEndEvent;
        inGameEvent?.RaiseEvent(comeDownCardUI);
        inGameEvent?.RaiseEvent(new CinemaUIEvent().Initialize(false));
        isClear = true;
    }
    #endregion
}