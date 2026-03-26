using System.Collections;
using UnityEngine;

public abstract class SequenceActorBase : MonoBehaviour
{
    [field: SerializeField] public Animator Animator { get; private set; }
    public ESequenceCharacter characterId;

    protected virtual void Awake()
    {

    }

    protected void PlayAnim(int animHash)
    {
        Animator.Play(animHash);
    }

    public abstract IEnumerator Execute(SequenceState ctx);

    // 역재생 루틴
    public virtual IEnumerator Rewind(SequenceState ctx)
    {
        yield break;
    }

    public virtual void OnInteract(string interactionType)
    {
    }
}