using System.Collections.Generic;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button continueButton;

    public GameObject creditsPanel;
    public GameObject warningPanel;

    private bool saveFileExists;
    private string savePath;

    private void Awake()
    {
        continueButton = HelperMethods.ParseGameObject("ContinueButton").GetComponent<Button>();
        creditsPanel = HelperMethods.ParseGameObject("CreditsPanel");
        warningPanel = HelperMethods.ParseGameObject("WarningPanel");
    }

    void Start()
    {
        savePath = SaveLoad.Instance.SavePath;
        saveFileExists = File.Exists(savePath);

        // Hide panels
        creditsPanel.SetActive(false);
        warningPanel.SetActive(false);

        // Non-interactable Continue button if a save doesn't exist
        if (saveFileExists)
        {
            continueButton.interactable = true;
        }
        else
        {
            continueButton.interactable = false;
        }
    }

    public void ShowCredits()
    {
        creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        creditsPanel.SetActive(false);
    }

    /// <summary>
    /// Quit the game
    /// </summary>
    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    /// <summary>
    /// Start a new game; gives last warning
    /// </summary>
    public void NewGame()
    {
        // Warning popup if a save exists. That way, they don't just accidentally start a new game
        if (saveFileExists)
        {
            warningPanel.SetActive(true);
        }
        else
        {
            // Start new game
            YesNewGame();
        }
    }

    public void Continue()
    {
        // Load scene based on JSON
        string json = File.ReadAllText(savePath);
        var saveData = JsonUtility.FromJson<SaveData>(json);
        string sceneToLoad = saveData.currentScene;
        SceneManager.LoadScene(sceneToLoad);
    }

    /// <summary>
    /// Start new game by deleting the old save file
    /// </summary>
    public void YesNewGame()
    {
        // Just delete the old save file
        File.Delete(savePath);
        Debug.Log($"Old save file deleted: {savePath}");

        Debug.Log("Starting a new game!");

        // Change the scene to the first starting scene
        SceneManager.LoadScene(ScriptConstants.newGameFirstScene);

        // Reason why we don't make a new one is that a save is made/updated at the end of each scene.
        // If the first scene is never finished, we don't wanna save since that would skip the first one.
    }

    /// <summary>
    /// Hide warning dialogue as no new game is started
    /// </summary>
    public void NoNewGame()
    {
        warningPanel.SetActive(false);
    }
}
