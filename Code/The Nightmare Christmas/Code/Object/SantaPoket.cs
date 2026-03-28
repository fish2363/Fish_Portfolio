using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SantaPoket : MonoBehaviour, IInteractable
{
    public GameObject wall;
    public void Interact()
    {
        ChatManager.Instance.Chat(2,"애들을 위한 선물가방이지만 어쩔 수 없지");
        gameObject.GetComponent<ObjectOutLine>().enabled = false;
        StartCoroutine(NextChat());
    }

    private IEnumerator NextChat()
    {
        yield return new WaitForSeconds(3f);
        wall.SetActive(false);
        ChatManager.Instance.Chat(3, "[G]를 눌러서 아이들 선물을 열자");
        FindAnyObjectByType<Item>().isGetPresent = true;
        Destroy(gameObject);
    }
}
