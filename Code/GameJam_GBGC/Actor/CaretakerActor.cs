using Ami.BroAudio;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class CaretakerActor : SequenceActorBase
{
    [SerializeField] private GameEventChannelSO inGameEvent;
    [Header("Durations")]
    [SerializeField] private float hitDuration = 0.6f;
    [SerializeField] private float waterDuration = 1f;
    [SerializeField] private float angryDuration = 0.5f;
    [SerializeField] private float moveDuration = 1f;

    [Header("Positions")]
    [SerializeField] private Transform goPos;
    [SerializeField] private Transform attackPos;

    [Header("UI & Audio")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private SoundID angry;
    [SerializeField] private SoundID watering;
    [SerializeField] private SoundID waterAttack;

    private Vector3 originPos;
    private const string FLAG_CLEAR_ENABLED = "ClearEnabledByPigeon";
    private const string FLAG_GONE = "RiderGone";

    // 애니메이션 해시
    private readonly int hashIdle = Animator.StringToHash("Gardner-Idle");
    private readonly int hashAngry = Animator.StringToHash("Gardner-Angry");
    private readonly int hashMove = Animator.StringToHash("Gardner-Move");
    private readonly int hashHit = Animator.StringToHash("Gardner-Hit");

    protected override void Awake()
    {
        originPos = transform.position;
    }

    public override IEnumerator Execute(SequenceState ctx)
    {
        int idxPigeon = ctx.GetIndex(ESequenceCharacter.Pigeon);
        int idxCaretaker = ctx.GetIndex(ESequenceCharacter.Caretaker);

        if (idxPigeon == -1)
        {
            BroAudio.Play(watering);
            PlayAnim(hashIdle);
            yield return new WaitForSeconds(waterDuration);
            yield break;
        }

        if (idxCaretaker > idxPigeon)
        {
            PlayAnim(hashAngry);
            yield return new WaitForSeconds(hitDuration);

            transform.DOMove(attackPos.position, 0.2f);
            yield return new WaitForSeconds(angryDuration / 2);

            BroAudio.Play(waterAttack);

            // 비둘기를 찾아 물을 뿌렸음을 알림
            if (ctx.TryGetActor(ESequenceCharacter.Pigeon, out PigeonActor pigeon))
            {
                pigeon.GetHitByWater(ctx);
            }

            dialogueBox.SetActive(false);
            yield return new WaitForSeconds(angryDuration / 2);
            PlayAnim(hashIdle);

            ctx.SetFlag(FLAG_CLEAR_ENABLED);
        }
        else
        {
            PlayAnim(hashMove);
            yield return transform.DOMove(goPos.position, moveDuration / 2).SetEase(Ease.InQuad).WaitForCompletion();
            yield return new WaitForSeconds(moveDuration / 2);
            BroAudio.Play(watering);
            PlayAnim(hashIdle);
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

    // 비둘기가 호출하는 메서드
    public void GetHitByPoop()
    {
        PlayAnim(hashHit);
        BroAudio.Play(angry);
        dialogueBox.SetActive(true);

        TaskCompleteEvent taskComplete = InGameEvent.TaskCompleteEvent;
        taskComplete.Initialize(Task.AngryMan, Color.green);
        inGameEvent.RaiseEvent(taskComplete);
    }

    public override IEnumerator Rewind(SequenceState ctx)
    {
        dialogueBox.SetActive(false);
        transform.DOKill();
        PlayAnim(hashIdle);
        yield return transform.DOMove(originPos, 1f).WaitForCompletion();
    }
}