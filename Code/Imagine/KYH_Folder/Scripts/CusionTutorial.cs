using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Profiling;
using UnityEngine;

public class CusionTutorial : MonoBehaviour
{
    public GameObject player;
    [SerializeField] public static bool isCatch = false;
    private bool isShot;
    private BoxCollider2D _boxCollider;
    private Rigidbody2D _Rigidbody2D;
    public Transform cusionUpTransform;
    private Transform plTransform;
    private Transform friendTransform;
    private float speed = 20f;
    private LineRenderer _lineRenderer;
    private Vector3 movedir;
    private GameManager manager;
    private bool isDoubleJump;
    private IControllerPhysics[] playerControll;
    GameObject hitMe;
    GameObject hitMe2;
    ColliderLanturn colliderLanturn;


    private void Awake()
    {
        colliderLanturn = GameObject.Find("BigLanturn").GetComponent<ColliderLanturn>();
        hitMe = GameObject.Find("HitMeBigLanturn");
        hitMe2 = GameObject.Find("HitMeBigLanturn2");
        _Rigidbody2D = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
        plTransform = GameObject.Find("Player").GetComponent<Transform>();
    }


    private void Update()
    {
        print(isCatch);
        //if(GameManager.stopAni == 2 && isCatch == true)
        //{
        //    cusionUpTransform = friendTransform;
        //}
        //if (GameManager.stopAni == 1 && isCatch == true) cusionUpTransform = plTransform;

        Vector3 mousPos = Input.mousePosition;
        mousPos = Camera.main.ScreenToWorldPoint(mousPos);
        movedir = mousPos - plTransform.position;
        Vector3 mosp = mousPos - plTransform.position;

        if (Input.GetMouseButton(0) && isCatch)
        {

            DotDrawer.Instance.Show();
            DotDrawer.Instance.Draw(gameObject.transform.position, mosp.normalized * speed);
        }


        if (Input.GetMouseButtonUp(0) && isCatch)
        {
            DotDrawer.Instance.Clear();

            _boxCollider.enabled = true;
            _Rigidbody2D.AddForce(mosp.normalized * speed, ForceMode2D.Impulse);
            isCatch = false;
            isShot = true;
            hitMe.SetActive(false);

        }


        if (isCatch)
        {
            if(!ColliderLanturn.firstFinish)
            {
                hitMe.SetActive(true);
            }
            if(!ColliderLanturn.secondFinish)
            {
                hitMe2.SetActive(true);
            }
            _Rigidbody2D.velocity = new Vector2(0, 0);
            gameObject.transform.position = plTransform.position + new Vector3(0, 0.5f, 0);
            _Rigidbody2D.gravityScale = 0;
            _boxCollider.enabled = false;
            this.gameObject.layer = 8;

        }
        else
        {
            FallCusion();
        }
    }


    public void FallCusion()
    {
        hitMe.SetActive(false);
        hitMe2.SetActive(false);
        isCatch = false;
        print("tlqkf");
        _Rigidbody2D.gravityScale = 1;
        _boxCollider.enabled = true;
        transform.SetParent(null);
    }




    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Magema"))
        {
            this.gameObject.layer = 7;
            Stage1Inter.Lava = true;
        }



        if (collision.gameObject.CompareTag("Tram"))
        {
            _Rigidbody2D.AddForce(Vector2.up * 6f, ForceMode2D.Impulse);
            if (isShot)
            {
                this.gameObject.layer = 0;
                isDoubleJump = true;
                isShot = false;
            }
        }


        if (isDoubleJump)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.rigidbody.AddForce(Vector2.up * 13f, ForceMode2D.Impulse);
                this.gameObject.layer = 3;
                isDoubleJump = false;
                collision.gameObject.GetComponent<IControllerPhysics>().isGround = false;
                collision.gameObject.GetComponentInChildren<Animator>().SetBool("Hoit", true);
            }
        }
    }

    public void Hehe(Transform trans)
    {
        trans = cusionUpTransform;
        print("tftf");
    }
}
