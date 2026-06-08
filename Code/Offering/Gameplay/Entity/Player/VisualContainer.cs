using UnityEngine;

public class VisualContainer : MonoBehaviour,IEntityComponent
{
    [field: SerializeField] public GameObject AttachedBodyModel { get; private set; }
    [field: SerializeField] public GameObject DetachedBodyModel { get; private set; }
    [field: SerializeField] public GameObject DetachedHeadModel { get; private set; }
    [field: SerializeField] public Transform HeadPoint { get; private set; }
    
    private Player _player;

    private Quaternion _originalHeadLocalRotation;

    public void Initialize(Entity entity)
    {
        _player = entity as Player;
        _player.OnHeadDropEvent += HandleDropHead;

        if (DetachedHeadModel != null)
            _originalHeadLocalRotation = DetachedHeadModel.transform.localRotation;
    }
    public void RestoreAttachedVisual()
    {
        if (DetachedHeadModel != null && HeadPoint != null)
        {
            DetachedHeadModel.transform.SetParent(HeadPoint, false);
            DetachedHeadModel.transform.localPosition = Vector3.zero;
            DetachedHeadModel.transform.localRotation = _originalHeadLocalRotation;
            DetachedHeadModel.SetActive(false);
        }

        if (DetachedBodyModel != null)
            DetachedBodyModel.SetActive(false);

        if (AttachedBodyModel != null)
            AttachedBodyModel.SetActive(true);
    }
    private void HandleDropHead()
    {
        AttachedBodyModel.SetActive(false);
        DetachedHeadModel.SetActive(true);
        DetachedBodyModel.SetActive(true);
    }

    private void OnDestroy()
    {
        _player.OnHeadDropEvent -= HandleDropHead;
    }
}
