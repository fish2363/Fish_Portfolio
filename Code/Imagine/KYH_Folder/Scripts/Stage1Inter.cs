using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage1Inter : MonoBehaviour
{
    private CusionTutorial goToBad;
    public bool badEnter = false;
    public bool InBasket = false;
    new Transform transform;
    public GameObject cusion;
    public GameObject plsPress;
    private GameObject plsPressCusionPack;
    GameObject informationText;
    public static bool Lava;
    private bool holdEnter = false;
    private Rigidbody2D _rigidbody2D;
    Animator inter;


    private void Awake()
    {
        inter = GameObject.Find("kidSprite").GetComponent<Animator>();
        //informationText = GameObject.Find("Information");
        plsPress = GameObject.Find("PushE");
        plsPressCusionPack = GameObject.Find("CusionPushInterface");
        _rigidbody2D = GetComponentInParent<Rigidbody2D>();

    }
    private void Start()
    {
        //informationText.SetActive(false);
        plsPress.SetActive(false);
        plsPressCusionPack.SetActive(false);
    }
    private void Update()
    {
        // e키를 누르고 badenter가 true면 실행
        if (Input.GetKeyDown(KeyCode.E) && badEnter && Lava == false)
        {
            CusionTutorial.isCatch = true;
            plsPress.SetActive(false);

        }
        if (Input.GetKeyDown(KeyCode.E) && InBasket)
        {

            //Instantiate(cusion);
            //cusion = null;
            CusionTutorial.isCatch = true;
            plsPress.SetActive(false);
            plsPressCusionPack.SetActive(false);
            Lava = false;
        }
        if (holdEnter)
        {
            _rigidbody2D.gravityScale = 0;
            _rigidbody2D.velocity = Vector3.zero;
        }
        else if (!holdEnter)
        {
            _rigidbody2D.gravityScale = 1f;
        }
        //if (Input.GetKeyDown(KeyCode.E) && badEnter)
        //{
        //    CusionTutorial.isCatch = true;
        //    plsPress.SetActive(false);
        //    informationText.SetActive(true); 
        //}
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 만약 닿은 친구가 cusion태그를 가지고 있다면
        if (collision.CompareTag("cusion"))
        {
            if (!Lava)
            {
                badEnter = true;
            goToBad = collision.gameObject.GetComponent<CusionTutorial>();
            transform = gameObject.GetComponentInParent<Transform>();
            goToBad.Hehe(transform);
                plsPress.SetActive(true);
            }
            //print(transform.name);

            //cusion = cusion.gameObject;
            //PutOnCusion(cusion);
        }

        if (collision.CompareTag("Shoes"))
        {
            InBasket = true;
            plsPressCusionPack.SetActive(true);
            print(collision.name);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("cusion"))
        {
            badEnter = false;
            goToBad = null;
            transform = null;
            plsPress.SetActive(false);

        }
        if (collision.CompareTag("Shoes"))
        {
            plsPressCusionPack.SetActive(false);
            InBasket = false;
            print(collision.name);
        }
        if (collision.CompareTag("Holding"))
        {
            holdEnter = false;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Holding") && Input.GetKey(KeyCode.Q))
        {
            inter.SetBool("Hold", true);
            holdEnter = true;
        }
        else
        {
            inter.SetBool("Hold", false);
            holdEnter = false;
        }
    }

    //public void PutOnCusion(GameObject OnCusion)
    //{
    //    OnCusion = cusion;
    //}
}
