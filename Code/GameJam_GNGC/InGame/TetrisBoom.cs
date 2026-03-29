using Ami.BroAudio;
using DG.Tweening;
using UnityEngine;

public class TetrisBoom : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer visual;

    [SerializeField] private float visualRotateSpeed = 90f;
    [SerializeField] private float checkRadius = 0.3f;
    [SerializeField] private LayerMask placeLayer;
    [SerializeField] private Transform effect;
    public SoundID placeMeteoBurst;

    private bool isOneTIme;

    private void Update()
    {
        // 회전
        visual.transform.Rotate(0f, 0f, visualRotateSpeed * Time.deltaTime);

        // 아래로 이동
        transform.position += Vector3.down * (Time.deltaTime / 2f);

        // 오버랩 체크
        if (Physics2D.OverlapCircle(transform.position, checkRadius, placeLayer) && !isOneTIme)
        {
            BroAudio.Play(placeMeteoBurst);
            effect.localScale = new Vector3(effect.localScale.x, 0f, effect.localScale.z);
            effect.DOScaleY(3f, 0.3f)
                  .OnComplete(() =>
                  {
                      effect.DOScaleY(0.1f, 0.3f).OnComplete(()=> {
                            isOneTIme = true;
                          Destroy(effect.gameObject);
                          FindAnyObjectByType<TetrisCompo>().LineBlueBoom();
                          visual.gameObject.SetActive(false);
                          GetComponent<Animator>().enabled = true;
                      });
                  });
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}