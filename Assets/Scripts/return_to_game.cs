using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class return_to_game : MonoBehaviour
{
    public Animator Trans;
    public AudioClip mainMenuMusic;
    public AudioClip overworldMusic;
    public AudioClip fightMusicA;
    public AudioClip fightMusicB;
    public AudioClip victoryClip;
    public AudioClip gunshotClip;
    public float musicVolume = 1f;
    public float victoryVolume = 1f;
    public float gunshotVolume = 1f;
    public float musicFadeDuration = 1f;
    public float victoryFadeDuration = 1f;

    private static MusicRuntime _musicRuntime;

    private void Awake()
    {
        EnsureMusicRuntime();
        ApplyMusicSettings();
    }

    private void OnEnable()
    {
        EnsureMusicRuntime();
        ApplyMusicSettings();
    }

    public void load_level(string level)
    {
        StartCoroutine(loading_level(level));
    }

    private IEnumerator loading_level(string level)
    {
        Trans.SetTrigger("Start");
        Debug.Log($"Loading back to {level}");
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(level);
        yield return null;
    }

    public void Quit()
    {
        Debug.Log("Thank you so much for-to playing my game!!");
        Application.Quit();
    }

    public static void PlayVictoryMusic()
    {
        if (_musicRuntime == null)
        {
            return;
        }

        _musicRuntime.PlayVictory();
    }

    public static void PlayGunshotSfx()
    {
        if (_musicRuntime == null)
        {
            return;
        }

        _musicRuntime.PlayGunshot();
    }

    private void ApplyMusicSettings()
    {
        if (_musicRuntime == null)
        {
            return;
        }

        _musicRuntime.Configure(
            mainMenuMusic,
            overworldMusic,
            fightMusicA,
            fightMusicB,
            victoryClip,
            gunshotClip,
            musicVolume,
            victoryVolume,
            gunshotVolume,
            musicFadeDuration,
            victoryFadeDuration
        );
    }

    private static void EnsureMusicRuntime()
    {
        if (_musicRuntime != null)
        {
            return;
        }

        GameObject runtimeObject = new GameObject("MusicRuntime");
        Object.DontDestroyOnLoad(runtimeObject);
        _musicRuntime = runtimeObject.AddComponent<MusicRuntime>();
    }

    private sealed class MusicRuntime : MonoBehaviour
    {
        private AudioClip _mainMenuMusic;
        private AudioClip _overworldMusic;
        private AudioClip _fightMusicA;
        private AudioClip _fightMusicB;
        private AudioClip _victoryClip;
        private AudioClip _gunshotClip;
        private AudioSource _musicSourceA;
        private AudioSource _musicSourceB;
        private AudioSource _overlaySource;
        private AudioSource _sfxSource;
        private AudioSource _activeMusicSource;
        private bool _isConfigured;
        private Coroutine _musicRoutine;
        private Coroutine _overlayRoutine;
        private float _musicVolume = 1f;
        private float _victoryVolume = 1f;
        private float _gunshotVolume = 1f;
        private float _musicFadeDuration = 1f;
        private float _victoryFadeDuration = 1f;

        private void Awake()
        {
            _musicSourceA = CreateSource("Music A");
            _musicSourceB = CreateSource("Music B");
            _overlaySource = CreateSource("Overlay");
            _sfxSource = CreateSource("Sfx");
            _activeMusicSource = _musicSourceA;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void Configure(
            AudioClip mainMenuMusic,
            AudioClip overworldMusic,
            AudioClip fightMusicA,
            AudioClip fightMusicB,
            AudioClip victoryClip,
            AudioClip gunshotClip,
            float musicVolume,
            float victoryVolume,
            float gunshotVolume,
            float musicFadeDuration,
            float victoryFadeDuration)
        {
            bool wasConfigured = _isConfigured;
            _mainMenuMusic = mainMenuMusic;
            _overworldMusic = overworldMusic;
            _fightMusicA = fightMusicA;
            _fightMusicB = fightMusicB;
            _victoryClip = victoryClip;
            _gunshotClip = gunshotClip;
            _musicVolume = Mathf.Max(0f, musicVolume);
            _victoryVolume = Mathf.Max(0f, victoryVolume);
            _gunshotVolume = Mathf.Max(0f, gunshotVolume);
            _musicFadeDuration = Mathf.Max(0.01f, musicFadeDuration);
            _victoryFadeDuration = Mathf.Max(0.01f, victoryFadeDuration);
            _isConfigured = true;

            if (!wasConfigured)
            {
                HandleSceneMusic(SceneManager.GetActiveScene().name);
            }
        }

        public void PlayVictory()
        {
            if (!_isConfigured || _victoryClip == null)
            {
                return;
            }

            if (_musicRoutine != null)
            {
                StopCoroutine(_musicRoutine);
            }

            if (_overlayRoutine != null)
            {
                StopCoroutine(_overlayRoutine);
            }

            _musicRoutine = StartCoroutine(FadeOutMusicSources(_musicFadeDuration));
            _overlayRoutine = StartCoroutine(PlayOverlayClip(_victoryClip, _victoryVolume, 0.2f));
        }

        public void PlayGunshot()
        {
            if (!_isConfigured || _gunshotClip == null || _sfxSource == null)
            {
                return;
            }

            _sfxSource.PlayOneShot(_gunshotClip, _gunshotVolume);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            HandleSceneMusic(scene.name);
        }

        private void HandleSceneMusic(string sceneName)
        {
            if (!_isConfigured)
            {
                return;
            }

            if (sceneName == "Main Menu")
            {
                PlayMusic(_mainMenuMusic, true);
                return;
            }

            if (sceneName == "OverworldScene")
            {
                PlayMusic(_overworldMusic, true);
                return;
            }

            if (sceneName == BattleSessionState.BattleSceneName)
            {
                PlayMusic(ChooseBattleTrack(), true);
            }
        }

        private AudioClip ChooseBattleTrack()
        {
            if (_fightMusicA == null)
            {
                return _fightMusicB;
            }

            if (_fightMusicB == null)
            {
                return _fightMusicA;
            }

            return Random.value < 0.5f ? _fightMusicA : _fightMusicB;
        }

        private void PlayMusic(AudioClip clip, bool loop)
        {
            if (clip == null)
            {
                return;
            }

            if (_musicRoutine != null)
            {
                StopCoroutine(_musicRoutine);
            }

            if (_overlayRoutine != null)
            {
                StopCoroutine(_overlayRoutine);
            }

            AudioSource inactiveSource = _activeMusicSource == _musicSourceA ? _musicSourceB : _musicSourceA;
            if (_activeMusicSource.clip == clip &&
                _activeMusicSource.isPlaying &&
                !inactiveSource.isPlaying &&
                !_overlaySource.isPlaying)
            {
                _musicRoutine = StartCoroutine(FadeSourceVolume(_activeMusicSource, _musicVolume, _musicFadeDuration));
                return;
            }

            _musicRoutine = StartCoroutine(SwitchMusic(clip, loop));
        }

        private AudioSource CreateSource(string sourceName)
        {
            GameObject sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            return source;
        }

        private IEnumerator SwitchMusic(AudioClip clip, bool loop)
        {
            if (_overlaySource.isPlaying)
            {
                yield return FadeOutSource(_overlaySource, _victoryFadeDuration, true);
                _overlayRoutine = null;
            }

            yield return FadeOutMusicSources(_musicFadeDuration);

            AudioSource nextSource = _activeMusicSource == _musicSourceA ? _musicSourceB : _musicSourceA;
            nextSource.clip = clip;
            nextSource.loop = loop;
            nextSource.volume = 0f;
            nextSource.Play();
            _activeMusicSource = nextSource;
            yield return FadeSourceVolume(nextSource, _musicVolume, _musicFadeDuration);
        }

        private IEnumerator FadeOutMusicSources(float duration)
        {
            yield return FadeOutSource(_musicSourceA, duration, true);
            yield return FadeOutSource(_musicSourceB, duration, true);
            _activeMusicSource = _musicSourceA;
            _musicRoutine = null;
        }

        private IEnumerator PlayOverlayClip(AudioClip clip, float targetVolume, float fadeInDuration)
        {
            _overlaySource.Stop();
            _overlaySource.clip = clip;
            _overlaySource.loop = false;
            _overlaySource.volume = 0f;
            _overlaySource.Play();

            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDuration);
                _overlaySource.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }

            _overlaySource.volume = targetVolume;
            _overlayRoutine = null;
        }

        private IEnumerator FadeOutAndStop(AudioSource source, float duration)
        {
            yield return FadeOutSource(source, duration, true);
            _overlayRoutine = null;
        }

        private IEnumerator FadeSourceVolume(AudioSource source, float targetVolume, float duration)
        {
            float elapsed = 0f;
            float startVolume = source.volume;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                source.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }

            source.volume = targetVolume;
            _musicRoutine = null;
        }

        private IEnumerator FadeOutSource(AudioSource source, float duration, bool stopAfterFade)
        {
            if (source == null)
            {
                yield break;
            }

            if (!source.isPlaying && source.volume <= 0f)
            {
                if (stopAfterFade)
                {
                    source.Stop();
                    source.clip = null;
                    source.volume = 0f;
                }

                yield break;
            }

            float elapsed = 0f;
            float startVolume = source.volume;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                source.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            source.volume = 0f;

            if (stopAfterFade)
            {
                source.Stop();
                source.clip = null;
            }
        }
    }
}
