using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controller인 친구들의 물리 관련 정보를 가진다. 
 */
public interface IControllerPhysics 
{
    public bool isCollisionStay { get; set; }
    public Transform trm { get; }
    public float moveSpeed { get; set; }
    public float jump { get; set; }
    public float h { get; set; }
    public bool isGround { get; set; }
}


public class FriendController : SpriteSystem, IControllerPhysics
{
    Gotobad gotobad;
    Rigidbody2D rigid;
    SpriteRenderer friendRenderer;
    public GameManager manager;
    bool isHorizonMove;
    Vector3 dirVec;
    GameObject scanObject;
    public bool isLookUp;
    Vector2 footPosition;
    public float v;
    Vector2 moveDir;
    Animator friendAni;
    BoxCollider2D colly;
    [SerializeField] private LayerMask ground;
    [SerializeField] private LayerMask whatIsObj;
    [SerializeField] private Transform pos;
    [SerializeField] private Vector2 size;
    bool currentFlip = false;
    public bool isoneplayer = false;

    private bool isDead = false;

    public bool isCollisionStay { get; set; } = false;
    [field:SerializeField] public float moveSpeed { get; set; }
    [field: SerializeField] public float jump { get; set; }
    [field:SerializeField] public float h { get; set; }
    public Transform trm => transform;
    [field: SerializeField] public bool isGround { get; set; }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        colly = GetComponent<BoxCollider2D>();
        gotobad = FindObjectOfType<Gotobad>();

        friendAni = GameObject.Find("FriendSprite").GetComponent<Animator>();
        friendRenderer = GameObject.Find("FriendSprite").GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
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
            if (collision.contacts[0].normal.y > 0.7f && collision.gameObject.CompareTag("Tram"))
            {
                rigid.AddForce(Vector2.up * 1.5f * jump, ForceMode2D.Impulse);
            }
    }
    void Update()
    {
        Bounds bounds = colly.bounds;
        footPosition = new Vector2(bounds.center.x, bounds.min.y);
        isGround = Physics2D.OverlapCircle(footPosition, 0.1f, ground);
        //Collider2D coll = Physics2D.OverlapCircle(footPosition, 1f, whatIsObj);
        //if (coll != null)
        //{
        //    gotobad.cusionUpTransform = coll.gameObject.transform;
        //}
        //if (!Gotobad.isCatch) gotobad.cusionUpTransform = null;
        friendAni.SetBool("Hoit", !(isGround));


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
            friendAni.SetBool("Walk", false);

            gameObject.GetComponent<FriendController>().enabled = false;
            GameObject.Find("Player").GetComponent<PlayerController>().enabled = true;
        }

        if (GameManager.stopAni == 2)
            {
                friendAni.SetBool("Stop", false);
                if (moveSpeed == 0)
                {
                    moveSpeed = 3;
                }

                //Animation
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
            else
            {
                friendAni.SetBool("Stop", GameManager.stopAni == 1);
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


}
