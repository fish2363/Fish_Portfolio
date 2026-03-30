using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JuicePlate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI juiceCountText;
    [SerializeField] private GameObject juicePrefab;
    [SerializeField] private Transform spawnParent;

    [Header("랜덤 오프셋 범위 (x,z ±값)")]
    [SerializeField] private Vector2 offsetRange = new Vector2(0.2f, 0.2f);

    private void Start()
    {
        JuiceManager.Instance.JuiceCount.OnValueChanged += Refresh;
    }

    private void OnDisable()
    {
        JuiceManager.Instance.JuiceCount.OnValueChanged -= Refresh;
    }

    private void Refresh(int count)
    {
        // 텍스트 업데이트
        if (juiceCountText != null)
            juiceCountText.text = $"Juice : {count}";

        // 기존 오브젝트 제거
        for (int i = spawnParent.childCount - 1; i >= 0; i--)
            Destroy(spawnParent.GetChild(i).gameObject);

        // count 만큼 랜덤 오프셋을 주어 생성
        for (int i = 0; i < count; i++)
        {
            Vector3 basePos = spawnParent.position;

            // x,z 축을 기준으로 offsetRange 범위 내에서 랜덤 오프셋 생성
            Vector3 randomOffset = new Vector3(
                Random.Range(-offsetRange.x, offsetRange.x),
                0f,
                Random.Range(-offsetRange.y, offsetRange.y)
        );

            // Instantiate 시 위치에 오프셋 적용
            GameObject juice = Instantiate(
                juicePrefab,
                basePos + randomOffset,
                spawnParent.rotation,
                spawnParent
            );
            juice.transform.SetParent(spawnParent.parent);

            // 필요하다면 UI 드래그 문제 방지를 위해 부모를 parent.parent로 옮기기
            // juice.transform.SetParent(spawnParent.parent, true);
        }
    }
}
