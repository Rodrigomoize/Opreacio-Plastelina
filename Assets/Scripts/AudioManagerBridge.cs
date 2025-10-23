using UnityEngine;

/// Puente estático para acceder al AudioManager desde los botones en el Inspector.
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

    public void PlayCardSpawnCharacter()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCardSpawnCharacter();
    }

    public void PlayCardSelected()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCardSelected();
    }

    public void PlayOperationCreated()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOperationCreated();
    }

    public void PlayTowerDestroyed()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTowerDestroyed();
    }

    public void PlayTimer5Seconds()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTimer5Seconds();
    }

    public void PlayOperatorSelected()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOperatorSelected();
    }

    public void PlayAttackDefenseCollision()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayAttackDefenseCollision();
    }

    public void PlayTowerHit()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTowerHit();
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