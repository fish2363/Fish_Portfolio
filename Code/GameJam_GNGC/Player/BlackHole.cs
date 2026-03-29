using UnityEngine;
using System;
using TMPro;
using Ami.BroAudio;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class BlackHole : MonoBehaviour
{
    public TetrisCompo tetrisCompo;

    [SerializeField] private Camera currentCamera;
    [SerializeField] private InputReader InputReader;
    [SerializeField] private Rigidbody2D _rigid;
    [field: SerializeField] public float MoveSpeed { get; set; }

    [SerializeField] private TextMeshProUGUI levelText;

    [Header("레벨 별 설정값")]
    public Level[] levelSetting;
    public int Level { get; set; } = 1;

    [Header("현재 지름")]
    public float diameter;
    private CircleCollider2D col;

    [Header("오디오")]
    [SerializeField] private SoundID levelUp;

    [Header("스턴 시간")]
    public float stunDuration;
    private float duri;
    private bool isDamage;

    [Header("깜빡임 UI 설정")]
    [SerializeField] private Image targetImage;
    [SerializeField] private float blinkDuration = 2f;
    [SerializeField] private float blinkInterval = 0.7f;

    private Tween blinkTween;

    public void StartBlink()
    {
        blinkTween?.Kill();

        // 무조건 시작 시 투명하게 설정
        Color c = targetImage.color;
        c.a = 0f;
        targetImage.color = c;

        // 깜빡임 트윈 (0 ↔ 1 반복)
        blinkTween = targetImage.DOFade(1f, blinkInterval)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetTarget(targetImage);

        // 3초 후 종료 및 알파 0 고정
        DOVirtual.DelayedCall(blinkDuration, () =>
        {
            blinkTween?.Kill();
            targetImage.DOFade(0f, 0.1f);
        });
    }

    private void Start()
    {
        col = GetComponent<CircleCollider2D>();
        tetrisCompo = FindAnyObjectByType<TetrisCompo>();
        SetPlayerSetting(levelSetting[0].levelSpeed, levelSetting[0].levelSize);
    }

    public void DamagePlayer()
    {
        if (isDamage) return;
        _rigid.linearVelocity = Vector2.zero;
        tetrisCompo.OnLineDestroyEvent?.Invoke();
        StartBlink();
        isDamage = true;
    }

    public void PlusLevel()
    {
        Level++;
        if (Level == 20) SceneManager.LoadScene("Clear");
        BroAudio.Play(levelUp);
        FindAnyObjectByType<MeteoSpawner>().SpawnWhiteTetBoomSpawn();
        tetrisCompo.DecreaseWallSpawnSpeed(tetrisCompo.levelDecreaseWallSpawnSpeed);

        if (Level > levelSetting.Length) return;

        Debug.Log($"{Level}레벨");
        SetPlayerSetting(levelSetting[Level - 1].levelSpeed, levelSetting[Level - 1].levelSize);
    }

    public void SetPlayerSetting(float speed, float size)
    {
        levelText.text = $"<b>{Level}</b>";
        MoveSpeed = speed;
        transform.localScale = new Vector3(size, size, transform.localScale.z);
        currentCamera.fieldOfView = levelSetting[Level - 1].cameraSize;

        float radius = col.radius;
        float scale = transform.lossyScale.x;
        diameter = radius * 2f * scale;

        Debug.Log($"내 지름은 {diameter:F2}");
    }

    private void FixedUpdate()
    {
        if (isDamage)
        {
            duri += Time.deltaTime;
            if (duri >= stunDuration)
            {
                isDamage = false;
                duri = 0f;
            }
        }
        else
        {
            Vector2 moveDir = InputReader.HoleInputDirection;
            _rigid.linearVelocity = moveDir * MoveSpeed;
        }
    }
}

[Serializable]
public struct Level
{
    public float levelSize;
    public float levelSpeed;
    public float cameraSize;
}
