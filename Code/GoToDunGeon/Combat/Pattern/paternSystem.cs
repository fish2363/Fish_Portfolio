using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static ScrollSO;

public class PatternSystem : MonoBehaviour
{
    [Header("Pattern Settings")]
    public Transform patternContainer;
    public GameObject[] patternPrefabs;

    [Header("테스트 설정")]
    public PatternDataSO testPattern;                  // 테스트용 패턴 데이터
    public PatternUIController patternUIController; // 패턴 UI 컨트롤러

    private ScrollSO currentScroll;
    private bool isPatternActive = false;
    private Coroutine patternCoroutine;

    void Update()
    {
        //// L키 테스트
        //if (Keyboard.current.lKey.wasPressedThisFrame && !isPatternActive)
        //{
        //    StartTestPattern();
        //}

        //if (Keyboard.current.lKey.wasPressedThisFrame && !isPatternActive)
        //{
        //    StartAttackPattern(Scroll_du); 
        //}

    }



    /// 테스트 패턴 완료 콜백
    private void OnTestPatternComplete(float accuracy)
    {
        isPatternActive = false;

        string result = accuracy >= currentScroll.accuracyThreshold ? "성공" : "실패";
        Debug.Log($"패턴 테스트 완료! 정확도: {accuracy:P1} ({result})");

        // 테스트 결과 UI 표시 (선택사항)
        if (accuracy >= currentScroll.accuracyThreshold)
        {
            Debug.Log("🎉 패턴 성공! 데미지가 100% 적용됩니다.");
        }
        else
        {
            Debug.Log("❌ 패턴 실패... 데미지가 50% 적용됩니다.");
        }
    }


    #region 공격 패턴 (수정됨)

    public void StartAttackPattern(ScrollSO scroll) // 공격 패턴 시작 (스크롤 기반)
    {
        if (isPatternActive) return;
        currentScroll = scroll;

        if (patternCoroutine != null)
            StopCoroutine(patternCoroutine);
        patternCoroutine = StartCoroutine(AttackPatternCoroutine(scroll));
    }

    private IEnumerator AttackPatternCoroutine(ScrollSO scroll) // 공격 패턴 전체 프로세스 관리
    {
        isPatternActive = true;

        // 패턴 UI 시작
        float accuracy = 0f;
        bool patternCompleted = false;

        if (patternUIController != null)
        {
            // 새로운 UI 시스템 사용
            patternUIController.StartPattern(currentScroll, (result) => {
                accuracy = result;
                patternCompleted = true;
            });

            // 패턴 완료까지 대기
            yield return new WaitUntil(() => patternCompleted);
        }
        else
        {
            // 기존 방식으로 폴백 (UI 컨트롤러가 없는 경우)
            yield return StartCoroutine(WaitForPlayerInputFallback(result => accuracy = result));
        }

        // 데미지 계산
        int damage = CalculateDamage(scroll.baseDamage, accuracy);

        // 패턴 정리
        ClearPattern();
        isPatternActive = false;

        // 결과 전달 (InGameManager가 있는 경우)
        if (InGameManager.Instance != null)
        {
            // Debug.Log("됏는데 ㅅㅂ?");
            InGameManager.Instance.OnAttackPatternComplete(scroll, accuracy, damage);
        }
        else
        {
            Debug.Log($"공격 패턴 완료! 정확도: {accuracy:P1}, 데미지: {damage}");
        }
    }

    #endregion

    #region 유틸리티 메서드

    /// 기본 데미지와 정확도로 최종 데미지 계산
    private int CalculateDamage(int baseDamage, float accuracy)
    {
        return Mathf.RoundToInt(baseDamage * accuracy);
    }

