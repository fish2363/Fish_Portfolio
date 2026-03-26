using Ami.BroAudio;
using DG.Tweening;
using EPOOutline;
using System.Collections;
using UnityEngine;

public class CatActor : SequenceActorBase
{
    [Header("Points")]
    [SerializeField] private Transform chasePoint;
    [SerializeField] private Transform meetPigeonPoint;
    [SerializeField] private Transform exitPoint;

    [Header("UI & Effect")]
    [SerializeField] private GameObject dialogueAttack;
    [SerializeField] private GameObject dialogue;
    [SerializeField] private GameObject dialogueWow;
    [SerializeField] private Outlinable outline;

    [Header("Audio & Events")]
    [SerializeField] private SoundID meow;
    [SerializeField] private GameEventChannelSO inGameEvent; 

    private Vector3 origin;
    private const string FLAG_CatSeePigeon = "CatSeePigeon";
    public const string FLAG_FlyPigeon = "flyPigeon";

    private readonly int hashMoveToPigeon = Animator.StringToHash("MoveToPigeon");
    private readonly int hashSeePigeon = Animator.StringToHash("SeePigeon");
    private readonly int hashChasePigeon = Animator.StringToHash("ChasePigeon");
    private readonly int hashIdle = Animator.StringToHash("Idle");
    private readonly int hashGaJiMa = Animator.StringToHash("GaJiMa");
    private readonly int hashHitTale = Animator.StringToHash("HitTale");
    private readonly int hashSeeMusicion = Animator.StringToHash("SeeMusicion");

    protected void Awake()
    {
        origin = transform.position;
    }

    public override IEnumerator Execute(SequenceState ctx)
    {
        bool AflyPigeon = ctx.HasFlag(FLAG_FlyPigeon);
        if (!AflyPigeon)
        {
            Debug.Log("고양이 : 비둘기 보러가기");
            dialogueAttack.SetActive(true);
            PlayAnim(hashMoveToPigeon);

            yield return transform.DOMove(meetPigeonPoint.position, 1f).WaitForCompletion();

            ctx.SetFlag(FLAG_CatSeePigeon);
            BroAudio.Play(meow);
            PlayAnim(hashSeePigeon);
        }
        else
        {
            dialogue.SetActive(true);
            yield return new WaitForSeconds(2f);

            Debug.Log("고양이 실패:나가기");
            PlayAnim(hashChasePigeon);
            BroAudio.Play(meow);

            yield return transform.DOMove(exitPoint.position, 3.5f).OnComplete(() =>
            {
                dialogue.SetActive(false);
                if (outline != null) outline.enabled = false;
                GetComponent<SpriteRenderer>().DOFade(0, 0.2f);
            }).WaitForCompletion();
        }
    }
    public void StartChasePigeon()
    {
        dialogueAttack.SetActive(false);
        Debug.Log("고양이 : 비둘기 쫓아가기");


        TaskCompleteEvent taskComplete = InGameEvent.TaskCompleteEvent;
        taskComplete.Initialize(Task.CatPigeon, Color.green);
        inGameEvent.RaiseEvent(taskComplete);

        dialogue.SetActive(false);
        BroAudio.Play(meow);

        transform.DOMove(chasePoint.position, 3f).OnComplete(() =>
        {
            PlayAnim(hashSeePigeon);
        });
    }

    public void OnWowDialogueEnd(SequenceState ctx)
    {
        dialogueWow.SetActive(true);
        BroAudio.Play(meow);
        ctx.TriggerClear(); 
    }

    public void BlockedByPigeon(SequenceState ctx)
    {
        PlayAnim(hashGaJiMa);
        ctx.SetFlag("Can_CatSeeMusicion");
    }

    public void HitByAjucy()
    {
        PlayAnim(hashHitTale);
    }

    public void ReactToMusicion()
    {
        PlayAnim(hashSeeMusicion);
    }

    public override IEnumerator Rewind(SequenceState ctx)
    {
        dialogue.SetActive(false);
        dialogueAttack.SetActive(false);
        dialogueWow.SetActive(false);
        PlayAnim(hashIdle);

        GetComponent<SpriteRenderer>().DOFade(1, 0f);
        if (outline != null) outline.enabled = true;

        yield return transform.DOMove(origin, 1f).WaitForCompletion();
    }
}