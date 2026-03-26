using UnityEngine;

public interface IInteractable
{
    Transform TargetTransform { get; }
    void OnInteract(Entity interactor);
    void OnEnterInteractionRange();
    void OnExitInteractionRange();
}