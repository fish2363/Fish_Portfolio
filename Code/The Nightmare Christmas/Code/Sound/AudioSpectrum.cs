using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSpectrum : MonoBehaviour
{
    [SerializeField] private AudioSource micInput;
    private static float[] _audioBand = new float[512];
    private static float[] _freqBand = new float[8];
    public static float[] FreqBand => _freqBand;

    private void Update()
    {
        GetAmplitude();
        MakeFrequencyBands();
    }

    private void GetAmplitude()
    {
        micInput.GetSpectrumData(_audioBand, 0, FFTWindow.Blackman);
    }

    private void MakeFrequencyBands()
    {
        int count = 0;

        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;

            if (i == 7) sampleCount += 2;

            for (int j = 0; j < sampleCount; j++)
            {
                average += _audioBand[count] * (count + 1);
                count++;
            }
            average /= count;

            _freqBand[i] = average * 10;
        }
    }
}