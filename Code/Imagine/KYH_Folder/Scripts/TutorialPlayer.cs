using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPlayer : SpriteSystem, IControllerPhysics
{
    Gotobad gotobad;
    Rigidbody2D rigid;
    SpriteRenderer friendRenderer;
    public GameManager manager;
    CusionTutorial cusion;
    bool isHorizonMove;
    GameObject intta;
    Vector3 dirVec;
    GameObject scanObject;
    GameObject deskObject;
    public bool isLookUp;
    Vector2 footPosition;
    public float v;
    Vector2 moveDir;
    Animator friendAni;
    bool end;
    GameObject playerCam;
    GameObject gameCam;
    BoxCollider2D colly;
    [SerializeField] private LayerMask ground;
    [SerializeField] private LayerMask whatIsObj;
    [SerializeField] private LayerMask desk;
    [SerializeField] private Transform pos;
    [SerializeField] private Vector2 size;
    bool currentFlip = false;
    public SpriteRenderer Panel;
    float time = 0f;
    float F_time = 1f;
    GameObject backGround;
    bool start;
    GameObject furnitures;


    private bool isDead = false;

    public bool isCollisionStay { get; set; } = false;
    [field: SerializeField] public float moveSpeed { get; set; }
    [field: SerializeField] public float jump { get; set; }
    [field: SerializeField] public float h { get; set; }
    public Transform trm => transform;
    [field: SerializeField] public bool isGround { get; set; }

    private void Awake()
    {
        furnitures = GameObject.Find("furniture");
        deskObject = GameObject.Find("Desk");
        backGround = GameObject.Find("BackGround");
        playerCam = GameObject.Find("PlayerCam");
        gameCam = GameObject.Find("GameCamera");
        intta = GameObject.Find("inttaget");
        rigid = GetComponent<Rigidbody2D>();
        colly = GetComponent<BoxCollider2D>();
        gotobad = FindObjectOfType<Gotobad>();
        cusion = GameObject.Find("cusion").GetComponent<CusionTutorial>();
        friendAni = GameObject.Find("LookUp").GetComponent<Animator>();
        friendRenderer = GameObject.Find("LookUp").GetComponent<SpriteRenderer>();
        gameCam.SetActive(false);
    }
    private void Start()
    {
        //deskCam.SetActive(false);
        isGround = false;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Magema"))
        {
            isDead = true;
        }
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("cusion") || collision.gameObject.CompareTag("Player") && collision.contacts[0].normal.y > 0.7f)
        {
            isGround = true;
            friendAni.SetBool("Hoit", false);
        }
    }

    void Update()
    {
        Bounds bounds = colly.bounds;
        footPosition = new Vector2(bounds.center.x, bounds.min.y);
        isGround = Physics2D.OverlapCircle(footPosition, 0.1f, ground);
        friendAni.SetBool("Hoit", !(isGround));

        if(!(start))
            end = Physics2D.OverlapCircle(footPosition, 0.1f, desk);

        if (end)
        {
            GameManager.isAction = true;
            isGround = true;
            friendAni.SetBool("Hoit", false);
            cusion.FallCusion();
            intta.SetActive(false);
           // deskCam.SetActive(true);
            playerCam.SetActive(false);
            //StartCoroutine(StartShot());
        }

        if (isDead)
        {
            Time.timeScale = 0;
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
        //if (h == -1)
        //{
        //    if (currentFlip != true)
        //    {
        //        currentFlip = true;
        //        dirVec = Vector3.left; //레이캐스트를 위한 벡터 방향 지정
        //        Flip(currentFlip);
        //    }
        //}
        //else if (h == 1)
        //{
        //    if (currentFlip != false)
        //    {
        //        currentFlip = false;
        //        dirVec = Vector3.right;
        //        Flip(currentFlip);
        //    }
        //}
        if (Input.GetButtonDown("Jump") && isGround)
        {
            rigid.velocity = Vector2.zero;
            rigid.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
        }
        else
        {
            friendAni.SetBool("Walk", moveDir.magnitude > 0);

            friendAni.SetFloat("vAxisRaw", v);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(footPosition, 0.1f);
    }

    private void Flip(bool value)
    {
        friendRenderer.flipX = value;
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

    //public IEnumerator StartShot()
    //{
    //    end = false;
    //    yield return new WaitForSecondsRealtime(2);

    //        Panel.gameObject.SetActive(true);
    //        Color alpha = Panel.color;
    //        while (alpha.a > 0f)
    //        {
    //            time += Time.deltaTime / F_time;
    //            alpha.a = Mathf.Lerp(1, 0, time);
    //            Panel.color = alpha;
    //            yield return null;
    //        }
    //        yield return null;
    //        time = 0f;
    //    deskCam.SetActive(false);
    //    backGround.SetActive(false);
    //    gameCam.SetActive(true);
    //    GameManager.isAction = false;
    //    start = true;
    //    deskObject.layer = 7;
    //    furnitures.SetActive(true);
    //}
}
