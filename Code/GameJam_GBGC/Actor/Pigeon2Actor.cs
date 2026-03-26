using Ami.BroAudio;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Pigeon2Actor : SequenceActorBase
{
    [SerializeField] private Transform runPoint;
    [SerializeField] private SoundID gogo;

    public const string FLAG_FlyPigeon = "flyPigeon";
    private const string FLAG_CatSeePigeon = "CatSeePigeon";
    private Vector3 origin;

    private readonly int hashRunToCat = Animator.StringToHash("RunToCat");
    private readonly int hashIdle = Animator.StringToHash("Idle");

    protected override void Awake()
    {
        origin = transform.position;
    }

    public override IEnumerator Execute(SequenceState ctx)
    {
        ctx.SetFlag(FLAG_FlyPigeon);
        PlayAnim(hashRunToCat);
        Debug.Log("비둘기:날아오르기");

        BroAudio.Play(gogo);
        yield return transform.DOMove(runPoint.position, 3f).WaitForCompletion();

        bool ASeePigeon = ctx.HasFlag(FLAG_CatSeePigeon);
        if (ASeePigeon)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log("고양이:비둘기에게 막힘");

            if (ctx.TryGetActor(ESequenceCharacter.Cat, out CatActor cat))
            {
                cat.BlockedByPigeon(ctx);
            }
            yield return new WaitForSeconds(2.5f);
        }
    }

    public override IEnumerator Rewind(SequenceState ctx)
    {
        PlayAnim(hashIdle);
        yield return transform.DOMove(origin, 1f).WaitForCompletion();
    }
}