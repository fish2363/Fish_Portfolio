using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using DG.Tweening;
using System.IO;
using System;
using System.Diagnostics;

public class PasswordMoniter : MonoBehaviour, IInteractable
{
    private CinemachineVirtualCamera inMoniterCamera;
    [SerializeField]
    private Player player;
    [SerializeField]
    private Image blackPanel;
    [SerializeField]
    private GameObject window;

    [SerializeField]
    private GameObject button;

    private Animator animator;

    private bool isClick;
    private bool isMonitor;

    private void Awake()
    {
        inMoniterCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        inMoniterCamera.gameObject.SetActive(false);
        animator = GetComponent<Animator>();
    }

    public void Interact()
    {
        inMoniterCamera.gameObject.SetActive(true);
        inMoniterCamera.Priority = 15;
        player.isStop = true;
        gameObject.layer = 0;
        StartCoroutine(InComputerRoutine());
    }

    private IEnumerator InComputerRoutine()
    {
        yield return null;
        yield return new WaitForSeconds(1f);
        blackPanel.DOFade(1, 1);
        yield return new WaitForSeconds(1f);
        window.SetActive(true);
        blackPanel.DOFade(0, 1);
        isMonitor = true;
        ChatManager.Instance.Chat(7,"S를 눌러 그만보기");
    }

    private void Update()
    {
        if(isMonitor)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!isClick)
                {
                    StartMoniter();
                }
                else
                {
                    ButtonClick();
                }
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                isClick = false;
                button.SetActive(false);
                inMoniterCamera.Priority = -5;
                inMoniterCamera.gameObject.SetActive(false);
                window.SetActive(false);
                player.isStop = false;
                gameObject.layer = 10;
                isMonitor = false;
            }
        }
    }

    public void StartMoniter()
    {
        button.SetActive(true);
        isClick = true;
    }

    public void ButtonClick()
    {
        button.SetActive(false);
        isClick = false;

        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\password.txt";
        print(path);
        var writer = new StreamWriter(File.Open(path, FileMode.OpenOrCreate));
        writer.WriteLine("password is 505");
        writer.Close();

        ProcessStartInfo startInfo = new ProcessStartInfo(path)
        {
            UseShellExecute = true
        };
        Process.Start(startInfo);
    }
}
