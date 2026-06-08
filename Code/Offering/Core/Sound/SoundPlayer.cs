using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour, IPoolable
{
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioSource audioSource;

    public GameObject GameObject => gameObject;
    [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
    private Pool _myPool;

    public async void PlaySound(SoundSO clipData)
    {
        if (clipData.audioType == SoundSO.AudioTypes.SFX)
        {
            audioSource.outputAudioMixerGroup = sfxGroup;
        }
        else if (clipData.audioType == SoundSO.AudioTypes.Music)
        {
            audioSource.outputAudioMixerGroup = musicGroup;
        }

        audioSource.volume = clipData.volume;
        audioSource.pitch = clipData.pitch;

        if (clipData.randomizePitch)
        {
            audioSource.pitch += Random.Range(-clipData.randomPitchModifier, clipData.randomPitchModifier);
        }

        audioSource.clip = clipData.clip;
        audioSource.loop = clipData.loop;

        audioSource.Play();
        if (clipData.loop == false)
        {
            await UniTask.WaitForSeconds(clipData.clip.length + 0.2f);
            _myPool.Push(this);
        }
    }

    public void StopAndReturnToPool()
    {
        audioSource.Stop();
        _myPool.Push(this);
    }

    public void SetUpPool(Pool pool) => _myPool = pool;

    public void ResetItem()
    {
        //do nothing
    }
}
