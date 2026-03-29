using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // 두트윈 네임스페이스
using UnityEngine.UI;
using TMPro;
using Ami.BroAudio;

public class RosenBridge : MonoBehaviour
{
    public static RosenBridge Instance;

    [SerializeField] private TetrisCompo _tetrisCompo;

    [SerializeField] private Transform left;
    [SerializeField] private Transform right;
    [SerializeField] private GameObject planetPrefabs;

    [Header("왼쪽에서 오른쪽까지 움직이는 시간")]
    [SerializeField] private float moveDuration;

    private Dictionary<PlanetType, Sprite> getPlanetsDictionary = new Dictionary<PlanetType, Sprite>();
    private int idx;

    [Header("행성이미지(순서:수,금,지,화,목,토,천,해)")]
    [SerializeField]
    private Sprite[] planetSprites;

    [Header("애니메이션")]
    public Ease inAnimation;
    public Ease outAnimation;
    public Ease moveAnimation;

    [Header("스케일 튐 설정")]
    [SerializeField] private float squishScale = 0.8f;
    [SerializeField] private float stretchScale = 1.2f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private int loopCount = -1; // 무한 반복

    private RectTransform rectTransform;
    private Sequence bounceSequence;

    private Animator Animator;

    [Header("오디오")]
    [SerializeField]
    private SoundID inSound;

    private void Awake()
    {
        Instance = this;
        rectTransform = GetComponent<RectTransform>();
        Animator = GetComponentInChildren<Animator>();
        foreach (PlanetType type in Enum.GetValues(typeof(PlanetType)))
        {
            getPlanetsDictionary.Add(type, planetSprites[idx]);
            idx++;
        }
    }

    private int _eatCount = 0;
    public void EatPlanet(PlanetType planet)
    {
        Animator.SetBool("Rosen", true);
        BroAudio.Play(inSound);
        _eatCount++;


        GameObject p = Instantiate(planetPrefabs, gameObject.transform);
        p.GetComponent<RectTransform>().position = left.position;

        if (planet == PlanetType.Venus)
            p.GetComponent<BugUIFix>().FixLong();
        if (planet == PlanetType.Earth)
            p.GetComponent<BugUIFix>().FixEarth();

        var sr = p.GetComponent<Image>();
        sr.sprite = getPlanetsDictionary[planet];

        Transform tf = p.transform;
        tf.localScale = Vector3.zero;

        tf.DOScale(Vector3.one, 0.5f).SetEase(inAnimation);

        tf.DOMove(right.position, moveDuration).SetEase(moveAnimation).OnComplete(() =>
        {
            tf.DOScale(Vector3.zero, 0.5f).SetEase(outAnimation).OnComplete(() =>
            {
                _tetrisCompo.SpawnTetris(planet);
                Destroy(p);

                _eatCount--;
                TryFinishAll();
            });
        });
    }


    private void TryFinishAll()
    {
        if (_eatCount == 0)
        {
            Animator.SetBool("Rosen", false);
        }
    }
}