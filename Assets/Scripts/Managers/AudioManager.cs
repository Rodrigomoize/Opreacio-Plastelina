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
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip defeatMusic;

    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip buttonClaySFX;
    [SerializeField] private AudioClip turnThePageSFX;

    [Header("Gameplay Sound Effects")]
    [SerializeField] private AudioClip cardSelectedSFX;            // Sonido al seleccionar una carta
    [SerializeField] private AudioClip towerDestroyedSFX;          // Sonido cuando se destruye la torre
    [SerializeField] private AudioClip timer5SecondsSFX;           // Sonido cuando quedan 5 segundos
    [SerializeField] private AudioClip operatorSelectedSFX;        // Sonido al seleccionar botón operador (suma/resta)
    [SerializeField] private AudioClip attackDefenseCollisionSFX;  // Sonido cuando ataque y defensa se encuentran y destruyen
    [SerializeField] private AudioClip towerHitSFX;                // Sonido al golpear la torre del jugador

    [Header("Spawn SFX - 6 Tipos de Unidades")]
    [Tooltip("Sonido cuando se crea un CAMIÓN/OPERACIÓN")]
    public AudioClip operationCreatedSFX;

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
        // Implementación del Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Crear AudioSources si no existen
        SetupAudioSources();

        Debug.Log("[AudioManager] Inicializado correctamente");
    }

    private void SetupAudioSources()
    {
        // Si no hay AudioSources asignados, crearlos
        if (musicSource == null)
        {
            // Buscar si ya existe un hijo llamado MusicSource
            Transform existingMusic = transform.Find("MusicSource");
            if (existingMusic != null)
            {
                musicSource = existingMusic.GetComponent<AudioSource>();
            }

            // Si aún no existe, crearlo
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
            // Buscar si ya existe un hijo llamado SFXSource
            Transform existingSFX = transform.Find("SFXSource");
            if (existingSFX != null)
            {
                sfxSource = existingSFX.GetComponent<AudioSource>();
            }

            // Si aún no existe, crearlo
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

        // Aplicar volúmenes iniciales
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

        // Si ya está sonando la misma música, no hacer nada
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

    public void PlayVictoryMusic()
    {
        PlayMusic(victoryMusic, false);
    }

    public void PlayDefeatMusic()
    {
        PlayMusic(defeatMusic, false);
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

    /// Reproduce sonido cuando se crea un camión/operación
    public void PlayOperationCreated()
    {
        if (operationCreatedSFX != null)
        {
            sfxSource.PlayOneShot(operationCreatedSFX, sfxVolume);
            Debug.Log("[AudioManager] 🚛 Sonido de camión reproducido");
        }
        else
        {
            Debug.LogWarning("[AudioManager] operationCreatedSFX no asignado");
        }
    }

    /// Reproduce sonido cuando se crea una tropa de valor 1
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

    /// Reproduce sonido cuando se crea una tropa de valor 2
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

    /// Reproduce sonido cuando se crea una tropa de valor 3
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

    /// Reproduce sonido cuando se crea una tropa de valor 5
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

        // Fade out
        float elapsed = 0f;
        while (elapsed < musicFadeDuration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (musicFadeDuration / 2f));
            yield return null;
        }

        // Cambiar clip y resetear tiempo
        musicSource.clip = newClip;
        musicSource.time = 0f;
        musicSource.Play();

        // Fade in
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