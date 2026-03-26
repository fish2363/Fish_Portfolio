using System.Collections;
using UnityEngine;

public class IceCreamActor : SequenceActorBase
{
    private float maketingDuration = 1f;
    private const string FLAG_GoingRoad = "GoingRoad";

    private readonly int hashMaketer = Animator.StringToHash("Maketer");
    private readonly int hashGive = Animator.StringToHash("Give");
    private readonly int hashIdle = Animator.StringToHash("Idle");

    public override IEnumerator Execute(SequenceState ctx)
    {
        Debug.Log("¾ÆÁÜ¸¶ : È«º¸");
        PlayAnim(hashMaketer);
        yield return new WaitForSeconds(maketingDuration);

        bool AjucyGone = ctx.HasFlag(FLAG_GoingRoad);
        if (!AjucyGone)
        {
            Debug.Log("¾ÆÀú¾¾ : ¾ÆÀÌ½ºÅ©¸² ¹ß°ß");

            if (ctx.TryGetActor(ESequenceCharacter.AJucy, out AjucyActor ajucy))
            {
                yield return StartCoroutine(ajucy.ApproachToIceCream(ctx));
            }

            PlayAnim(hashGive);
            yield return new WaitForSeconds(0.3f);
            PlayAnim(hashIdle);
        }
    }

    public override IEnumerator Rewind(SequenceState ctx)
    {
        PlayAnim(hashIdle);
        yield return null;
    }
}