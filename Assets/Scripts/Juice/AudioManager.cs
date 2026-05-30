// AudioManager.cs
// Reproduce SFX con pitch/volumen aleatorios y un pool de AudioSources para evitar fatiga auditiva.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game.Juice
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [System.Serializable]
        public class SoundEntry
        {
            public string id;
            public AudioClip[] variants;                 // tone.wav, tone(1).wav, ...
            [Range(0f, 1f)]   public float baseVolume = 1f;
            [Range(0.5f, 2f)] public float basePitch = 1f;
            [Range(0f, 0.3f)] public float pitchVariance = 0.1f;
            [Range(0f, 0.3f)] public float volumeVariance = 0.05f;
            public bool preventRepeat = true;
        }

        [SerializeField] private SoundEntry[] soundLibrary;
        [SerializeField] private int poolSize = 8;

        private Dictionary<string, SoundEntry> _soundMap;
        private Dictionary<string, int> _lastClipIndex;
        private List<AudioSource> _pool;
        private int _poolCursor;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildMap();
            BuildPool();
        }

        private void BuildMap()
        {
            _soundMap = new Dictionary<string, SoundEntry>(soundLibrary.Length);
            _lastClipIndex = new Dictionary<string, int>(soundLibrary.Length);
            foreach (var entry in soundLibrary)
            {
                if (string.IsNullOrEmpty(entry.id)) continue;
                _soundMap[entry.id] = entry;
                _lastClipIndex[entry.id] = -1;
            }
        }

        private void BuildPool()
        {
            _pool = new List<AudioSource>(poolSize);
            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject($"AudioSource_{i}");
                go.transform.SetParent(transform);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                _pool.Add(src);
            }
        }

        // ── API pública ─────────────────────────────────────────────────────
        public void Play(string id)
        {
            if (!_soundMap.TryGetValue(id, out SoundEntry entry) || entry.variants.Length == 0)
            {
                Debug.LogWarning($"[AudioManager] Sound '{id}' no encontrado o sin variantes.");
                return;
            }

            AudioClip clip = PickClip(entry, id);
            AudioSource source = GetSource();

            source.clip   = clip;
            source.volume = Mathf.Clamp01(entry.baseVolume + Random.Range(-entry.volumeVariance, entry.volumeVariance));
            source.pitch  = entry.basePitch + Random.Range(-entry.pitchVariance, entry.pitchVariance);
            source.Play();
        }

        public void PlayAtPosition(string id, Vector3 worldPosition)
        {
            if (!_soundMap.TryGetValue(id, out SoundEntry entry) || entry.variants.Length == 0) return;
            AudioClip clip = PickClip(entry, id);
            float volume = Mathf.Clamp01(entry.baseVolume + Random.Range(-entry.volumeVariance, entry.volumeVariance));
            AudioSource.PlayClipAtPoint(clip, worldPosition, volume);
        }

        // ── Internos ────────────────────────────────────────────────────────
        private AudioClip PickClip(SoundEntry entry, string id)
        {
            if (entry.variants.Length == 1) return entry.variants[0];

            int index;
            if (entry.preventRepeat)
            {
                do { index = Random.Range(0, entry.variants.Length); }
                while (index == _lastClipIndex[id]);
            }
            else
            {
                index = Random.Range(0, entry.variants.Length);
            }
            _lastClipIndex[id] = index;
            return entry.variants[index];
        }

        private AudioSource GetSource()
        {
            // Round-robin: prioriza fuentes libres, si no reutiliza la más antigua.
            for (int i = 0; i < _pool.Count; i++)
            {
                var src = _pool[_poolCursor];
                _poolCursor = (_poolCursor + 1) % _pool.Count;
                if (!src.isPlaying) return src;
            }
            var oldest = _pool[_poolCursor];
            _poolCursor = (_poolCursor + 1) % _pool.Count;
            return oldest;
        }
    }
}
