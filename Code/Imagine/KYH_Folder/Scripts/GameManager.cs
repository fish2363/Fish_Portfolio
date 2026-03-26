using Cinemachine;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TalkManagement talkManagement;
    public GameObject talkPanel;
    public TextMeshProUGUI talkText;
    public GameObject scanObject;
    public static bool isAction;
    public static int stopAni = 1;
    public bool isoneplayer = true;
    Transform kid;
    Transform friend;
    Interaction playerInteraction;
    Interaction friendInteraction;

    public int talkIndex;
    bool player;
    GameObject gotobad;
    new CinemachineVirtualCamera camera;


    public void Awake()
    {
        gotobad = GameObject.FindGameObjectWithTag("cusion");
        camera = GameObject.Find("MainPlayCam").GetComponent<CinemachineVirtualCamera>();

        kid = GameObject.Find("Player").transform;
        friend = GameObject.Find("Friend").transform;

        playerInteraction = kid.transform.Find("inttaget").GetComponent<Interaction>();
        friendInteraction = friend.transform.Find("inttaget").GetComponent<Interaction>();
        Debug.Log(playerInteraction is null);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1")&&isoneplayer==false)
        {
            if (player)
            {
                stopAni = 1;
                camera.Follow = kid;
            }
            else
            {
                stopAni = 2;
                camera.Follow = friend;
            }

            playerInteraction.isActive = player;
            friendInteraction.isActive = !player;

            player = !player;

        }


    }

    //public void Action(GameObject scanObj)
    //{
    //    if(isAction)
    //    {
    //        isAction = false;
    //    }
    //    else
    //    {
    //        isAction = true;
    //        scanObject = scanObj;
    //        ObjData objData = scanObject.GetComponent<ObjData>();
    //        //Talk(objData.id, objData.isNpc);
    //    }
    //    talkPanel.SetActive(isAction);
    //}

    //void Talk(int id, bool isNpc)
    //{
    //    string talkData = talkManagement.GetTalk(id, talkIndex);

    //    if(isNpc)
    //    {
    //        talkText.text = talkData;
    //    }
    //    else
    //    {
    //        talkText.text = talkData;
    //    }
    //}
}
