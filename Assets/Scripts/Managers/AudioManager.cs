using UnityEngine;

namespace aerisOS.Managers
{
    /// <summary>
    /// Plays UI feedback sounds. Clips are synthesized at runtime — no .wav files
    /// are shipped with the project, so the build is self-contained.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _source;
        private AudioSource _droneSource;
        private AudioClip _click;
        private AudioClip _success;
        private AudioClip _notify;
        private AudioClip _typing;
        private AudioClip _drone;

        [Range(0f, 1f)] public float MasterVolume = 0.6f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _click   = SynthesizeBeep(800f, 0.06f, 0.5f);
            _success = SynthesizeSweep(600f, 1200f, 0.25f, 0.5f);
            _notify  = SynthesizeDoubleBeep(1000f, 0.07f, 0.05f, 0.5f);
            _typing  = SynthesizeBeep(1200f, 0.025f, 0.20f);
            _drone   = SynthesizeDrone(55f, 4f, 0.45f);

            _droneSource = gameObject.AddComponent<AudioSource>();
            _droneSource.playOnAwake = false;
            _droneSource.loop = true;
            _droneSource.clip = _drone;
            _droneSource.volume = 0f;
        }

        public void PlayClick()   => Play(_click);
        public void PlaySuccess() => Play(_success);
        public void PlayNotify()  => Play(_notify);
        public void PlayTyping()  => Play(_typing);

        public void PlayDrone()
        {
            if (_droneSource == null || _drone == null) return;
            _droneSource.volume = MasterVolume * 0.7f;
            if (!_droneSource.isPlaying) _droneSource.Play();
        }

        public void StopDrone()
        {
            if (_droneSource == null) return;
            _droneSource.Stop();
        }

        private void Play(AudioClip clip)
        {
            if (clip == null || _source == null) return;
            _source.PlayOneShot(clip, MasterVolume);
        }

        // --- synthesis ---

        private const int SampleRate = 44100;

        private AudioClip SynthesizeBeep(float freq, float duration, float amplitude)
        {
            int samples = Mathf.RoundToInt(SampleRate * duration);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float env = FastEnvelope(i, samples);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * amplitude * env;
            }
            var clip = AudioClip.Create("beep", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip SynthesizeSweep(float fromHz, float toHz, float duration, float amplitude)
        {
            int samples = Mathf.RoundToInt(SampleRate * duration);
            var data = new float[samples];
            float phase = 0f;
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(fromHz, toHz, t);
                phase += 2f * Mathf.PI * freq / SampleRate;
                float env = FastEnvelope(i, samples);
                data[i] = Mathf.Sin(phase) * amplitude * env;
            }
            var clip = AudioClip.Create("sweep", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip SynthesizeDoubleBeep(float freq, float beepDur, float gapDur, float amplitude)
        {
            int beepSamples = Mathf.RoundToInt(SampleRate * beepDur);
            int gapSamples  = Mathf.RoundToInt(SampleRate * gapDur);
            int total = beepSamples * 2 + gapSamples;
            var data = new float[total];
            for (int i = 0; i < beepSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = FastEnvelope(i, beepSamples);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * amplitude * env;
            }
            int offset = beepSamples + gapSamples;
            for (int i = 0; i < beepSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = FastEnvelope(i, beepSamples);
                data[offset + i] = Mathf.Sin(2f * Mathf.PI * freq * 1.25f * t) * amplitude * env;
            }
            var clip = AudioClip.Create("notify", total, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Дрон: низкочастотный гул с модуляцией (для концовки 2)
        private AudioClip SynthesizeDrone(float baseFreq, float duration, float amplitude)
        {
            int samples = Mathf.RoundToInt(SampleRate * duration);
            var data = new float[samples];
            float phase = 0f;
            float lfoPhase = 0f;
            float phase2 = 0f;
            var rng = new System.Random(7);
            for (int i = 0; i < samples; i++)
            {
                // LFO ~0.25Hz — медленная пульсация громкости
                lfoPhase += 2f * Mathf.PI * 0.25f / SampleRate;
                float lfo = 0.55f + 0.45f * Mathf.Sin(lfoPhase);

                // Основная частота с дрейфом
                float freqDrift = baseFreq + Mathf.Sin((float)i / SampleRate * 0.6f) * 4f;
                phase  += 2f * Mathf.PI * freqDrift / SampleRate;
                phase2 += 2f * Mathf.PI * (freqDrift * 1.98f) / SampleRate; // чуть расстроенная октава

                float s = Mathf.Sin(phase) * 0.5f
                        + Mathf.Sin(phase * 3f) * 0.2f  // третья гармоника
                        + Mathf.Sin(phase2) * 0.18f     // расстроенная октава
                        + (float)(rng.NextDouble() * 2 - 1) * 0.04f; // шум

                data[i] = Mathf.Clamp(s * amplitude * lfo, -1f, 1f);
            }
            var clip = AudioClip.Create("drone", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Soft attack/decay envelope so beeps don't click.
        private static float FastEnvelope(int i, int total)
        {
            int attack = Mathf.Min(400, total / 4);
            int release = Mathf.Min(800, total / 3);
            if (i < attack) return (float)i / attack;
            if (i > total - release) return (float)(total - i) / release;
            return 1f;
        }
    }
}
