using UnityEngine;

public class CaptureManager : MonoBehaviour
{
    [Header("캡쳐 설정값")]
    private Door _door;
    [SerializeField]
    private Vector2 captureSize;
    [SerializeField] private GameObject _captureBoxImage;
    [SerializeField]
    private LayerMask whatIsCaptureObj;

    private Vector2 mousePos;
    private bool isNowCapture;

    public static int inventoryIdx;
    [SerializeField] private Transform _captureCollection;


    private void Awake()
    {
        isNowCapture = false;
    }

    private void Start()
    {
        inventoryIdx = -1;
    }
    private void Update()
    {
        Debug.Log(inventoryIdx);
        MouseFollow();
        Capture();
        _captureBoxImage.transform.position = transform.position;
        if (inventoryIdx >= 7)
        {
            inventoryIdx = -1;
        }
    }

    private void MouseFollow()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!isNowCapture)
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, mousePos, 10f * Time.deltaTime);
    }

    private void Capture()
    {
        Collider2D[] captureObject = Physics2D.OverlapBoxAll(gameObject.transform.position, captureSize, 0, whatIsCaptureObj);

        if (Input.GetMouseButtonDown(0) && !isNowCapture && transform.localPosition != _captureCollection.localPosition)
        {
            if (captureObject != null)
            {
                SaveInventory(captureObject);
                //StartCoroutine(WaitCaptureRoutine());
            }
            else
            {
                print("캡쳐할 물cprk djqttmq니다");
                isNowCapture = false;
            }
        }
    }

    private void SaveInventory(Collider2D[] captureObject)
    {
        for (int i = 0; i < captureObject.Length; i++)
        {
            if (inventoryIdx <= 6)
            {
                inventoryIdx += 1;
                captureObject[i].GetComponent<CaptureObject>().CaptureFinish(inventoryIdx);
            }
            //else
           // {
                //inventoryIdx = 0;
            //    captureObject[i].GetComponent<CaptureObject>().CaptureFinish(inventoryIdx);
           // }
        }
    }

    //private IEnumerator WaitCaptureRoutine()
    //{
    //  isNowCapture = true;
    //  yield return new WaitForSeconds(1f);
    //  isNowCapture = false;
    //  }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(gameObject.transform.position, captureSize);
    }
}
