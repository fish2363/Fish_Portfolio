using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.InputSystem;

public enum StartTextType
{
    MYTURN = 0,
    ROUNDEND,
    CLEAR,
    ENEMYTURN
}

public class TurnStartText : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform fightImage;
    [SerializeField] private Image whiteBar;
    [SerializeField] private BattleIcons battleIcons;

    [Header("Animation Settings")]
    [SerializeField] private float scaleUpTime = 0.3f;
    [SerializeField] private float shakeTime = 0.4f;
    [SerializeField] private float whiteBarExpandTime = 0.4f;
    [SerializeField] private float disappearTime = 0.3f;

    private Sequence _animSequence;
    private Sequence _startTurnSequence;

    private Dictionary<StartTextType, Func<BattleIcon, Sequence>> _startTextDict;

    private void Awake()
    {
        _startTextDict = new Dictionary<StartTextType, Func<BattleIcon, Sequence>>
        {
            { StartTextType.CLEAR, CreateClearSequence },
            { StartTextType.ROUNDEND, CreateNextRoundSequence },
            { StartTextType.ENEMYTURN, CreateEnemyTurnSequence },
            { StartTextType.MYTURN, CreateMyTurnSequence }
        };
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Keyboard.current.aKey.wasPressedThisFrame)
            StartTurnUI(StartTextType.ENEMYTURN, 1).Forget();
        if (Keyboard.current.sKey.wasPressedThisFrame)
            StartTurnUI(StartTextType.MYTURN, 1).Forget();
    }
