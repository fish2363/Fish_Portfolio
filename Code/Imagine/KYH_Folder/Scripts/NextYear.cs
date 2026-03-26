using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextYear : SpriteSystem
{
    public static bool fadeOut = false;
    public static bool fadeIn = false;
    bool trigger;
    int year =1;
    // Start is called before the first frame update


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            trigger = true;
        }
    }

    IEnumerator Waiting()
    {
        yield return new WaitForSecondsRealtime(1);
        year = 2;
    }

    // Update is called once per frame
    void Update()
    {
        if(trigger)
        {
            switch (year)
            {
                case 1:                   
                        fadeOut = true;
                        StartCoroutine(Waiting());
                    break;
                case 2:
                        fadeOut = false;
                        fadeIn = true;
                        playerType = 2;
                        GameManager.isAction = false;
                    break;
            }
        }
    }
}
