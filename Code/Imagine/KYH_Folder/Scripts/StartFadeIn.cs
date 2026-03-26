using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartFadeIn : NextYear
{
    public SpriteRenderer panel;
    float time = 0f;
    float F_time = 1f;
    // Start is called before the first frame update

    private void Awake()
    {
        panel = GameObject.Find("Black").GetComponent<SpriteRenderer>();  
    }

    public void Update()
    {
        if(fadeOut)
        {
            StartCoroutine(FadeIn());
            Debug.Log("ÆäÀÌµå¾Æ¿ô");
        }
        else if (fadeIn)
        {
            StartCoroutine(FadeOut());
        }
    }

    public IEnumerator FadeOut()
    {
        panel.gameObject.SetActive(true);
        Color alpha = panel.color;
        while(alpha.a > 0f)
        {
            time += Time.deltaTime / F_time;
            alpha.a = Mathf.Lerp(1, 0, time);
            panel.color = alpha;
            yield return null;
        }
        yield return null;
        time = 0f;
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

    void Start()
    {
        StartCoroutine(FadeOut());
    }
}
