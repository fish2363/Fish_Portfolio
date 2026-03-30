using Ami.BroAudio;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneBtns : MonoBehaviour
{
    string loadingSceneName = "LoadingScene";

    public void NewGameBtn()
    {
        SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
    }

    public void RankBtn()
    {
        BroAudio.Play(SoundIDs.Instance.Click);
        UIManager.Show<UIPopupRanking>();
    }

    public void SettingBtn()
    {
        BroAudio.Play(SoundIDs.Instance.Click);
        UIManager.Show<UIPopupSetting>();
    }

    public void ExitBtn()
    {
        BroAudio.Play(SoundIDs.Instance.Click);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
    }
}
