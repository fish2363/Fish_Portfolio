using System;
using UnityEngine;
using DG.Tweening;
using Ami.BroAudio;

public class InGameManager : MonoBehaviour
{
    [SerializeField] private GameEventChannelSO inGameEvent;
    [SerializeField] private RectTransform _cardUI;
    [SerializeField] private RectTransform _cinemaUI;
    [SerializeField] private Ease cardUIEase;
    [SerializeField] private Ease cinemaEase;
    [SerializeField] private float cardUiSpeed=0.2f;
    [SerializeField] private float cinemaUISpeed=0.2f;
    [SerializeField] private GameObject blockPanel;

    [SerializeField] private SoundID stage1Bgm;

    void Awake()
    {
        inGameEvent.AddListener<CinemaUIEvent>(OnUIPhaseChanged);
        inGameEvent.AddListener<ComeUpCardUIEvent>(HandleGameStart);
        inGameEvent.AddListener<ComeDownCardUIEvent>(HandleGameEnd);
        BroAudio.Stop(BroAudioType.Music);
        BroAudio.Play(stage1Bgm);
    }
    private void OnDestroy()
    {
        inGameEvent.RemoveListener<ComeUpCardUIEvent>(HandleGameStart);
        inGameEvent.RemoveListener<ComeDownCardUIEvent>(HandleGameEnd);
        inGameEvent.RemoveListener<CinemaUIEvent>(OnUIPhaseChanged);
    }
    private void HandleGameStart(ComeUpCardUIEvent obj)
    {
        _cardUI.DOAnchorPosY(198f, cardUiSpeed).SetEase(cardUIEase);
        _cardUI.GetComponent<CanvasGroupCompo>().SetGroup(true);
    }

    private void HandleGameEnd(ComeDownCardUIEvent obj)
    {
        _cardUI.DOAnchorPosY(-230f, cardUiSpeed).SetEase(cardUIEase);
        _cardUI.GetComponent<CanvasGroupCompo>().SetGroup(false);
    }

    private void OnUIPhaseChanged(CinemaUIEvent evt)
    {
        if (!evt.IsRunning)
        {
            blockPanel.SetActive(true);
            _cardUI.DOAnchorPosY(-230f, cardUiSpeed).SetEase(cardUIEase);
            _cardUI.GetComponent<CanvasGroupCompo>().SetGroup(false);

            _cinemaUI.DOScale(1f, cinemaUISpeed).SetEase(cinemaEase);
        }
        else
        {
            blockPanel.SetActive(false);
            _cardUI.DOAnchorPosY(198f, cardUiSpeed).SetEase(cardUIEase);
            _cardUI.GetComponent<CanvasGroupCompo>().SetGroup(true);

            _cinemaUI.DOScale(1.3f, cinemaUISpeed).SetEase(cinemaEase);
        }
    }
}
