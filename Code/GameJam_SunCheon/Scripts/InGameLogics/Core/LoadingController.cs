// LoadingController.cs
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // TMP 쓰면 TMP_Text로 교체

public class LoadingController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Image loadingImage;     // 로딩 화면용 이미지
    [SerializeField] TextMeshProUGUI loadingText;       // TMP 사용 시 TMP_Text

    [Header("Config")]
    string mainSceneName = "MainScene";
    List<string> messages = new()
    {
        "주스 농도를 계산하는 중...",
        "최적의 당도를 찾는 중...",
        "컵을 소독하는 중...",
        "얼음을 채우는 중...",
        "비밀 레시피를 불러오는 중...",
        "칠게빵 굽는 중...",
        "매실청 담그는 중...",
        "짱뚱어 잡는 중...",
        "짱뚱어탕 끓이는 중...",
        "매실 수확 중"
    };

    void OnEnable() => StartCoroutine(RunFakeLoading());

    IEnumerator RunFakeLoading()
    {
        // 표시 스케줄: 1s, 1s, 0.5s
        float[] slots = { 2f, 2f, 1f };

        string last = null;
        for (int i = 0; i < slots.Length; i++)
        {
            string msg = PickRandomMessageAvoidRepeat(last);
            last = msg;
            if (loadingText) loadingText.text = msg;
            yield return new WaitForSeconds(slots[i]);
        }

        // 메인 씬으로 전환(Single: Start/Loading 정리)
        yield return SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);
        var loadingScene = gameObject.scene;
        yield return SceneManager.UnloadSceneAsync("StartScene");
        GameManager.Instance.StartGame();
        yield return SceneManager.UnloadSceneAsync(loadingScene);

    }

    string PickRandomMessageAvoidRepeat(string prev)
    {
        if (messages == null || messages.Count == 0) return "";
        if (messages.Count == 1) return messages[0];
        int idx;
        do { idx = Random.Range(0, messages.Count); }
        while (messages[idx] == prev);
        return messages[idx];
    }
}
