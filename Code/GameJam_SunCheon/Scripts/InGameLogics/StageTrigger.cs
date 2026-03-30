using System.Collections.Generic;
using UnityEngine;

public class StageTrigger : MonoBehaviour
{
    [SerializeField] InGameManager InGameManager;
    public static StageTrigger Instance;
    [SerializeField] GameObject _cover;
    [SerializeField] Transform[] spawnPos;
    [SerializeField] private MiniBread miniBread;
    [SerializeField] private MiniCup miniCup;
    [SerializeField] private Transform cupSpawnPos;
    [SerializeField] private StewManager stewManager;
    private int idx;

    private MiniCup mini;

    // ▶ 슬롯별로 1:1 레퍼런스 관리 (spawnPos 길이와 동일)
    private List<GameObject> _slots = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InGameManager.StartGame();

        // 슬롯 리스트 초기화 (null 패딩)
        _slots = new List<GameObject>(spawnPos.Length);
        for (int i = 0; i < spawnPos.Length; i++) _slots.Add(null);
    }

    // putPos: 1-based 슬롯 번호
    public void BreadPut(int putPos, MiniBreadState state)
    {
        int idx = Mathf.Clamp(putPos - 1, 0, spawnPos.Length - 1);

        // 1) 기존 오브젝트 있으면 파괴 후 슬롯 비우기
        if (_slots[idx] != null)
        {
            Destroy(_slots[idx]);
            _slots[idx] = null;
        }

        // 2) None이면 비우기만 하고 종료 (새로 만들지 않음)
        if (state == MiniBreadState.None) return;

        // 3) 새로 생성해서 슬롯에 배정
        MiniBread miniB = Instantiate(miniBread, spawnPos[idx]);
        miniB.SetVisual(state);

        var meta = miniB.GetComponent<BreadMeta>();
        if (meta != null) meta.kitchenIndex = idx;

        _slots[idx] = miniB.gameObject;
    }

    public void CupPut(JuiceState state)
    {
        if (mini == null)
            mini = Instantiate(miniCup, cupSpawnPos);
        mini.SetVisual(state);
    }

    public void Burn()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] == null) continue;
            _slots[i].GetComponent<MiniBread>()?.SetVisual(MiniBreadState.Burn);
        }
    }
    public void OnRemoveStew()
    {
        if(stewManager.firstFireplace.CurState != StewState.None)
        {
            stewManager.firstFireplace.CurState = StewState.None;
            if (stewManager.CoolCoroutine != null)
            {
                StopCoroutine(stewManager.CoolCoroutine);
                stewManager.CoolCoroutine = null;
             }
            stewManager.firstFireplace.smokeObj.SetActive(false);
            return;
        }
        else if(stewManager.secondFireplace.CurState != StewState.None)
        {
            stewManager.secondFireplace.CurState = StewState.None;
            if (stewManager.CoolCoroutine != null)
            {
                StopCoroutine(stewManager.CoolCoroutine);
                stewManager.CoolCoroutine = null;
            }
            stewManager.secondFireplace.smokeObj.SetActive(false);
            return;
        }
    }
    public void CoverPut(bool active, bool burn = false)
    {
        if (burn)
            _cover.GetComponent<Animator>().SetBool("Burn", burn);
        _cover.SetActive(active);
    }

    public void Remove(int idx)
    {
        if (idx < 0 || idx >= _slots.Count) return;

        if (_slots[idx] != null)
        {
            Destroy(_slots[idx]);   // ← 실제 오브젝트 파괴
            _slots[idx] = null;     // ← 슬롯 비우기
        }
    }

    // (선택) 전부 비우고 싶을 때
    public void ClearAll()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] != null) Destroy(_slots[i]);
            _slots[i] = null;
        }
    }
}
