using UnityEngine;

public class JJangDDungPlate : MonoBehaviour
{
    [SerializeField] GameObject stewObj;
    [SerializeField] Transform stewPos;
    [SerializeField] Transform stewParent;
    [SerializeField] Vector2 offsetRange = new Vector2(150f, 150f); // x, y 최대 오프셋

    public void CreateStew()
    {
        Debug.Log("됨");
        // 프리펩 생성
        var newStew = Instantiate(stewObj, stewPos.position, Quaternion.identity, stewParent);

        // UI 좌표계니까 localPosition을 기준으로 랜덤 오프셋 적용
        var rect = newStew.GetComponent<RectTransform>();
        if (rect != null)
        {
            Vector2 randOffset = new Vector2(
                Random.Range(-offsetRange.x, offsetRange.x),
                Random.Range(-offsetRange.y, offsetRange.y)
            );
            rect.localPosition += (Vector3)randOffset;
        }
    }
}
