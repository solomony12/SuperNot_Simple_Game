using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptsManager : MonoBehaviour
{
    public static ScriptsManager Instance;

    public event Action OnScriptsLoaded;

    // The script types to manage (ORDER DOES MATTER)
    private readonly System.Type[] managedComponents = new System.Type[]
    {
        typeof(DialogueCommands),
        typeof(CharacterCommands)
    };

    void Awake()
    {
        // Ensure singleton instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Hook into scene change events
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    // Called after the new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == ScriptConstants.mainMenuSceneName)
        {
            Debug.Log("ScriptsManager: Skipping adding scripts on MainMenu scene.");
            return;
        }

        // Add the components
        foreach (var type in managedComponents)
        {
            // Remove existing copy if present
            var existing = GetComponent(type);
            if (existing != null)
            {
                Destroy(existing);
                //Debug.Log($"ScriptsManager: Removed old {type.Name}");
            }

            // Add a fresh copy
            gameObject.AddComponent(type);
            //Debug.Log($"ScriptsManager: Added {type.Name}");
        }

        Debug.Log("OnScriptsLoaded Invoked");
        OnScriptsLoaded?.Invoke();
    }

    // Called *before* the new scene becomes active
    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        foreach (var type in managedComponents)
        {
            var existing = GetComponent(type);
            if (existing != null)
            {
                Destroy(existing);
                Debug.Log($"ScriptsManager: Cleaned up {type.Name} before scene change.");
            }
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;

        if (Instance == this)
            Instance = null;
    }
}
