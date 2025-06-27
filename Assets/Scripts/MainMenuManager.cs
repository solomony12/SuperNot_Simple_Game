using System.Collections.Generic;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button continueButton;

    public GameObject creditsPanel;

    private bool saveFileExists;

    private void Awake()
    {
        continueButton = GameObject.Find("ContinueButton").GetComponent<Button>();
        creditsPanel = GameObject.Find("CreditsPanel");
    }

    void Start()
    {
        saveFileExists = File.Exists(DialogueProgressionManager.Instance.SavePath);

        creditsPanel.SetActive(false);

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

    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void NewGame()
    {

    }

    public void Continue()
    {
        //DialogueProgressionManager.Instance.SavePath
    }
}
