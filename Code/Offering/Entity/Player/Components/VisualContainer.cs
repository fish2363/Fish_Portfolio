using UnityEngine;

public class VisualContainer : MonoBehaviour,IEntityComponent
{
    [SerializeField] private GameObject _attachedBodyModel; // 머리가 붙어있는 온전한 모델
    [SerializeField] private GameObject _detachedHeadModel; // 굴러다닐 머리 모델
    public Transform _headPoint; // 굴러다닐 머리 모델
    public GameObject AttachedBodyModel => _attachedBodyModel;
    public GameObject DetachedHeadModel => _detachedHeadModel;
    private Player _player;

    public void Initialize(Entity entity)
    {
        _player = entity as Player;
        _player.OnHeadDropEvent += HandleDropHead;
    }

    private void HandleDropHead()
    {
        AttachedBodyModel.SetActive(false);
        DetachedHeadModel.SetActive(true);
    }

    private void OnDestroy()
    {
        _player.OnHeadDropEvent -= HandleDropHead;
    }
}
