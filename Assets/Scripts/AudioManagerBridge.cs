using UnityEngine;

/// <summary>
/// Puente estático para acceder al AudioManager desde los botones en el Inspector.
/// Este es el ÚNICO script adicional que necesitas.
/// </summary>
public class AudioManagerBridge : MonoBehaviour
{
    // ===== MÉTODOS PARA BOTONES (UI) =====

    public void PlayButtonClick()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    public void PlayButtonClay()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClay();
    }

    public void PlayTurnThePage()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTurnThePage();
    }

    // ===== MÉTODOS PARA GAMEPLAY =====

    public void PlayCardPlaced()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCardPlaced();
    }

    public void PlayCardAttack()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCardAttack();
    }

    public void PlayCardDefeat()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCardDefeat();
    }

    public void PlayHealthLost()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHealthLost();
    }

    public void PlayTimerTick()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTimerTick();
    }

    public void PlayTimeUp()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTimeUp();
    }

    // ===== MÉTODOS PARA MÚSICA =====

    public void PlayMainMenuMusic()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMainMenuMusic();
    }

    public void PlayGameplayMusic()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameplayMusic();
    }

    public void PlayVictoryMusic()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayVictoryMusic();
    }

    public void PlayDefeatMusic()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDefeatMusic();
    }

    public void StopMusic()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();
    }

    public void PauseMusic()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PauseMusic();
    }

    public void ResumeMusic()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeMusic();
    }
}