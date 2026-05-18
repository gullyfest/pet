using System.Collections;
using UnityEngine;

namespace aerisOS.Managers
{
    public class MusicPlayer : MonoBehaviour
    {
        public static MusicPlayer Instance { get; private set; }

        [Range(0f, 1f)] public float Volume = 0.28f;

        public int TrackCount => _clips != null ? _clips.Length : TrackMeta.Length;
        public int CurrentIndex { get; private set; }
        public bool IsPlaying => _source != null && _source.isPlaying;
        public float Progress => (_source == null || _source.clip == null) ? 0f
            : (float)_source.timeSamples / _source.clip.samples;

        public static readonly (string Title, string Artist)[] TrackMeta =
        {
            ("Aqua Dreams",       "Terra"),
            ("LEASE",             "Takeshi Abo"),
            ("Frutiger aero",     "temcandoanything"),
            ("FRESH AIR",         "ETHERNET SKY"),
            ("Who Is Using This", "CoolMan"),
            ("Menu Settings",     "A-Dog"),
        };

        private AudioSource _source;
        private AudioClip[] _clips;
        private bool _pendingPlay;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _source = gameObject.AddComponent<AudioSource>();
            _source.loop = true;
            _source.playOnAwake = false;
            _source.volume = Volume;
            StartCoroutine(LoadAll());
        }

        private IEnumerator LoadAll()
        {
            // Track 0: synthesized Aqua Dreams (always available immediately)
            var synth = Track1_AquaDreams();

            // Tracks 1-5: loaded from Resources/Music
            string[] files =
            {
                "Music/Takeshi-Abo-Lease",
                "Music/temcandoanything_-_frutiger_aero__SkySound.cc_",
                "Music/ETHERNET_SKY-FRESH_AIR",
                "Music/CoolMan-WhoIsUsingThisComputer_",
                "Music/A-Dog-Menu-Settings",
            };

            _clips = new AudioClip[files.Length + 1];
            _clips[0] = synth;

            for (int i = 0; i < files.Length; i++)
            {
                var req = Resources.LoadAsync<AudioClip>(files[i]);
                yield return req;
                _clips[i + 1] = req.asset as AudioClip;
            }

            // Загружаем трек 0 без воспроизведения — старт по RequestPlay()
            CurrentIndex = 0;
            _source.clip = _clips[0];
            _source.time = 0f;
            if (_pendingPlay) { _pendingPlay = false; _source.Play(); }
        }

        // Запросить воспроизведение: стартует сразу если клипы загружены, иначе — после загрузки
        public void RequestPlay()
        {
            if (_clips != null) { if (_source && !_source.isPlaying) _source.Play(); }
            else _pendingPlay = true;
        }

        private void Update() { if (_source) _source.volume = Volume; }
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Play()  { if (_source && !_source.isPlaying) _source.Play(); }
        public void Pause() { if (_source) { if (_source.isPlaying) _source.Pause(); else _source.UnPause(); } }
        public void Stop()  { if (_source) { _source.Stop(); _source.time = 0f; } }
        public void Next()  { if (_clips != null) LoadAndPlay((CurrentIndex + 1) % _clips.Length); }
        public void Prev()  { if (_clips != null) LoadAndPlay((CurrentIndex - 1 + _clips.Length) % _clips.Length); }
        public void SetTrack(int i) { if (_clips != null && i >= 0 && i < _clips.Length) LoadAndPlay(i); }

        private void LoadAndPlay(int i)
        {
            if (_clips == null || _clips[i] == null) return;
            CurrentIndex = i;
            _source.clip = _clips[i];
            _source.time = 0f;
            _source.Play();
        }

        // ── Synthesis constants ────────────────────────────────────────────
        private const int Sr = 44100;

        // Track 0 — Aqua Dreams (synthesized, always available as fallback)
        private static AudioClip Track1_AquaDreams()
        {
            float[] arp   = { 293.66f, 369.99f, 440f, 493.88f, 587.33f, 493.88f, 440f, 369.99f };
            float[] pad   = { 73.42f, 110f, 146.83f };
            float noteDur = 0.375f;
            return BuildBellArp("AquaDreams", arp, pad, noteDur, 4,
                padAmp: 0.055f, bellAmp: 0.38f, bellDecay: 0.30f);
        }

        private static AudioClip BuildBellArp(string name, float[] arp, float[] pad,
            float noteDur, int cycles, float padAmp, float bellAmp, float bellDecay)
        {
            int noteLen  = Mathf.RoundToInt(Sr * noteDur);
            int cycleLen = noteLen * arp.Length;
            int total    = cycleLen * cycles;
            float[] data = new float[total];
            for (int i = 0; i < total; i++)
            {
                float t   = (float)i / Sr;
                float env = Mathf.Clamp01(t / 1.2f) * Mathf.Clamp01((total - i) / (float)(Sr * 0.8f));
                foreach (float f in pad) data[i] += TriWave(f, t) * padAmp * env;
            }
            for (int cy = 0; cy < cycles; cy++)
                for (int n = 0; n < arp.Length; n++)
                {
                    int start = cy * cycleLen + n * noteLen;
                    AddBell(data, start, arp[n], bellAmp, bellDecay, total, Sr);
                    AddBell(data, start, arp[n] * 2f, bellAmp * 0.18f, bellDecay * 0.6f, total, Sr);
                    AddBell(data, start + Mathf.RoundToInt(Sr * 0.11f), arp[n], bellAmp * 0.26f, bellDecay * 0.75f, total, Sr);
                    AddBell(data, start + Mathf.RoundToInt(Sr * 0.23f), arp[n], bellAmp * 0.09f, bellDecay * 0.5f, total, Sr);
                }
            Normalize(data, 0.88f);
            var clip = AudioClip.Create(name, total, 1, Sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static void AddBell(float[] buf, int start, float freq, float amp,
            float decay, int bufLen, int sr)
        {
            int len = Mathf.Min(bufLen - Mathf.Max(0, start), Mathf.RoundToInt(sr * decay * 4f));
            for (int i = 0; i < len; i++)
            {
                int idx = start + i;
                if (idx < 0 || idx >= bufLen) continue;
                float t   = (float)i / sr;
                float env = Mathf.Exp(-t / decay) * Mathf.Clamp01(t / 0.008f);
                float s   = Mathf.Sin(2f * Mathf.PI * freq * t)
                          + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.28f
                          + Mathf.Sin(2f * Mathf.PI * freq * 3f * t) * 0.07f;
                buf[idx] += s * amp * env;
            }
        }

        private static float TriWave(float freq, float t)
        {
            float p = Mathf.Repeat(t * freq, 1f);
            return p < 0.5f ? (4f * p - 1f) : (3f - 4f * p);
        }

        private static void Normalize(float[] data, float ceiling)
        {
            float peak = 0;
            foreach (float v in data) { float a = Mathf.Abs(v); if (a > peak) peak = a; }
            if (peak > ceiling) { float s = ceiling / peak; for (int i = 0; i < data.Length; i++) data[i] *= s; }
        }
    }
}
