using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PlayerController : SpriteSystem, IControllerPhysics
{
    private Rigidbody2D rigid;
    private SpriteRenderer KidRenderer;
    private SpriteRenderer BabyRenderer;
    private Animator KidAni;
    bool isHorizonMove;
    private Vector3 dirVec;
    private GameObject scanObject;
    public bool isLookUp;
    public float v;
    private Vector2 moveDir;
    private Vector2 footPosition;
    private Animator BabyAni;
    private Gotobad gotobad;
    private BoxCollider2D colly;
    private bool isDead = false;
    public Vector3 savepos;
    SpriteRenderer deathScreen;
    FriendController friendControll;
    [SerializeField] private LayerMask ground;
    [SerializeField] private LayerMask whatIsObj;
    [SerializeField] private Transform pos;
    [SerializeField] private Vector2 size;
    public UnityEvent Dead;
    public bool isoneplayer = true;

    //private Transform cusionUpTransform;
    //private Transform plTransform;
    //private Transform friendTransform;

    public bool isCollisionStay { get; set; } = false;
    [field: SerializeField] public float moveSpeed { get; set; } = 3;
    [field: SerializeField] public float jump { get; set; } = 6;
    [field: SerializeField] public float h { get; set; }
    public Transform trm => transform;
    [field: SerializeField] public bool isGround { get; set; }

    bool currentFlip = false;

    public LayerMask interactableLayer;
    public float interactionRadius = 3f;
    private Gotobad _gotobad;

    private void Awake()
    {
        deathScreen = GameObject.Find("Death").GetComponent<SpriteRenderer>();
        gotobad = FindObjectOfType<Gotobad>();
        colly = GetComponent<BoxCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
        KidAni = GameObject.Find("kidSprite").GetComponent<Animator>();
        BabyAni = GameObject.Find("BabySprite").GetComponent<Animator>();
        KidRenderer = GameObject.Find("kidSprite").GetComponent<SpriteRenderer>();
        BabyRenderer = GameObject.Find("BabySprite").GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        isGround = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        /*if ((collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("cusion")) && collision.contacts[0].normal.y > 0.7f)
        {
            isGround = true;
            KidAni.SetBool("Hoit", false);
        }\
        */
        if (collision.gameObject.CompareTag("Magema"))
        {
            isDead = true;
        }
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("cusion") || collision.gameObject.CompareTag("Player") && collision.contacts[0].normal.y > 0.7f)
        {
            isGround = true;
            KidAni.SetBool("Hoit", false);
        }
        if (collision.contacts[0].normal.y > 0.7f && collision.gameObject.CompareTag("Tram"))
        {
            rigid.AddForce(Vector2.up * 1.5f * jump, ForceMode2D.Impulse);
        }
    }

    void Update()
    {


        //Collider2D[] hit = Physics2D.OverlapBoxAll(pos.position, size, 0);
        //foreach (Collider2D ray in hit)
        //{
        //    if ( && ray.gameObject.CompareTag("Floor"))
        //    {
        //        isGround = true;
        //        KidAni.SetBool("Hoit", false);
        //    }
        //}
        Bounds bounds = colly.bounds;
        footPosition = new Vector2(bounds.center.x, bounds.min.y);
        isGround = Physics2D.OverlapCircle(footPosition, 0.1f, ground);
        Collider2D coll = Physics2D.OverlapCircle(footPosition, 1f, whatIsObj);
        if(coll != null)
            gotobad.cusionUpTransform = coll.gameObject.transform;
        //if (!Gotobad.isCatch)
        //{
        //    gotobad.cusionUpTransform = null;
        //}

        KidAni.SetBool("Hoit", !(isGround));


        if (isDead)
        {
            Dead.Invoke();
            KidAni.SetBool("Die", true);
            deathScreen.DOFade(1, 1);
            GameManager.isAction = true;
            StartCoroutine(Death());
        }
        h = GameManager.isAction ? 0 : Input.GetAxisRaw("Horizontal");
        v = GameManager.isAction ? 0 : Input.GetAxisRaw("Vertical");
        moveDir = GameManager.isAction ? new Vector2(0, 0) : new Vector2(h, 0);

        bool hDown = GameManager.isAction ? false : Input.GetButtonDown("Horizontal");
        bool vDown = GameManager.isAction ? false : Input.GetButtonDown("Vertical");
        bool hUp = GameManager.isAction ? false : Input.GetButtonDown("Horizontal");
        bool vUp = GameManager.isAction ? false : Input.GetButtonDown("Vertical");

        if (hDown)
            isHorizonMove = true;
        else if (vDown)
            isHorizonMove = false;
        else if (hUp || vUp)
            isHorizonMove = h != 0;

        if (vDown && v == 1)
        {
            isLookUp = true;
        }
        else if (vDown && v == -1)
        {
            isLookUp = true;
        }
        else if (hDown)
        {
        }
        else if (v == 0)
        {
            isLookUp = false;
        }
        if (h == -1)
        {
            if (currentFlip != true)
            {
                currentFlip = true;
                dirVec = Vector3.left; //레이캐스트를 위한 벡터 방향 지정
                Flip(currentFlip);
            }
        }
        else if (h == 1)
        {
            if (currentFlip != false)
            {
                currentFlip = false;
                dirVec = Vector3.right;
                Flip(currentFlip);
            }
        }

        if (Input.GetButtonDown("Fire1")&&!isoneplayer)
        {
            print("뙛어용");
            isHorizonMove = false;
            h = 0;
            isGround = false;

            KidAni.SetBool("Walk", false);
            gameObject.GetComponent<PlayerController>().enabled = false;
            GameObject.Find("Friend").GetComponent<FriendController>().enabled = true;
        }

        if (GameManager.stopAni == 1)
        {
            KidAni.SetBool("Stop", false);
            if (moveSpeed == 0)
            {
                moveSpeed = 3;
            }


            //Animation
            if (Input.GetButtonDown("Jump") && isGround && playerType == 2)
            { 
                rigid.velocity = Vector2.zero;
                rigid.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
            }
            else
            {
                switch (playerType)
                {
                    case 1:
                        BabyAni.SetBool("Walk", moveDir.magnitude > 0);
                        break;
                    case 2:
                        KidAni.SetBool("Walk", moveDir.magnitude > 0);

                        KidAni.SetFloat("vAxisRaw", v);
                        break;
                }
            }
        }
        else
        {
            KidAni.SetBool("Stop", GameManager.stopAni == 2);
            moveSpeed = 0;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(footPosition, 0.1f);
    }

    private void Flip(bool value)
    {
        switch (playerType)
        {
            case 1:
                BabyRenderer.flipX = value;
                break;
            case 2:
                KidRenderer.flipX = value;
                break;
        }
    }

    private void FixedUpdate()
    {

        if (isLookUp == false)
        {
            Vector2 moveVec = isHorizonMove ? new Vector2(h * moveSpeed, rigid.velocity.y) : new Vector2(0, rigid.velocity.y);
            rigid.velocity = moveVec;
        }

        Debug.DrawRay(rigid.position, dirVec * 0.7f, new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, dirVec, 0.7f, LayerMask.GetMask("Object"));

        if (rayHit.collider != null)
            scanObject = rayHit.collider.gameObject;
        else
            scanObject = null;

    }
    IEnumerator Death()
    {
        isDead = false;
        yield return new WaitForSecondsRealtime(2); //3초후에 밑에꺼 실행
        transform.position = savepos;
        KidAni.SetBool("Die", false);
        GameManager.isAction = false;
        deathScreen.DOFade(0, 1);
    }

}