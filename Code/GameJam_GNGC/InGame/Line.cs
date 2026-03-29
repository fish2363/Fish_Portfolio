using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Ami.BroAudio;

public class Line : MonoBehaviour
{
    private Collider2D myCollider;
    private int tetrisLayer;

    private float detectTimer = 0f;
    private const float requiredDuration = 3f;
    private const float requiredCoverageRatio = 0.9f;

    private List<PlanetTetrisBlock> currentDetectedBlocks = new();
    private List<PlanetTetrisBlock> blinkingBlocks = new();

    private TetrisCompo tetrisCompo;

    private float currentCoverage = 0f;

    private SpriteRenderer wallet;
    [SerializeField] private Image prograssBar;

    private Tween sizeTween;
    private Tween colorTween;
    private Tween shakeTween;

    [Header("깜빡횟수")] private float redZoneCount=3f;
    [Header("깜빡빈도")] private float redZoneDuration=0.02f;

    private void Start()
    {
        tetrisCompo = FindObjectOfType<TetrisCompo>();
        myCollider = GetComponent<Collider2D>();
        wallet = GetComponentInChildren<Wallet>().GetComponent<SpriteRenderer>();

        if (myCollider == null)
        {
            Debug.LogError("Collider2D가 없습니다!");
            enabled = false;
            return;
        }

        tetrisLayer = LayerMask.NameToLayer("Tetris");
    }

    public void UnLockLine()
    {
        wallet.enabled = true;
        myCollider.isTrigger = false;
    }

    public void LockLine()
    {
        wallet.enabled = false;
        myCollider.isTrigger = true;
    }

    private void FixedUpdate()
    {
        Bounds lineBounds = myCollider.bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(lineBounds.center, new Vector2(lineBounds.size.x, lineBounds.size.y - 0.1f), 0f);

        currentDetectedBlocks.Clear();
        float totalWidthCovered = 0f;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            GameObject obj = hit.gameObject;
            if (obj.CompareTag("Tetris") || obj.CompareTag("Putris") || obj.layer == tetrisLayer)
            {
                var block = hit.GetComponent<PlanetTetrisBlock>();
                if (block != null)
                {
                    currentDetectedBlocks.Add(block);

                    Bounds blockBounds = hit.bounds;
                    float overlapMinX = Mathf.Max(lineBounds.min.x, blockBounds.min.x);
                    float overlapMaxX = Mathf.Min(lineBounds.max.x, blockBounds.max.x);
                    float overlapWidth = Mathf.Max(0f, overlapMaxX - overlapMinX);

                    totalWidthCovered += overlapWidth;
                }
            }
        }

        currentCoverage = totalWidthCovered / lineBounds.size.x;

        if (currentCoverage >= requiredCoverageRatio)
        {
            detectTimer += Time.fixedDeltaTime;

            foreach (var block in currentDetectedBlocks)
            {
                if (!blinkingBlocks.Contains(block))
                {
                    block.StartBlinking();
                    blinkingBlocks.Add(block);
                }
            }

            if (detectTimer >= requiredDuration)
            {
                foreach (PlanetTetrisBlock block in currentDetectedBlocks)
                {
                    block.StopBlinking();
                    block.isBoom = true;
                }

                blinkingBlocks.Clear();
                detectTimer = 0f;
                StartCoroutine(DestroyRoutine());
            }
        }
        else
        {
            detectTimer = 0f;
            foreach (var block in blinkingBlocks)
            {
                block?.StopBlinking();
            }
            blinkingBlocks.Clear();
        }

