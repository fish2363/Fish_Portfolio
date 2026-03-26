using Ami.BroAudio;
using DG.Tweening;
using EPOOutline;
using System.Collections;
using UnityEngine;

public class BicycleActor : SequenceActorBase
{
    [SerializeField] private GameEventChannelSO inGameEvent;

    [SerializeField] private float rideOutDuration = 1f;
    [SerializeField] private float rideBackDuration = 1f;
    [SerializeField] private Transform goPos;
    [SerializeField] private Transform comePos;
    [SerializeField] private GameObject dialogue;
    [SerializeField] private Outlinable outlinable;
    [SerializeField] private SoundID bell;

    private const string FLAG_GONE = "RiderGone";
    private const string FLAG_GRANDPA_SNACK_DONE = "GrandpaSnackDone";

    private readonly int hashGo = Animator.StringToHash("Go");
    private readonly int hashIdle = Animator.StringToHash("Idle");
    private readonly int hashComeback = Animator.StringToHash("Comeback");

    public override IEnumerator Execute(SequenceState ctx)
    {
        Debug.Log("자전거: 타고 나가기");
        PlayAnim(hashGo);
        BroAudio.Play(bell);

        yield return transform.DOMove(goPos.position, rideOutDuration).OnComplete(() =>
        {
            TaskCompleteEvent taskComplete = InGameEvent.TaskCompleteEvent;
            taskComplete.Initialize(Task.GoodbyeBike, Color.green);
            inGameEvent.RaiseEvent(taskComplete);
            ctx.SetFlag(FLAG_GONE);
            dialogue.SetActive(true);
            outlinable.enabled = false;
            GetComponent<SpriteRenderer>().DOFade(0, 0.2f);
        }).WaitForCompletion();

        yield return new WaitForSeconds(1f);

        if (ctx.HasFlag(FLAG_GRANDPA_SNACK_DONE))
        {
            dialogue.SetActive(false);
            ctx.TriggerClear(); // 클리어!
            ESCManager.isClearFirstStage = true;
        }
    }

    public void RequestComeback()
    {
        StartCoroutine(ComebackRoutine());
    }

    private IEnumerator ComebackRoutine()
    {
        PlayAnim(hashComeback);
        BroAudio.Play(bell);
        GetComponent<SpriteRenderer>().DOFade(1, 0.2f);
        outlinable.enabled = true;
        yield return transform.DOMove(comePos.position, rideBackDuration).WaitForCompletion();
        PlayAnim(hashIdle);
    }

    public override IEnumerator Rewind(SequenceState ctx)
    {
        dialogue.SetActive(false);
        transform.DOKill();
        GetComponent<SpriteRenderer>().DOFade(1, 0f);
        outlinable.enabled = true;
        transform.position = comePos.position;
        PlayAnim(hashIdle);
        yield return null;
    }
}