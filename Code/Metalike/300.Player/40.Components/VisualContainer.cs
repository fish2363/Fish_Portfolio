using System;
using UnityEngine;

public class VisualContainer : MonoBehaviour, ICharacterChangeReceiver, IEntityComponent
{
    public GameObject CurrentVisual { get; private set; }

    public event Action<GameObject> OnVisualChanged;
    public MeshContainer CurrentMesh { get; private set; }

    private Entity owner;

    public void Initialize(Entity _entity)
    {
        owner = _entity;

        if (CurrentVisual != null)
            OnVisualChanged?.Invoke(CurrentVisual);
    }
    public void OnVisualOutline(bool visible)
    {
        CurrentMesh.outlinable.enabled = visible;
    }
    public void OnCharacterChanged(CharacterData info)
    {
        if (CurrentVisual != null) Destroy(CurrentVisual);

        CurrentVisual = Instantiate(info.visual, transform);
        CurrentMesh = CurrentVisual.GetComponent<MeshContainer>();

        OnVisualChanged?.Invoke(CurrentVisual);
    }
}
