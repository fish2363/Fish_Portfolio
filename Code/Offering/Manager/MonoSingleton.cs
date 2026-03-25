using UnityEngine;

/*
수정 사항
- lock 제거: Unity MonoBehaviour는 메인 스레드 전용이므로 불필요한 동기화 제거
- Instance 자동 생성 제거: 초기화 순서를 호출 타이밍에 의존하지 않도록 구조 변경
- FindObject 계열 최신 API 사용: FindAnyObjectByType로 비용 최소화
- GameManager 자식 설정 제거: 싱글톤의 생명주기/계층 의존성 제거
- _prefix 제거: 변수명 컨벤션 통일
- shutdown 처리 단순화: OnApplicationQuit 중심으로 종료 상태 관리

이유
- 싱글톤의 책임을 "생성"이 아닌 "등록/조회"로 제한
- 초기화 순서 버그 및 숨겨진 의존성 제거
- Unity 메인 스레드 모델과 일관된 구조 유지.
*/

[DisallowMultipleComponent]
public abstract class MonoSingleton<T> : ExtendedMono where T : MonoSingleton<T>
{
    private static bool isShutDown;
    private static T instance;

    
    public static T Instance
    {
        get
        {
            if (isShutDown)
            {
                Debug.LogWarning($"{typeof(T).Name} is shutting down.");
                return null;
            }

            if (instance == null)
            {
                instance = FindAnyObjectByType<T>();

                if (instance == null)
                {
                    Debug.LogError($"{typeof(T).Name} instance is not initialized. " +
                                   $"Make sure it exists in the scene or is created during bootstrap.");
                }
            }

            return instance;
        }
    }

    #region Unity Methods
    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"Duplicate singleton detected: {typeof(T).Name}");
            Destroy(gameObject);
            return;
        }

        instance = (T)this;
    }

    protected virtual void OnApplicationQuit()
    {
        isShutDown = true;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    #endregion
}