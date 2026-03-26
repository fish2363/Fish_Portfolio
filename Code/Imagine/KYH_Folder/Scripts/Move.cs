using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Cinemachine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Move : MonoBehaviour
{
    //DOMove(vector 원하는 위치, float 이동 시간, bool 스냅핑);
    //DOLocalMove(); // Local 좌표로도 이동 시킬 수도 있다.
    //transform.DOScale(targetScale, 3).SetEase(ease); 크기변경
    //transform.DORotate(new Vector3(0, -90, 0), 3) 회전
    //transform.DOShakeRotation(3) 흔들기
    //public RectTransform rect;
    public Ease ease;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text;
    public TextMeshProUGUI textStart;
    public TextMeshProUGUI textWhat;
    public TextMeshProUGUI startText;
    public GameObject textBoxMom;
    public GameObject textBoxKid;
    public GameObject camOn;
    public GameObject camTw;
    public bool mouse = false;
    public int textNum = 1;
    private float targetZoomSize;
    public CinemachineVirtualCamera camOne;
    public CinemachineVirtualCamera camTwo;
    private GameObject tutorial;
    private Animator animator;
    //private GameObject shoes;
    public PlayableDirector start;
    SpriteRenderer spriteRenderer;
    GameObject textBox;
    GameObject textBox2;
    SpriteRenderer couch;
    SpriteRenderer surap;
    public SpriteRenderer panel;
    float time = 0f;
    float F_time = 1f;
    SpriteRenderer player;
    SpriteRenderer cusion;
    Animator mom;
    SpriteRenderer mouseClick;
    public AudioSource door;
    public AudioSource nextAudio;
    public AudioSource aaudio;

    private void Awake()
    {
        mouseClick = GameObject.Find("Mouse").GetComponent<SpriteRenderer>();
        mom = GameObject.Find("MOm").GetComponent<Animator>();
        cusion = GameObject.Find("cusion").GetComponent<SpriteRenderer>();
        couch = GameObject.Find("Couch").GetComponent<SpriteRenderer>();
        surap = GameObject.Find("Surap").GetComponent<SpriteRenderer>();
        textBox = GameObject.Find("TextBoxUI");
        textBox2 = GameObject.Find("TextBoxUI2");
        spriteRenderer = GameObject.Find("LookUp").GetComponent<SpriteRenderer>();
        targetZoomSize = camOne.m_Lens.OrthographicSize;
        tutorial = GameObject.Find("Player");
        animator = GameObject.Find("LookUp").GetComponent<Animator>();
        //shoes = GameObject.Find("Shoes");
        textBox.SetActive(false);
        textBox2.SetActive(false);
        panel = GameObject.Find("Black").GetComponent<SpriteRenderer>();
        player = GameObject.Find("LookUp").GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        text.text = "금방 갔다올게";
        TmPD0Text(text, 1.3f);
        tutorial.GetComponent<TutorialPlayer>().enabled = false;
        animator.SetFloat("vAxisRaw", 1);
        //shoes.SetActive(false);
    }

    private void Update()
    {
        
        if(!(mouse))
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseClick.DOFade(0,1);

                print("발동!");
                textNum++;
                mouse = true;
                TextLine();
                mouse = false;
            }
        }
    }

    public static void TmPD0Text(TextMeshProUGUI text, float duration)
    {
        text.maxVisibleCharacters = 0;

        DOTween.To(x => text.maxVisibleCharacters = (int)x, 0f, text.text.Length, duration);
    }

    void TextLine()
    {
        switch(textNum)
        {
            case 2:
                mouseClick.DOFade(1, 1);

                text.text = "기다리고 있으렴.";
                TmPD0Text(text, 1.3f);
                break;
            case 3:
                mouseClick.DOFade(1, 1);

                textBoxMom.SetActive(false);
                camTw.SetActive(false);
                camOn.SetActive(true);
                text2.text = "네";
                TmPD0Text(text, 1.3f);
                mom.gameObject.GetComponent<SpriteRenderer>().flipX = false;
                mom.SetBool("Move", true);
                door.Play();
                break;
            case 4:
                mom.gameObject.SetActive(false);
                camTw.SetActive(true);
                camOn.SetActive(false);
                animator.SetFloat("vAxisRaw", 0);
                tutorial.GetComponent<TutorialPlayer>().enabled = true;
                //startText.text = "집에서 뭐하고 놀아야할까?";
                TmPD0Text(startText, 1.3f);
                //shoes.SetActive(true);
                start.Play();
                break;
            case 11:

                break;
        }
    }

    public IEnumerator Typing(string talk)
    {
        text2.text = talk;
        TmPD0Text(text2, 3f);

        yield return new WaitForSecondsRealtime(1.5f);
        //NextTalk();
        print("끝");
    }

    public static void TmPDOText(TextMeshProUGUI text, float duration)
    {
        text.maxVisibleCharacters = 0;

        DOTween.To(x => text.maxVisibleCharacters = (int)x, 0f, text.text.Length, duration);
    }

    public void TextBoring()
    {
        StartCoroutine(Waitwo());
    }
    public void TextWhat()
    {
        StartCoroutine(WhatCanIDo());
    }

    IEnumerator Waitwo()
    {
        textBox.SetActive(true);
        textStart.text = "혼자 남았네..";
        aaudio.Stop();
        nextAudio.Play();
        TmPDOText(textStart, 1f);
        yield return new WaitForSecondsRealtime(3);
        textStart.text = "음..";
        TmPDOText(textStart, 1f);
        yield return new WaitForSecondsRealtime(3);
        textStart.text = "심심해";
        TmPDOText(textStart, 1f);
        yield return new WaitForSecondsRealtime(3);
        textBox.SetActive(false);
    }

    IEnumerator WhatCanIDo()
    {
        textBox2.SetActive(true);
        textWhat.text = "뭐하고 놀지?";
        TmPDOText(textWhat, 1f);
        yield return new WaitForSecondsRealtime(3);
        textBox2.SetActive(false);
        yield return new WaitForSecondsRealtime(3);
        textBox2.SetActive(true);
        textWhat.text = "흠..";
        TmPDOText(textWhat, 1f);
        yield return new WaitForSecondsRealtime(3);
        textWhat.text = "좋아..";
        TmPDOText(textWhat, 1f);
        nextAudio.Stop();

        yield return new WaitForSecondsRealtime(3);
        textBox2.SetActive(false);
        yield return new WaitForSecondsRealtime(2);
        textBox2.SetActive(true);
        textWhat.text = "어떤 상상을 해보지?";
        TmPDOText(textWhat, 1f);
        yield return new WaitForSecondsRealtime(3);
        couch.DOFade(1, 1);
        textBox2.SetActive(false);
        yield return new WaitForSecondsRealtime(7);
        textBox2.SetActive(true);
        textWhat.text = "쇼파가 있네";
        TmPDOText(textWhat, 1f);
        yield return new WaitForSecondsRealtime(4);
        surap.DOFade(1, 1);
        spriteRenderer.flipX = false;
        textWhat.text = "서랍도 있고..";
        TmPDOText(textWhat, 1f);
        yield return new WaitForSecondsRealtime(3);
        spriteRenderer.flipX = true;
        surap.DOFade(0, 1);
        couch.DOFade(0, 1);
        cusion.DOFade(0, 2);
        yield return new WaitForSecondsRealtime(3);

        textWhat.text = "그래 이제부터 바닥이 용암인거야";
        TmPDOText(textWhat, 1f);
        yield return new WaitForSecondsRealtime(3);
        textBox2.SetActive(false);
        yield return new WaitForSecondsRealtime(5);
        player.DOFade(0, 2);
        yield return new WaitForSecondsRealtime(3);
        SceneManager.LoadScene("Stage1");
    }

    public void Flip()
    {
        spriteRenderer.flipX = true;
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        Color alpha = panel.color;
        while (alpha.a < 1f)
        {
            time += Time.deltaTime / F_time;
            alpha.a = Mathf.Lerp(0, 1, time);
            panel.color = alpha;
            yield return null;
        }
        yield return null;
        time = 0f;
    }

}
