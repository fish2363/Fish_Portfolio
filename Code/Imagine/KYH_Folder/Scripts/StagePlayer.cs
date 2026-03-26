using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using DG.Tweening;

public class StagePlayer : SpriteSystem, IControllerPhysics
{ 
    private Rigidbody2D rigid;
    private SpriteRenderer KidRenderer;
    private Animator kidAni;
    bool isHorizonMove;
    private Vector3 dirVec;
    private GameObject scanObject;
    public bool isLookUp;
    public float v;
    private Vector2 moveDir;
    private Vector2 footPosition;
    private CusionTutorial gotobad;
    private BoxCollider2D colly;
    SpriteRenderer deathScreen;
    private bool isDead = false;
    [SerializeField] private LayerMask ground;
    [SerializeField] private LayerMask whatIsObj;
    [SerializeField] private Transform pos;
    [SerializeField] private Vector2 size;
    public PlayableDirector firstLavaDirector;
    public PlayableDirector cusionJump;
    public Vector3 SavePos;
    AudioSource jumpSound;
    public AudioSource lavaBGM;

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

    private void Awake()
    {
        jumpSound = gameObject.GetComponent<AudioSource>();
        deathScreen = GameObject.Find("Death").GetComponent<SpriteRenderer>();
        gotobad = FindObjectOfType<CusionTutorial>();
        colly = GetComponent<BoxCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
        kidAni = GameObject.Find("kidSprite").GetComponent<Animator>();
        KidRenderer = GameObject.Find("kidSprite").GetComponent<SpriteRenderer>();
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
            kidAni.SetBool("Hoit", false);
        }
        if (collision.contacts[0].normal.y > 0.7f && collision.gameObject.CompareTag("Tram"))
        {
            rigid.AddForce(Vector2.up * 1.5f * jump, ForceMode2D.Impulse);
            jumpSound.Play();
        }
        if(collision.gameObject.CompareTag("TestTimeLine"))
        {
            firstLavaDirector.Play();
            lavaBGM.Stop();
            GameManager.isAction = true;
            gameObject.GetComponent<Collider2D>().enabled = false;
            gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;

        }
        if (collision.gameObject.CompareTag("SecondTimeLine"))
        {
            print("타임라인발동");
            lavaBGM.Stop();

            cusionJump.Play();
            GameManager.isAction = true;
            GameObject.Find("inttaget").GetComponent<Stage1Inter>().enabled = false;
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
        if (coll != null)
            gotobad.cusionUpTransform = coll.gameObject.transform;
        //if (!Gotobad.isCatch)
        //{
        //    print("이게 왜 안되냐고");
        //    gotobad.cusionUpTransform = null;
        //}

        if(!(GameManager.isAction))
            kidAni.SetBool("Hoit", !(isGround));


        if (isDead)
        {
            kidAni.SetBool("Die", true);
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
            Debug.Log("됨");
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


        if (GameManager.stopAni == 1)
        {
            kidAni.SetBool("Stop", false);
            if (moveSpeed == 0)
            {
                moveSpeed = 3;
            }

            if(GameManager.isAction == false)
            {
                //Animation
                if (Input.GetButtonDown("Jump") && isGround)
                {
                    rigid.velocity = Vector2.zero;
                    rigid.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
                }
                else
                {
                    kidAni.SetBool("Walk", moveDir.magnitude > 0);

                    kidAni.SetFloat("vAxisRaw", v);
                }
            }
        }
        else
        {
            kidAni.SetBool("Stop", GameManager.stopAni == 2);
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

                KidRenderer.flipX = value;
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
        transform.position = SavePos;
        kidAni.SetBool("Die", false);
        GameManager.isAction = false;
        deathScreen.DOFade(0, 1);
    }

}

