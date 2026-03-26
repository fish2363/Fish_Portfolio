using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lanturn : MonoBehaviour
{
    public Sprite[] sprite;
    SpriteRenderer backGround;
    GameObject playerCam;
    GameObject cam;
    float time = 0f;
    float F_time = 1f;
    private bool rightWall = false;
    SpriteRenderer rightWallRenderer;
    GameObject leftWall;
    GameObject text;


    private void Awake()
    {
        text = GameObject.Find("hmm");
        backGround = GameObject.Find("BackGround").GetComponent<SpriteRenderer>();
        playerCam = GameObject.Find("PlayerCam");
        cam = GameObject.Find("MainCam");
        rightWallRenderer = GameObject.Find("RightWall").GetComponent<SpriteRenderer>();
        leftWall = GameObject.Find("LeftWall");

    }

    private void Start()
    {
        playerCam.SetActive(false);
    }
    private void Update()
    {
        if (rightWall)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
                StartCoroutine(FadeOut());
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("cusion"))
        {
            backGround.color = Color.black;
            gameObject.SetActive(false);
            playerCam.SetActive(true);
            cam.SetActive(false);
            gameObject.SetActive(false);
            rightWall = true;
            leftWall.SetActive(false);
            text.SetActive(false);
        }
    }

    public IEnumerator FadeOut()
    {
        Color alpha = rightWallRenderer.color;
        while (alpha.a > 0f)
        {
            time += Time.deltaTime / F_time;
            alpha.a = Mathf.Lerp(1, 0, time);
            rightWallRenderer.color = alpha;
            yield return null;
        }
        yield return null;
        time = 0f;
    }

    public IEnumerator FadeIn()
    {
        print("ž´ž´");
        Color alpha = rightWallRenderer.color;
        while (alpha.a < 1f)
        {
            time += Time.deltaTime / F_time;
            alpha.a = Mathf.Lerp(0, 1, time);
            rightWallRenderer.color = alpha;
            yield return null;
        }
        yield return null;
        time = 0f;
    }
}
