using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tutorial : MonoBehaviour
{
    public TextMeshProUGUI text;
    float time = 0f;
    float F_time = 1f;

    // Start is called before the first frame update
    void Start()
    {
        SpriteSystem.playerType = 1;
        StartCoroutine(TextIn());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator TextIn()
    {
        Color textAlpha = text.color;

        while (textAlpha.a < 1f)
        {
            time += Time.deltaTime / F_time;
            textAlpha.a = Mathf.Lerp(0, 1, time);
            text.color = textAlpha;
            yield return null;
        }
        yield return null;
        time = 0f;
    }
}
