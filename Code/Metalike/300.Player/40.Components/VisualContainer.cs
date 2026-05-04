using GondrLib.Dependencies;
using GondrLib.ObjectPool.RunTime;
using System;
using UnityEngine;

public class VisualContainer : MonoBehaviour, ICharacterChangeReceiver, IEntityComponent
{
    [HideInInspector][Inject] public PoolManagerMono poolManager;
    public PlayerVisual CurrentVisual { get; private set; }

    public event Action<PlayerVisual> OnVisualChanged;
    public MeshContainer CurrentMesh { get; private set; }

    private Entity owner;

    public void Initialize(Entity _entity)
    {
        owner = _entity;

        if (CurrentVisual != null)
            OnVisualChanged?.Invoke(CurrentVisual);
    }
    
    public void OnCharacterChanged(CharacterData info)
    {
        if (CurrentVisual != null) poolManager.Push(CurrentVisual);

        CurrentVisual = poolManager.Pop<PlayerVisual>(info.visual);
        CurrentVisual.transform.SetParent(transform);
        CurrentVisual.InitalSetting();
        CurrentMesh = CurrentVisual.CurrentMesh;

        OnVisualChanged?.Invoke(CurrentVisual);
    }
}
