using Ami.BroAudio;
using System.Collections;
using UnityEngine;

public class GrandpaActor : SequenceActorBase
{
    [SerializeField] private GameEventChannelSO inGameEvent;

    [SerializeField] private float snackDuration = 2f;
    [SerializeField] private float newspaperDuration = 1f;

    [SerializeField] private SoundID feed;
    [SerializeField] private SoundID paper;
    [SerializeField] private SoundID hmm;
    [SerializeField] private GameObject dialogue;

    private const string FLAG_GRANDPA_SNACK_DONE = "GrandpaSnackDone";
    private const string FLAG_CLEAR_ENABLED = "ClearEnabledByPigeon";
    private const string FLAG_GONE = "RiderGone";

    private readonly int hashFeed = Animator.StringToHash("GrandFather-Feed");
    private readonly int hashOpenPaper = Animator.StringToHash("GrandFather-OpenPaper");
    private readonly int hashIdle = Animator.StringToHash("GrandFather-Idle");

    public override IEnumerator Execute(SequenceState ctx)
    {
        bool clearEnabled = ctx.HasFlag(FLAG_CLEAR_ENABLED);

        if (clearEnabled)
        {
            Debug.Log("할아버지: 비둘기에게 과자 주기");
            TaskCompleteEvent taskComplete = InGameEvent.TaskCompleteEvent;
            taskComplete.Initialize(Task.FeedPigeon, Color.green);
            inGameEvent.RaiseEvent(taskComplete);
            PlayAnim(hashFeed);
            BroAudio.Play(feed);
            yield return new WaitForSeconds(snackDuration);

            // 비둘기에게 내려오라고 명령
            if (ctx.TryGetActor(ESequenceCharacter.Pigeon, out PigeonActor pigeon))
            {
                pigeon.DescendToEat(ctx);
            }

            ctx.SetFlag(FLAG_GRANDPA_SNACK_DONE);
        }
        else
        {
            dialogue.SetActive(true);
            PlayAnim(hashOpenPaper);
            BroAudio.Play(paper);
            BroAudio.Play(hmm);
            yield return new WaitForSeconds(newspaperDuration);
            dialogue.SetActive(false);
        }

        // 자전거 복귀 체크
        if (ctx.HasFlag(FLAG_GONE))
        {
            if (ctx.TryGetActor(ESequenceCharacter.Bicycle, out BicycleActor bicycle))
            {
                bicycle.RequestComeback();
            }
        }
    }

    public override IEnumerator Rewind(SequenceState ctx)
    {
        dialogue.SetActive(false);
        PlayAnim(hashIdle);
        yield return null;
    }
}