        UpdateProgressBar();
    }

    private IEnumerator DestroyRoutine(bool force = false)
    {
        tetrisCompo.DestroyTetris();
        yield return new WaitForSeconds(0.7f);
        Time.timeScale = 0f;
        for (int i=0;i<redZoneCount;i++)
        {
            GetComponent<SpriteRenderer>().enabled = true;
            yield return new WaitForSecondsRealtime(redZoneDuration);
            GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSecondsRealtime(redZoneDuration);
        }
        Time.timeScale = 1f;
        Instantiate(tetrisCompo.EffectPrefabs, transform);
        BroAudio.Play(tetrisCompo.lazerSound);
        tetrisCompo.LineDestroyEffect();
        if (force)
        {
            GameManager.Instance.Score(10);  // 강제 점수
        }
        else
        {
            GameManager.Instance.Score(tetrisCompo.boomScore); // 보통 500점
        }
        yield return new WaitForSecondsRealtime(0.5f);

        for (int i = currentDetectedBlocks.Count - 1; i >= 0; i--)
        {
            var block = currentDetectedBlocks[i];
            if (block.isBoom)
            {
                currentDetectedBlocks.RemoveAt(i);
                BroAudio.Play(tetrisCompo.destroyPlanetSound);
                block.GetComponent<Animator>().Play("Explosion");
            }
        }
        ResetProgressBar(); // 슬라이더 리셋
    }

    private Tween flashTween;

    private void UpdateProgressBar()
    {
        if (prograssBar == null) return;

        float ratio = Mathf.Clamp01(currentCoverage / requiredCoverageRatio);
        float maxWidth = 72f;
        float targetWidth = ratio * maxWidth;

        sizeTween?.Kill();
        colorTween?.Kill();

        sizeTween = prograssBar.rectTransform.DOSizeDelta(
            new Vector2(targetWidth, prograssBar.rectTransform.sizeDelta.y),
            0.35f
        ).SetEase(Ease.OutExpo);

        // 색상 정의
        Color startColor = new Color(0.7f, 1f, 0.7f);
        Color midColor = new Color(0.3f, 0.9f, 0.3f);
        Color endColor = new Color(0f, 0.6f, 0.2f);
        Color dangerColor = Color.red;

        Color targetColor;

        if (ratio >= 1f)
        {
            targetColor = dangerColor;

            // 🔴 점멸 효과 시작
            if (flashTween == null || !flashTween.IsActive() || !flashTween.IsPlaying())
            {
                flashTween?.Kill();
                flashTween = DOTween.Sequence()
                    .Append(prograssBar.DOFade(0.3f, 0.3f))
                    .Append(prograssBar.DOFade(1f, 0.3f))
                    .SetLoops(-1);
            }
        }
        else
        {
            // 🔁 점멸 효과 중지 및 불투명 복원
            if (flashTween != null && flashTween.IsActive())
            {
                flashTween.Kill();
                flashTween = null;
                prograssBar.color = new Color(prograssBar.color.r, prograssBar.color.g, prograssBar.color.b, 1f);
            }

            // 색상 보간
            if (ratio < 0.5f)
                targetColor = Color.Lerp(startColor, midColor, ratio * 2f);
            else
                targetColor = Color.Lerp(midColor, endColor, (ratio - 0.5f) * 2f);
        }

        colorTween = prograssBar.DOColor(targetColor, 0.35f).SetEase(Ease.OutCubic);
    }

    private void ResetProgressBar()
    {
        if (prograssBar == null) return;

        float duration = 0.4f;
        float minWidth = 0f;
        Color resetColor = new Color(0.7f, 1f, 0.7f);

        sizeTween?.Kill();
        colorTween?.Kill();
        shakeTween?.Kill();

        // 흔들림
        shakeTween = prograssBar.rectTransform.DOShakeAnchorPos(
            duration: 0.3f,
            strength: new Vector2(10f, 0f),
            vibrato: 10,
            randomness: 90,
            snapping: false,
            fadeOut: true
        );

        // 리셋 시퀀스
        Sequence resetSeq = DOTween.Sequence();
        resetSeq.Join(prograssBar.rectTransform.DOSizeDelta(
            new Vector2(minWidth, prograssBar.rectTransform.sizeDelta.y),
            duration
        ).SetEase(Ease.InOutSine));

        resetSeq.Join(prograssBar.DOColor(
            resetColor,
            duration
        ).SetEase(Ease.InOutSine));
    }
    public void ForceDestroy()
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(DestroyRoutine(force: true));
    }
    private void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = currentCoverage >= requiredCoverageRatio ? Color.green : Color.red;
            Gizmos.DrawWireCube(col.bounds.center, new Vector2(col.bounds.size.x, col.bounds.size.y - 0.1f));
        }
    }
}
