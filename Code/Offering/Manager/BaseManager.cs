using Cysharp.Threading.Tasks;
using UnityEngine;

/*
수정 사항
- 불필요한 IManager 인터페이스 제거
- Event 기반 StartManager 방식 제거
- Init() / OnInit() 이중 구조로 공통 초기화 흐름 고정
- 중복 초기화 방지를 위한 IsInitialized 추가
- 필요 시 해제 가능한 Release() / OnRelease() 구조 추가

이유
- BaseManager만으로 공통 계약과 기본 구현을 모두 제공할 수 있음
- 초기화 순서를 GameManager 코드 작성 순서로 직접 관리함으로써
  이벤트 구독 순서에 의존하는 불명확한 실행 흐름을 제거하고
  매니저 공통 책임을 일관되게 관리하기 위함
*/

public abstract class BaseManager<T> : MonoSingleton<T> where T : BaseManager<T>
{
    public bool IsInitialized { get; private set; }

    #region Public Methods
    public async UniTask InitAsync()
    {
        if (IsInitialized) return;

        await OnInit();
        IsInitialized = true;
    }

    public void Release()
    {
        if (!IsInitialized) return;

        OnRelease();
        IsInitialized = false;
    }
    #endregion

    #region Protected Methods
    protected virtual UniTask OnInit()
    {
        return UniTask.CompletedTask;
    }

    protected virtual void OnRelease() { }
    #endregion
}