    /// 패턴 정리 (화면의 모든 패턴 오브젝트 제거)
    private void ClearPattern()
    {
        if (patternContainer != null)
        {
            // 패턴 컨테이너의 모든 자식 오브젝트 제거
            foreach (Transform child in patternContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// 임시 랜덤 패턴 데이터 생성 (테스트용)
    //private PatternData GenerateRandomPattern()
    //{
    //    PatternData pattern = ScriptableObject.CreateInstance<PatternData>();
    //    pattern.patternName = "임시 패턴";
    //    pattern.timeLimit = 5f;
    //    pattern.accuracyThreshold = 0.7f;

    //    // 간단한 4개 패턴 생성
    //    pattern.patternLength = 4;
    //    pattern.patternSequence = new PatternData.PatternElement[]
    //    {
    //        PatternData.PatternElement.Fire,
    //        PatternData.PatternElement.Water,
    //        PatternData.PatternElement.Fire,
    //        PatternData.PatternElement.Water
    //    };

    //    return pattern;
    //}

    /// UI 컨트롤러가 없을 때 사용하는 폴백 입력 처리
    private IEnumerator WaitForPlayerInputFallback(System.Action<float> onComplete)
    {
        float startTime = Time.time;
        float inputAccuracy = 0f;
        bool inputReceived = false;

        Debug.Log("UI 컨트롤러가 없어 폴백 모드로 실행됩니다. 아무 키나 누르세요.");

        // 간단한 입력 시뮬레이션
        while (!inputReceived && Time.time - startTime < currentScroll.timeLimit)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                inputAccuracy = Random.Range(0.5f, 1.0f); // 임시 정확도
                inputReceived = true;
                Debug.Log($"입력 감지! 임시 정확도: {inputAccuracy:P1}");
            }
            yield return null;
        }

        if (!inputReceived)
        {
            Debug.Log("시간 초과! 정확도 0%");
            inputAccuracy = 0f;
        }

        onComplete?.Invoke(inputAccuracy);
    }

    #endregion

    #region 기존 호환성 (사용하지 않음)

    // 기존 코드와의 호환성을 위해 남겨둠 (실제로는 사용하지 않음)
    private void DisplayPattern()
    {
        Debug.LogWarning("DisplayPattern()는 더 이상 사용되지 않습니다. PatternUIController를 사용하세요.");
    }

    private IEnumerator WaitForPlayerInput(System.Action<float> onComplete)
    {
        Debug.LogWarning("WaitForPlayerInput()는 더 이상 사용되지 않습니다. PatternUIController를 사용하세요.");
        yield return StartCoroutine(WaitForPlayerInputFallback(onComplete));
    }

    #endregion
}

//using System.Collections;
//using UnityEngine;

//public class PatternSystem : MonoBehaviour
//{
//    [Header("Pattern Settings")]
//    public Transform patternContainer;
//    public GameObject[] patternPrefabs;

//    private PatternData currentPattern;
//    private bool isPatternActive = false;
//    private Coroutine patternCoroutine;

//    public void StartAttackPattern(Scroll scroll) // 공격 패턴 시작 (스크롤 기반)
//    {
//        if (isPatternActive) return;

//        // 스크롤에서 패턴 데이터 가져오기 (임시로 랜덤 생성)
//        currentPattern = GenerateRandomPattern();

//        if (patternCoroutine != null)
//            StopCoroutine(patternCoroutine);
//        patternCoroutine = StartCoroutine(AttackPatternCoroutine(scroll));
//    }

//    public void StartDefensePattern(PatternData monsterPattern) // 방어 패턴 시작 (몬스터 패턴 기반)

//    {
//        if (isPatternActive) return;

//        currentPattern = monsterPattern;

//        if (patternCoroutine != null)
//            StopCoroutine(patternCoroutine);
//        patternCoroutine = StartCoroutine(DefensePatternCoroutine());
//    }

//    private IEnumerator AttackPatternCoroutine(Scroll scroll) // 공격 패턴 전체 프로세스 관리

//    {
//        isPatternActive = true;

//        // 패턴 생성 및 표시
//        DisplayPattern();

//        // 플레이어 입력 대기
//        float accuracy = 0f;
//        yield return StartCoroutine(WaitForPlayerInput(result => accuracy = result));

//        // 데미지 계산
//        int damage = CalculateDamage(scroll.baseDamage, accuracy);

//        // 패턴 정리
//        ClearPattern();
//        isPatternActive = false;

//        // 결과 전달
//        InGameManager.Instance.OnAttackPatternComplete(accuracy, damage);
//    }

//    private IEnumerator DefensePatternCoroutine() // 방어 패턴 전체 프로세스 관리
//    {
//        isPatternActive = true;

//        // 몬스터 패턴 생성 및 이동
//        yield return StartCoroutine(AnimateDefensePattern());

//        // 방어 결과 계산
//        bool success = CalculateDefenseSuccess();
//        int blockedDamage = success ? 50 : 0; // 임시값

//        // 패턴 정리
//        ClearPattern();
//        isPatternActive = false;

//        // 결과 전달
//        InGameManager.Instance.OnDefensePatternComplete(success, blockedDamage);
//    }

//    private void DisplayPattern() // 패턴 오브젝트를 화면에 배치
//    {
//        // 패턴 오브젝트들을 화면에 배치
//        for (int i = 0; i < currentPattern.positions.Length; i++)
//        {
//            if (i < patternPrefabs.Length)
//            {
//                GameObject pattern = Instantiate(patternPrefabs[i], patternContainer);
//                pattern.transform.localPosition = currentPattern.positions[i];
//            }
//        }
//    }

//    private IEnumerator WaitForPlayerInput(System.Action<float> onComplete) // 플레이어 입력 대기 및 정확도 계산
//    {
//        float startTime = Time.time;
//        float inputAccuracy = 0f;
//        bool inputReceived = false;

//        // 간단한 입력 시뮬레이션 (실제로는 터치/클릭 처리)
//        while (!inputReceived && Time.time - startTime < currentPattern.timeLimit)
//        {
//            if (Input.anyKeyDown) // 임시 입력 처리
//            {
//                inputAccuracy = Random.Range(0.5f, 1.0f); // 임시 정확도
//                inputReceived = true;
//            }
//            yield return null;
//        }

//        onComplete?.Invoke(inputAccuracy);
//    }

//    private IEnumerator AnimateDefensePattern() // 몬스터 패턴의 떨어지는 애니메이션
//    {
//        // 몬스터 패턴이 위에서 아래로 떨어지는 애니메이션
//        float duration = 3f;
//        float elapsed = 0f;

//        GameObject[] patterns = new GameObject[currentPattern.positions.Length];

//        // 패턴 생성
//        for (int i = 0; i < patterns.Length; i++)
//        {
//            if (i < patternPrefabs.Length)
//            {
//                patterns[i] = Instantiate(patternPrefabs[i], patternContainer);
//                Vector3 startPos = currentPattern.positions[i] + Vector2.up * 10f;
//                patterns[i].transform.localPosition = startPos;
//            }
//        }

//        // 애니메이션
//        while (elapsed < duration)
//        {
//            elapsed += Time.deltaTime;
//            float progress = elapsed / duration;

//            for (int i = 0; i < patterns.Length; i++)
//            {
//                if (patterns[i] != null)
//                {
//                    Vector3 startPos = currentPattern.positions[i] + Vector2.up * 10f;
//                    Vector3 endPos = currentPattern.positions[i];
//                    patterns[i].transform.localPosition = Vector3.Lerp(startPos, endPos, progress);
//                }
//            }

//            yield return null;
//        }
//    }

//    private int CalculateDamage(int baseDamage, float accuracy) // 기본 데미지와 정확도로 최종 데미지 계산
//    {
//        return Mathf.RoundToInt(baseDamage * accuracy);
//    }

//    private bool CalculateDefenseSuccess() // 방어 성공 여부 판정
//    {
//        // 임시로 랜덤하게 방어 성공 여부 결정
//        return Random.Range(0f, 1f) > 0.3f;
//    }

//    private void ClearPattern() // 화면의 모든 패턴 오브젝트 제거
//    {
//        // 패턴 컨테이너의 모든 자식 오브젝트 제거
//        foreach (Transform child in patternContainer)
//        {
//            Destroy(child.gameObject);
//        }
//    }

//    private PatternData GenerateRandomPattern() // 임시 랜덤 패턴 데이터 생성
//    {
//        // 임시로 랜덤 패턴 데이터 생성
//        PatternData pattern = ScriptableObject.CreateInstance<PatternData>();
//        pattern.timeLimit = 5f;
//        pattern.accuracyThreshold = 0.7f;
//        pattern.positions = new Vector2[]
//        {
//            Vector2.zero,
//            Vector2.right * 2f,
//            Vector2.up * 2f
//        };
//        return pattern;
//    }
//}