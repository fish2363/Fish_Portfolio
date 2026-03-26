using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkManagement : MonoBehaviour
{
    Dictionary<int, string[]> talkData;

    private void Awake()
    {
        talkData = new Dictionary<int, string[]>();
        GenerateData();
    }

    void GenerateData()
    {
        talkData.Add(101, new string[] { "그렇지! 엄마한테 오렴.." });
        talkData.Add(1, new string[] { "평범한 테스트용 더미네?" });
        talkData.Add(2, new string[] { "평범한 테스트용 공이다." });
    }

    public string GetTalk(int id, int talkIndex)
    {
        return talkData[id][talkIndex];
    }
}
