using UnityEngine;

// Based on https://www.youtube.com/watch?v=GqHFGMy_51c
[RequireComponent (typeof (AudioSource))]
public class Audio : MonoBehaviour {

    public float frequency = 440;
    public float gain = .2f;
    float phase;
    float sampleRate;

    void Awake () {
        sampleRate = AudioSettings.outputSampleRate;
    }

    void OnAudioFilterRead(float[] data, int numChannels) {
        float increment = frequency * Mathf.PI * 2 / sampleRate;

        for (int i = 0; i < data.Length; i += numChannels) {
            phase += increment;
            data[i] = gain * Mathf.Sin(phase);
            if (numChannels == 2) {
                data[i+1] = data[i];
            }
        }
        phase %= Mathf.PI * 2;
    }
}


