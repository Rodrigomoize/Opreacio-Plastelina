using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Tracks")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    [Header("Ending SFX")]
    [SerializeField] private AudioClip victorySFX;
    [SerializeField] private AudioClip defeatSFX;

    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip buttonClaySFX;
    [SerializeField] private AudioClip turnThePageSFX;

    [Header("Gameplay Sound Effects")]
    [SerializeField] private AudioClip cardSelectedSFX;
    [SerializeField] private AudioClip towerDestroyedSFX;
    [SerializeField] private AudioClip timer5SecondsSFX;
    [SerializeField] private AudioClip operatorSelectedSFX;
    [SerializeField] private AudioClip attackDefenseCollisionSFX;
    [SerializeField] private AudioClip towerHitSFX;

    [Header("Spawn SFX - 6 Tipos de Unidades")]
    [Tooltip("Sonidos cuando se crea un CAMIÓN/OPERACIÓN (se elige uno aleatorio)")]
    public AudioClip[] operationCreatedSFX;

    [Tooltip("Sonido cuando se crea una TROPA de valor 1")]
    public AudioClip troopValue1CreatedSFX;

    [Tooltip("Sonido cuando se crea una TROPA de valor 2")]
    public AudioClip troopValue2CreatedSFX;

    [Tooltip("Sonido cuando se crea una TROPA de valor 3")]
    public AudioClip troopValue3CreatedSFX;

    [Tooltip("Sonido cuando se crea una TROPA de valor 4")]
    public AudioClip troopValue4CreatedSFX;

    [Tooltip("Sonido cuando se crea una TROPA de valor 5")]
    public AudioClip troopValue5CreatedSFX;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Header("Fade Settings")]
    [SerializeField] private float musicFadeDuration = 1f;

    private Coroutine musicFadeCoroutine;
    private Dictionary<string, float> sfxCooldowns = new Dictionary<string, float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();

        Debug.Log("[AudioManager] Inicializado correctamente");
    }

    private void Start()
{
    // Reproducir música de menú al iniciar si estamos en una escena de menú
    string currentScene = SceneManager.GetActiveScene().name;
    string[] menuScenes = { "MainMenu", "HistoryScene", "InstructionScene", "LevelScene" };
    
    if (System.Array.Exists(menuScenes, s => s == currentScene))
    {
        PlayMainMenuMusic();
        Debug.Log($"[AudioManager] Música de menú iniciada automáticamente en {currentScene}");
    }
}

    private void SetupAudioSources()
    {
        if (musicSource == null)
        {
            Transform existingMusic = transform.Find("MusicSource");
            if (existingMusic != null)
            {
                musicSource = existingMusic.GetComponent<AudioSource>();
            }

            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                Debug.Log("[AudioManager] MusicSource creado automáticamente");
            }
        }

        if (sfxSource == null)
        {
            Transform existingSFX = transform.Find("SFXSource");
            if (existingSFX != null)
            {
                sfxSource = existingSFX.GetComponent<AudioSource>();
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                Debug.Log("[AudioManager] SFXSource creado automáticamente");
            }
        }

        if (musicSource != null) musicSource.volume = musicVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    // ===== MÚSICA =====

    public void PlayMusic(AudioClip clip, bool fadeIn = true)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] Clip de música nulo");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        if (fadeIn)
        {
            musicFadeCoroutine = StartCoroutine(CrossfadeMusic(clip));
        }
        else
        {
            musicSource.clip = clip;
            musicSource.time = 0f;
            musicSource.Play();
        }
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }

    public void PlayVictorySFX()
    {
        if (victorySFX != null)
        {
            PlaySFX(victorySFX);
            Debug.Log("[AudioManager] 🎉 SFX de victoria reproducido (una vez)");
        }
        else
        {
            Debug.LogWarning("[AudioManager] victorySFX no asignado");
        }
    }

    public void PlayDefeatSFX()
    {
        if (defeatSFX != null)
        {
            PlaySFX(defeatSFX);
            Debug.Log("[AudioManager] 💀 SFX de derrota reproducido (una vez)");
        }
        else
        {
            Debug.LogWarning("[AudioManager] defeatSFX no asignado");
        }
    }

    public void StopMusic(bool fadeOut = true)
    {
        if (fadeOut)
        {
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }
            musicFadeCoroutine = StartCoroutine(FadeOutMusic());
        }
        else
        {
            musicSource.Stop();
        }
    }

    public void StopAndResetMusic()
    {
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        musicSource.Stop();
        musicSource.time = 0f;
        musicSource.clip = null;
        musicSource.volume = musicVolume;

        Debug.Log("[AudioManager] Música detenida y reseteada completamente");
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    // ===== EFECTOS DE SONIDO =====

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] Clip de SFX nulo");
            return;
        }

        sfxSource.PlayOneShot(clip, volumeScale * sfxVolume);
    }

    public void PlaySFXWithCooldown(AudioClip clip, string cooldownKey, float cooldownTime = 0.1f)
    {
        if (clip == null) return;

        float lastPlayTime;
        if (sfxCooldowns.TryGetValue(cooldownKey, out lastPlayTime))
        {
            if (Time.unscaledTime - lastPlayTime < cooldownTime)
            {
                return;
            }
        }

        PlaySFX(clip);
        sfxCooldowns[cooldownKey] = Time.unscaledTime;
    }

    // ===== EFECTOS DE UI =====

    public void PlayButtonClick()
    {
        PlaySFXWithCooldown(buttonClickSFX, "buttonClick", 0.1f);
    }

    public void PlayButtonClay()
    {
        PlaySFXWithCooldown(buttonClaySFX, "buttonClay", 0.05f);
    }

    public void PlayTurnThePage()
    {
        PlaySFX(turnThePageSFX);
    }

    // ===== EFECTOS DE GAMEPLAY =====

    public void PlayCardSelected()
    {
        PlaySFX(cardSelectedSFX);
    }

    public void PlayTowerDestroyed()
    {
        PlaySFX(towerDestroyedSFX);
    }

    public void PlayTimer5Seconds()
    {
        PlaySFX(timer5SecondsSFX);
    }

    public void PlayOperatorSelected()
    {
        PlaySFX(operatorSelectedSFX);
    }

    public void PlayAttackDefenseCollision()
    {
        PlaySFX(attackDefenseCollisionSFX);
    }

    public void PlayTowerHit()
    {
        PlaySFX(towerHitSFX);
    }

    // ===== SONIDOS DE SPAWN (6 TIPOS) =====

    /// Reproduce un sonido ALEATORIO de camión/operación
    public void PlayOperationCreated()
    {
        if (operationCreatedSFX != null && operationCreatedSFX.Length > 0)
        {
            // Filtrar clips nulos
            List<AudioClip> validClips = new List<AudioClip>();
            foreach (var clip in operationCreatedSFX)
            {
                if (clip != null)
                    validClips.Add(clip);
            }

            if (validClips.Count > 0)
            {
                // Elegir uno aleatorio
                int randomIndex = Random.Range(0, validClips.Count);
                AudioClip selectedClip = validClips[randomIndex];
                
                sfxSource.PlayOneShot(selectedClip, sfxVolume);
                Debug.Log($"[AudioManager] 🚛 Sonido de camión reproducido (variante {randomIndex + 1}/{validClips.Count})");
            }
            else
            {
                Debug.LogWarning("[AudioManager] operationCreatedSFX array no tiene clips asignados");
            }
        }
        else
        {
            Debug.LogWarning("[AudioManager] operationCreatedSFX array vacío o no asignado");
        }
    }

    public void PlayTroopValue1Created()
    {
        if (troopValue1CreatedSFX != null)
        {
            sfxSource.PlayOneShot(troopValue1CreatedSFX, sfxVolume);
            Debug.Log("[AudioManager] 1️⃣ Sonido de tropa valor 1 reproducido");
        }
        else
        {
            Debug.LogWarning("[AudioManager] troopValue1CreatedSFX no asignado");
        }
    }

    public void PlayTroopValue2Created()
    {
        if (troopValue2CreatedSFX != null)
        {
            sfxSource.PlayOneShot(troopValue2CreatedSFX, sfxVolume);
            Debug.Log("[AudioManager] 2️⃣ Sonido de tropa valor 2 reproducido");
        }
        else
        {
            Debug.LogWarning("[AudioManager] troopValue2CreatedSFX no asignado");
        }
    }

    public void PlayTroopValue3Created()
    {
        if (troopValue3CreatedSFX != null)
        {
            sfxSource.PlayOneShot(troopValue3CreatedSFX, sfxVolume);
            Debug.Log("[AudioManager] 3️⃣ Sonido de tropa valor 3 reproducido");
        }
        else
        {
            Debug.LogWarning("[AudioManager] troopValue3CreatedSFX no asignado");
        }
    }

    public void PlayTroopValue4Created()
    {
        if (troopValue4CreatedSFX != null)
        {
            sfxSource.PlayOneShot(troopValue4CreatedSFX, sfxVolume);
            Debug.Log("[AudioManager] 4️⃣ Sonido de tropa valor 4 reproducido");
        }
        else
        {
            Debug.LogWarning("[AudioManager] troopValue4CreatedSFX no asignado");
        }
    }

    public void PlayTroopValue5Created()
    {
        if (troopValue5CreatedSFX != null)
        {
            sfxSource.PlayOneShot(troopValue5CreatedSFX, sfxVolume);
            Debug.Log("[AudioManager] 5️⃣ Sonido de tropa valor 5 reproducido");
        }
        else
        {
            Debug.LogWarning("[AudioManager] troopValue5CreatedSFX no asignado");
        }
    }

    // ===== CONTROL DE VOLUMEN =====

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;

    // ===== COROUTINES PARA FADE =====

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        float startVolume = musicSource.volume;

        float elapsed = 0f;
        while (elapsed < musicFadeDuration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (musicFadeDuration / 2f));
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.time = 0f;
        musicSource.Play();

        elapsed = 0f;
        while (elapsed < musicFadeDuration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / (musicFadeDuration / 2f));
            yield return null;
        }

        musicSource.volume = musicVolume;
        musicFadeCoroutine = null;
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < musicFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = musicVolume;
        musicFadeCoroutine = null;
    }
}