using Ami.BroAudio;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class PigeonActor : SequenceActorBase
{
    [SerializeField] private GameObject poopDialogue;
    [SerializeField] private GameObject poopEffect;
    [SerializeField] private Transform eatPos;
    [SerializeField] private Transform flyCenter;
    [SerializeField] private float flyRadius = 1.5f;
    [SerializeField] private float flyLoopDuration = 2f;

    [SerializeField] private SoundID poopSound;
    [SerializeField] private SoundID flySound;

    private Vector3 originPos;
    private Tween flyTween;
    private const string FLAG_GONE = "RiderGone";

    private readonly int hashPoop = Animator.StringToHash("Poop");
    private readonly int hashIdle = Animator.StringToHash("Idle");
    private readonly int hashRun = Animator.StringToHash("Run");
    private readonly int hashEat = Animator.StringToHash("Eat");
    private readonly int hashHit = Animator.StringToHash("Hit");

    protected override void Awake()
    {
        originPos = transform.position;
    }

    public override IEnumerator Execute(SequenceState ctx)
    {
        poopDialogue.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        poopEffect.SetActive(true);
        poopDialogue.SetActive(false);
        PlayAnim(hashPoop);
        BroAudio.Play(poopSound);

        int idxPigeon = ctx.GetIndex(ESequenceCharacter.Pigeon);
        int idxCaretaker = ctx.GetIndex(ESequenceCharacter.Caretaker);

        if (idxCaretaker != -1 && idxCaretaker > idxPigeon)
        {
            if (ctx.TryGetActor(ESequenceCharacter.Caretaker, out CaretakerActor caretaker))
            {
                caretaker.GetHitByPoop();
            }
            yield return new WaitForSeconds(2f);
            poopEffect.SetActive(false);
        }

        if (ctx.HasFlag(FLAG_GONE))
        {
            if (ctx.TryGetActor(ESequenceCharacter.Bicycle, out BicycleActor bicycle))
            {
                bicycle.RequestComeback();
            }
        }
    }

    public void GetHitByWater(SequenceState ctx)
    {
        PlayAnim(hashHit);
        StartCircleFly();
    }

    public void DescendToEat(SequenceState ctx)
    {
        StopCircleFly();
        PlayAnim(hashRun);
        transform.DOMove(eatPos.position, 2f).OnComplete(() =>
        {
            PlayAnim(hashEat);
        });
    }

    private void StartCircleFly()
    {
        if (flyTween != null) return;
        BroAudio.Play(flySound);

        float angle = 0;
        flyTween = DOTween.To(() => angle, x => {
            angle = x;
            var offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * flyRadius;
            transform.position = flyCenter.position + offset;
        }, Mathf.PI * 2f, flyLoopDuration).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
    }

    private void StopCircleFly()
    {
        if (flyTween != null)
        {
            flyTween.Kill();
            flyTween = null;
        }
    }

    public override IEnumerator Rewind(SequenceState ctx)
    {
        StopCircleFly();
        poopEffect.SetActive(false);
        poopDialogue.SetActive(false);
        transform.DOKill();
        PlayAnim(hashIdle);
        yield return transform.DOMove(originPos, 1f).WaitForCompletion();
    }
}