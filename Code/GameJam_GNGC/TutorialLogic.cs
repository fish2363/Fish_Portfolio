using UnityEngine;
using System;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using DG.Tweening;
using UnityEngine.InputSystem;

public enum IDialogueSkip
{
    Defalut,
    Next,
    Prev
}

public class TutorialLogic : MonoBehaviour
{
    [Header("대사 입력하기 \\n을 입력하면 띄어쓰기")]
    public Dialogue[] tutorialDialogue;
    int idx;
    private IDialogueSkip isMouse = IDialogueSkip.Defalut;
    private bool isOn;

    [SerializeField] private Image[] tutorialImage;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private RectTransform bubbleBackground;

    [Header("말풍선 설정")]
    [SerializeField] private Vector2 padding = new Vector2(20f, 10f);
    [SerializeField] private Vector2 minSize = new Vector2(100f, 40f);
    [SerializeField] private Vector2 maxSize = new Vector2(500f, 200f);

    [SerializeField]
    private GameObject tip;

    public Volume volume;
    private ColorAdjustments colorAdjustments;

    void Start()
    {
        if (!volume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("Color Adjustments not found in Volume Profile!");
        }
    }

    public void TutorialStart()
    {
        idx = 0;
        tip.SetActive(true);
        TutorialIN(-100f);
        bubbleBackground.gameObject.SetActive(true);
        GetComponent<CanvasGroup>().alpha = 1f;
        isOn = true;
        StartCoroutine(DialogueRoutine());
    }

    void TutorialIN(float value)
    {
        DOTween.To(
            () => colorAdjustments.saturation.value,
            x => colorAdjustments.saturation.value = x,
            value,
            0.1f
        ).SetEase(Ease.InOutSine);
    }

    private void Update()
    {
        if (isOn && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Space)))
            isMouse = IDialogueSkip.Next;
        if(isOn && (Input.GetKeyDown(KeyCode.LeftArrow)))
            isMouse = IDialogueSkip.Prev;
    }

    private IEnumerator DialogueRoutine()
    {
        _text.text = tutorialDialogue[idx].tutorialText;
        bubbleBackground.transform.position = tutorialDialogue[idx].TextPoint.position;

        ResizeBubble();

        if (tutorialImage.Length > idx)
            tutorialImage[idx].gameObject.SetActive(true);

        yield return new WaitUntil(() => isMouse != IDialogueSkip.Defalut);

        if (tutorialImage.Length > idx)
            tutorialImage[idx].gameObject.SetActive(false);

        if(isMouse == IDialogueSkip.Next)
            idx++;
        else if (isMouse == IDialogueSkip.Prev && (idx -1) >= 0)
            idx--;
        isMouse = IDialogueSkip.Defalut;

        if (idx < tutorialDialogue.Length)
            StartCoroutine(DialogueRoutine());
        else
        {
            TutorialIN(-10f);
            _text.text = "";
            bubbleBackground.gameObject.SetActive(false);
            GetComponent<CanvasGroup>().alpha = 0f;
            FindAnyObjectByType<MainmenuLogic>().CanceledButton(true);
            isOn = false;
            tip.SetActive(false);
        }
    }

    private void ResizeBubble()
    {
        if (_text == null || bubbleBackground == null) return;

        Vector2 preferredSize = _text.GetPreferredValues(_text.text);
        Vector2 targetSize = preferredSize + padding;

        targetSize.x = Mathf.Clamp(targetSize.x, minSize.x, maxSize.x);
        targetSize.y = Mathf.Clamp(targetSize.y, minSize.y, maxSize.y);

        bubbleBackground.sizeDelta = targetSize;
    }
}

[Serializable]
public struct Dialogue
{
    public string tutorialText;
    public Transform TextPoint;
}