#endif

    /// <summary>
    /// 턴 시작 UI 애니메이션을 실행합니다.
    /// </summary>
    public async UniTask StartTurnUI(StartTextType textType, int currentTurn, CancellationToken ct = default)
    {
        if (ct == default)
            ct = this.GetCancellationTokenOnDestroy();

        KillActiveSequences();

        BattleIcon icon = battleIcons.GetIcons(textType);
        icon.gameObject.SetActive(true);

        if (textType != StartTextType.CLEAR)
            icon.turnText.text = $"제{currentTurn} 턴";

        try
        {
            await PlayCommonIntroSequence(ct);

            if (_startTextDict.TryGetValue(textType, out var sequenceFactory))
            {
                _startTurnSequence = sequenceFactory.Invoke(icon);

                if (_startTurnSequence != null)
                {
                    _startTurnSequence.SetLink(gameObject);
                    await _startTurnSequence.ToUniTask(cancellationToken: ct);
                }
            }
        }
        finally
        {
            if (icon != null)
                icon.gameObject.SetActive(false);
        }
    }

    private async UniTask PlayCommonIntroSequence(CancellationToken ct)
    {
        _animSequence = DOTween.Sequence()
            .Append(whiteBar.transform.DOScaleY(1, 0.2f).SetEase(Ease.OutBack))
            .Append(fightImage.DOScale(1f, 0.2f).SetEase(Ease.InQuart))
            .Append(fightImage.DOScale(1.2f, 0.5f).SetEase(Ease.Linear))
            .Append(fightImage.DOScale(1f, 0.2f).SetEase(Ease.OutBack))
            .SetLink(gameObject);

        await _animSequence.ToUniTask(cancellationToken: ct);
    }

    #region Sequence Factories

    private Sequence CreateClearSequence(BattleIcon icon)
    {
        return DOTween.Sequence()
            .Append(icon.parts[0].transform.DOLocalRotate(new Vector3(0f, 180f, -10f), 0.1f).SetEase(Ease.InQuart).SetLoops(6, LoopType.Yoyo))
            .Join(icon.parts[1].transform.DOLocalRotate(new Vector3(0f, 0f, -10f), 0.1f).SetEase(Ease.InQuart).SetLoops(6, LoopType.Yoyo))
            .Append(DOVirtual.DelayedCall(1f, () => whiteBar.transform.DOScaleY(0, 0.2f).SetEase(Ease.Linear)))
            .Append(icon.parts[0].transform.DOLocalRotate(new Vector3(0f, 180f, 0f), 0.1f).SetEase(Ease.InQuart))
            .Join(icon.parts[1].transform.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.1f).SetEase(Ease.InQuart))
            .Append(fightImage.DOScale(0f, 0.2f));
    }

    private Sequence CreateEnemyTurnSequence(BattleIcon icon)
    {
        return DOTween.Sequence()
            .Append(icon.parts[0].transform.DOScale(new Vector3(2.64f, 2.64f, 10f), 0.2f).SetEase(Ease.OutBack))
            .Append(icon.parts[0].transform.DOLocalRotate(new Vector3(0f, 0f, -720f), 0.2f).SetEase(Ease.InQuart).SetRelative())
            .Append(icon.parts[0].transform.DOLocalRotate(new Vector3(0f, 0f, -720f), 1f).SetEase(Ease.OutBack).SetRelative())
            .Append(fightImage.DOShakePosition(shakeTime, strength: new Vector3(10f, 0, 0), vibrato: 20, randomness: 90, snapping: false, fadeOut: true))
            .Append(DOVirtual.DelayedCall(1f, () => whiteBar.transform.DOScaleY(0, 0.2f).SetEase(Ease.Linear)))
            .Append(icon.parts[0].transform.DOLocalRotate(Vector3.zero, 0.2f))
            .Append(fightImage.DOScale(0f, 0.2f));
    }

    private Sequence CreateNextRoundSequence(BattleIcon icon)
    {
        return DOTween.Sequence()
            .Append(icon.parts[0].transform.DOLocalRotate(new Vector3(-720f, 0f, 0f), 0.6f).SetEase(Ease.InQuart).SetRelative())
            .Append(fightImage.DOShakePosition(shakeTime, strength: new Vector3(10f, 0, 0), vibrato: 20, randomness: 90, snapping: false, fadeOut: true))
            .Append(DOVirtual.DelayedCall(1f, () => whiteBar.transform.DOScaleY(0, 0.2f).SetEase(Ease.Linear)))
            .Append(icon.parts[0].transform.DOLocalRotate(Vector3.zero, 0.6f).SetEase(Ease.InQuart))
            .Append(fightImage.DOScale(0f, 0.2f));
    }

    private Sequence CreateMyTurnSequence(BattleIcon icon)
    {
        return DOTween.Sequence()
            .Append(icon.parts[0].transform.DOScale(new Vector3(3.77f, 3.77f, 10f), 0.2f).SetEase(Ease.InQuart))
            .Join(icon.parts[1].transform.DOScale(new Vector3(3.77f, 3.77f, 10f), 0.2f).SetEase(Ease.InQuart))
            .Append(icon.parts[0].transform.DOLocalRotate(new Vector3(0f, 0f, 720f), 1f).SetEase(Ease.OutBack).SetRelative())
            .Join(icon.parts[1].transform.DOLocalRotate(new Vector3(0f, 0f, -720f), 1f).SetEase(Ease.OutBack).SetRelative())
            .Append(icon.parts[0].transform.DOLocalMove(new Vector3(15f, 2.5f, 1f), 0.3f).SetEase(Ease.OutBack))
            .Join(icon.parts[1].transform.DOLocalMove(new Vector3(-15f, 2.5f, 1f), 0.3f).SetEase(Ease.OutBack))
            .Append(fightImage.DOShakePosition(shakeTime, strength: new Vector3(10f, 0, 0), vibrato: 20, randomness: 90, snapping: false, fadeOut: true))
            .Append(DOVirtual.DelayedCall(1f, () => whiteBar.transform.DOScaleY(0, 0.2f).SetEase(Ease.Linear)))
            .Append(icon.parts[0].transform.DOLocalMove(new Vector3(112f, 80.14f, 1f), 0.3f))
            .Join(icon.parts[1].transform.DOLocalMove(new Vector3(-112f, 80.14f, 1f), 0.3f))
            .Join(icon.parts[0].transform.DOScale(new Vector3(0f, 0f, 10f), 0.2f))
            .Join(icon.parts[1].transform.DOScale(new Vector3(0f, 0f, 10f), 0.2f))
            .Append(fightImage.DOScale(0f, 0.2f));
    }

    #endregion

    private void KillActiveSequences()
    {
        if (_animSequence != null && _animSequence.IsActive()) _animSequence.Kill(true);
        if (_startTurnSequence != null && _startTurnSequence.IsActive()) _startTurnSequence.Kill(true);
        _startTurnSequence = null;
    }
}