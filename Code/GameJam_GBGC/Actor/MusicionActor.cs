using Ami.BroAudio;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class MusicionActor : SequenceActorBase
{
    [Header("Settings & UI")]
    [SerializeField] private SoundID meow;
    [SerializeField] private GameObject dialogue;

    [Header("Events")]
    [SerializeField] private GameEventChannelSO inGameEvent;

    private const string FLAG_Can_CatSeeMusicion = "Can_CatSeeMusicion";
    private const string FLAG_GoingRoad = "GoingRoad";

    private readonly int hashSong = Animator.StringToHash("Song");
    private readonly int hashIdle = Animator.StringToHash("Idle");

    public override IEnumerator Execute(SequenceState ctx)
    {
        Debug.Log("음악가 : 노래 부르기");
        BroAudio.Play(meow);
        PlayAnim(hashSong);

        bool canCatSee = ctx.HasFlag(FLAG_Can_CatSeeMusicion);

        if (canCatSee)
        {
            if (ctx.TryGetActor(ESequenceCharacter.Cat, out CatActor cat))
            {
                cat.ReactToMusicion();
                Debug.Log("고양이 : 음악가 발견");
            }

            bool isAjucyGone = ctx.HasFlag(FLAG_GoingRoad);
            if (isAjucyGone)
            {
                if (ctx.TryGetActor(ESequenceCharacter.AJucy, out AjucyActor ajucy))
                {
                    if (ctx.IsIceCreamHeld)
                    {
                        TaskCompleteEvent taskComplete = InGameEvent.TaskCompleteEvent;
                        taskComplete.Initialize(Task.ILoveSong, Color.green);
                        inGameEvent.RaiseEvent(taskComplete);

                        ajucy.WalkWithIceCream();
                    }
                    else
                    {
                        ajucy.StopAndLookBack();
                    }
                }
            }
            else
            {
                yield return StartCoroutine(ShowDialogueRoutine());
            }
        }
        else
        {
            yield return StartCoroutine(ShowDialogueRoutine());
        }
    }

    private IEnumerator ShowDialogueRoutine()
    {
        dialogue.SetActive(true);
        yield return new WaitForSeconds(2f);
        dialogue.SetActive(false);
    }

    public override IEnumerator Rewind(SequenceState ctx)
    {
        dialogue.SetActive(false);
        PlayAnim(hashIdle);
        yield return null;
    }
}