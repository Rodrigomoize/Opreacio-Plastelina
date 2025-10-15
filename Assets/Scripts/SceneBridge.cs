using UnityEngine;

public class SceneBridge : MonoBehaviour
{
    public static void LoadMainMenu()
    {
        GameManager.GoToMainMenu();
    }

    public static void LoadInstructionScene()
    {
        GameManager.GoToInstructionScene();
    }

    public static void LoadLevelScene()
    {
        GameManager.GoToLevelScene();
    }

    public static void LoadPlayScene()
    {
        GameManager.GoToPlayScene();
    }
}