using Ami.BroAudio;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class MeteoPlanet : MonoBehaviour
{
    private Rigidbody2D _rigid;
    [SerializeField]
    private Transform meteoSprite;
    public float meteoSpinSpeed=90f;
    [SerializeField]
    private float moveSpeed=10f;
    [Header("디스폰시간")]
    [SerializeField] private float deSpawnDuration;
    private float deSpawnDu;

    public bool isEnemy;

    public UnityEvent OnSpawnBoom;

    public SoundID meteoBurst;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        deSpawnDu += Time.deltaTime;
        if (deSpawnDuration <= deSpawnDu)
            Destroy(gameObject);
        _rigid.linearVelocity = new Vector2(-1, -1).normalized * moveSpeed;
        meteoSprite.Rotate(0f,0f,meteoSpinSpeed*Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            transform.DOMove(collision.gameObject.transform.position,1f);
            if (isEnemy)
            {
                BroAudio.Play(meteoBurst);
                meteoSprite.gameObject.SetActive(false);
                GetComponent<Animator>().Play("BadPlanetExplosion");
                collision.GetComponent<BlackHole>().DamagePlayer();
            }
            else
            {
                transform.DOScale(0f, 1f).OnComplete(() =>
                 {
                     collision.GetComponent<BlackHole>().tetrisCompo.BlueBoom();
                     DestroyObj();
                 });
            }
        }
    }
    public void DestroyObj()
    {
        Destroy(gameObject);
    }
}
