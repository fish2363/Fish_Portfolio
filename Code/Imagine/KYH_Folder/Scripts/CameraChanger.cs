using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;


public class CameraChanger : MonoBehaviour
{
    GameObject camRightNow;
    GameObject cam2;
    GameObject textBox;
    public TextMeshProUGUI text;
    public GameObject changer2;
    bool stop = false;

    private void Awake()
    {
        //changer2 = GameObject.Find("CameraChanger2");
        textBox = GameObject.Find("TextBox");
        cam2 = GameObject.Find("cam02");
        camRightNow = GameObject.Find("PlayerCam");
    }

    private void Start()
    {
        changer2.SetActive(false);
        textBox.SetActive(false);
        cam2.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.isAction = true;
            camRightNow.SetActive(false);
            cam2.SetActive(true);
            StartCoroutine(Wait());
        }
    }

    public static void TmPDOText(TextMeshProUGUI text, float duration)
    {
        text.maxVisibleCharacters = 0;

        DOTween.To(x => text.maxVisibleCharacters = (int)x, 0f, text.text.Length, duration);
    }

    IEnumerator Wait()
    {
        yield return new WaitForSecondsRealtime(2);
        textBox.SetActive(true);
        print("ang");
        text.text = "침대에서는 장난치지 \n말라고 하셨는데..";
        TmPDOText(text, 3f);
        yield return new WaitForSecondsRealtime(5);
        text.text = "잘못뛰면 달까지\n 날아간다고 얘기하셨어";
        TmPDOText(text, 3f);
        yield return new WaitForSecondsRealtime(4);
        text.text = "조심히 뛰어보자";
        TmPDOText(text, 2f);
        stop = true;
        
    }
    private void Update()
    {
        if(stop)
        {
            changer2.SetActive(true);
            GameManager.isAction = false;
            camRightNow.SetActive(true);
            cam2.SetActive(false);
            stop = false;
        }
    }

    bool WaitClick(bool stop)
    {
        while(!(stop))
        {
            if(Input.GetMouseButtonDown(0))
            {
                stop = true;
            }
        }
        return stop;
    }
}
