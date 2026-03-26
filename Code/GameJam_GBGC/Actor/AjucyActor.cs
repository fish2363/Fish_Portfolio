using Ami.BroAudio;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class AjucyActor : SequenceActorBase
{
    [SerializeField] private GameEventChannelSO inGameEvent;

    [Header("Movement & Positioning")]
    [SerializeField] private Transform movePoint;
    [SerializeField] private Transform givePos;
    [SerializeField] private Transform movePoint_CatAttack;
    private Vector3 origin;

    [Header("UI & Effect")]
    [SerializeField] private GameObject iceCreamDialogue;
    [SerializeField] private GameObject songDialogue;
    [SerializeField] private SoundID eatSound;
    [SerializeField] private float eatTime = 1f;

    private const string FLAG_GoingRoad = "GoingRoad";

    private readonly int hashFrontWalk = Animator.StringToHash("FrontWalk");
    private readonly int hashIcyFrontWalk = Animator.StringToHash("Icy_FrontWalk");
    private readonly int hashIcyEat = Animator.StringToHash("Icy_Eat");
    private readonly int hashIdle = Animator.StringToHash("Idle");
    private readonly int hashIcyIdle = Animator.StringToHash("Icy_Idle");
    private readonly int hashStreching = Animator.StringToHash("Streching");
    private readonly int hashIcyCat = Animator.StringToHash("Icy_Cat");
    private readonly int hashBackIdle = Animator.StringToHash("BackIdle");

    protected override void Awake()
    {
        origin = transform.position;
    }

    public override IEnumerator Execute(SequenceState ctx)
    {
        if (ctx.IsTropical)
        {
            int walkAnim = ctx.IsIceCreamHeld ? hashIcyFrontWalk : hashFrontWalk;
            PlayAnim(walkAnim);

            yield return transform.DOMove(movePoint.position, 4f).SetEase(Ease.InSine).WaitForCompletion();

            TaskCompleteEvent taskComplete = InGameEvent.TaskCompleteEvent;
            taskComplete.Initialize(Task.GreenLight, Color.green);
            inGameEvent.RaiseEvent(taskComplete);

            SetSortingLayer("Front", 100);
            ctx.SetFlag(FLAG_GoingRoad);

            if (!ctx.HasFlag("Can_CatSeeMusicion") && ctx.IsIceCreamHeld)
            {
                PlayAnim(hashIcyEat);
                BroAudio.Play(eatSound);
                yield return new WaitForSeconds(eatTime);

                ctx.IsIceCreamHeld = false;
            }
            PlayAnim(hashIdle);
        }
        else
        {
            int failAnim = ctx.IsIceCreamHeld ? hashIcyEat : hashStreching;
            PlayAnim(failAnim);
            yield return new WaitForSeconds(eatTime);

            ctx.IsIceCreamHeld = false;
            PlayAnim(hashIdle);
        }
    }

    public IEnumerator ApproachToIceCream(SequenceState ctx)
    {
        iceCreamDialogue.SetActive(true);
        TaskCompleteEvent taskComplete = InGameEvent.TaskCompleteEvent;
        taskComplete.Initialize(Task.SellingIceCream, Color.green);
        inGameEvent.RaiseEvent(taskComplete);
        yield return transform.DOMove(givePos.position, 0.7f).WaitForCompletion();

        PlayAnim(hashIcyIdle);
        ctx.IsIceCreamHeld = true;
    }

    public void WalkWithIceCream()
    {
        PlayAnim(hashIcyFrontWalk);
    }

    public void StopAndLookBack()
    {
        PlayAnim(hashBackIdle);
    }

    public void AttackedByCat()
    {
        songDialogue.SetActive(true);
        iceCreamDialogue.SetActive(false);
        transform.DOMove(movePoint_CatAttack.position, 1f).OnComplete(() =>
        {
            PlayAnim(hashIcyCat);
        });
    }

    public override IEnumerator Rewind(SequenceState ctx)
    {
        songDialogue.SetActive(false);
        iceCreamDialogue.SetActive(false);
        PlayAnim(hashIdle);
        yield return transform.DOMove(origin, 1f).WaitForCompletion();
    }

    private void SetSortingLayer(string layerName, int order)
    {
        if (TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.sortingLayerName = layerName;
            sr.sortingOrder = order;
        }
    }
}