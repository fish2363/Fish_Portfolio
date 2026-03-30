using Ami.BroAudio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UICanvas : MonoBehaviour
{
    [SerializeField] private Transform[] uiParents;

    protected virtual void Start()
    {
        UIManager.SetCanvas(uiParents);

        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0:
                BroAudio.Play(SoundIDs.Instance.Title);
                break;
            case 2:
                BroAudio.Play(SoundIDs.Instance.Main);
                break;
        }
    }
}