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

    public void PlayCardSelected()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCardSelected();
    }

    public void PlayOperatorSelected()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOperatorSelected();
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

    // ===== MÉTODOS PARA SPAWN (6 TIPOS) =====

    public void PlayOperationCreated()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOperationCreated();
    }

    public void PlayTroopValue1Created()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTroopValue1Created();
    }

    public void PlayTroopValue2Created()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTroopValue2Created();
    }

    public void PlayTroopValue3Created()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTroopValue3Created();
    }

    public void PlayTroopValue4Created()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTroopValue4Created();
    }

    public void PlayTroopValue5Created()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTroopValue5Created();
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

    // CAMBIADO: Ahora son métodos SFX en lugar de música
    public void PlayVictorySFX()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayVictorySFX();
    }

    public void PlayDefeatSFX()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDefeatSFX();
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