using TMPro;
using UnityEngine;


public interface IInteractable
{
    void Interact();
}


public class CheckInteraction : MonoBehaviour
{
    #region SerializeField
    [SerializeField] private float _checkRate = 0.05f;
    [SerializeField] private float _maxDistance = 3.0f;
    [SerializeField] private LayerMask _layerMask;
    #endregion

    #region private field
    private float _lastCheckTime;

    private GameObject _curGameobject;
    [SerializeField]
    private IInteractable _curInteractable;

    public GameObject OutLineObj = null;

    private Camera _camera;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;

        _layerMask = LayerMask.GetMask("Interactable");
    }

    public void InInteract(RaycastHit hit)
    {
        _curGameobject = hit.transform.gameObject;
        _curInteractable = hit.transform.GetComponent<IInteractable>();
    }
    public void OutInteract()
    {
        _curGameobject = null;
        _curInteractable = null;
    }

    public void OnInteraction()
    {
        if (_curInteractable != null)
        {
            print("Å¬¸¯ µé¾î¿È");
            _curInteractable.Interact();
            //_curGameobject = null;
            _curInteractable = null;
        }
    }

}

