using Ami.BroAudio;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainmenuLogic : MonoBehaviour
{
    private LensDistortion lensDistortion;
    public PlayableDirector timeline;
    public UnityEngine.Rendering.Volume volume;
    public SineMover[] SineMover;
    public Camera Camera;
    private bool isRotate;
    public Image white;
    public Image black;

    public SoundID uiSound;
    public SoundID inToSound;

    [SerializeField] private Button[] activeButton;

    void Start()
    {
        if (!volume.profile.TryGet(out lensDistortion))
        {
            Debug.LogError("Lens Distortion not found in Volume Profile!");
        }
    }

    public void CanceledButton(bool value)
    {
        foreach(Button button in activeButton)
        {
            button.enabled = value;
        }
    }

    public void AnimateDistortion(float toValue, float duration)
    {
        if (lensDistortion == null) return;

        DOTween.To(
            () => lensDistortion.intensity.value,
            x => lensDistortion.intensity.value = x,
            toValue,
            duration
        ).SetEase(Ease.InOutSine);
        
    }

    public void GameStart()
    {
        CanceledButton(false);
        BroAudio.Play(uiSound);
        AnimateDistortion(-0.85f, 2f);
        for (int i =0;i<SineMover.Length;i++)
        {
            SineMover[i].enabled = false;
        }
        GetComponentInChildren<CanvasGroup>().DOFade(0f,0.2f);
        timeline.Play();
    }

    public void InToVoid()
    {
        BroAudio.Play(inToSound);
        DOTween.To(() => Camera.orthographicSize, x => {
            Camera.orthographicSize = x;
        }, 0.56f, 0.5f).OnComplete(()=> { 
        });
        AnimateDistortion(-1f, 1f);
        white.DOFade(1f,0.6f);
        DOVirtual.DelayedCall(5f,()=> { black.DOFade(1f, 1f).OnComplete(()=>SceneManager.LoadScene("GameScene")); });
        isRotate = true;
    }

    private void Update()
    {
        if(isRotate)
        Camera.transform.parent.Rotate(0f,0f, 360f*Time.deltaTime);
    }

    public void StartTutorial()
    {
        CanceledButton(false);

        BroAudio.Play(uiSound);

        FindAnyObjectByType<TutorialLogic>().TutorialStart();
    }

    public void QuitGame()
    {
        BroAudio.Play(uiSound);

#if UNITY_EDITOR
        // 에디터 모드에서 실행 중이면 Play 모드를 끔
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서는 애플리케이션 종료
        Application.Quit();
#endif
    }
}
