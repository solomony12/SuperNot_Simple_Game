using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoad : MonoBehaviour
{

    public static SaveLoad Instance;

    // Saving
    private const string SaveFileName = "progression_save.json";
    public string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    // Story parts reached
    private HashSet<string> reachedStates = new();

    // Latest story parts
    private string latestMainStory;
    private Dictionary<string, string> latestCharacterArcs = new();

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Save current unlocked states of the game in a JSON file
    /// </summary>
    public void SaveProgress()
    {
        // TODO: Enable this when necessary or done with build (currently disabled for debugging)
        //return;

        // Save data
        var saveData = new DialogueProgressionSaveData
        {
            currentScene = SceneManager.GetActiveScene().name,
            reachedStates = reachedStates.ToList(),
            latestMainStory = latestMainStory,
            latestCharacterArcs = latestCharacterArcs
                .Select(kvp => new CharacterArcEntry { character = kvp.Key, node = kvp.Value })
                .ToList()
        };

        // Save to JSON
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Progression saved");
        Debug.Log("Save path: " + Application.persistentDataPath);
    }

    /// <summary>
    /// Load current unlocked states of the game from a JSON file
    /// </summary>
    public void LoadProgress()
    {
        // New game - set initial main story part here
        if (!File.Exists(SavePath))
        {
            latestMainStory = ScriptConstants.startingStoryID;
            reachedStates = new HashSet<string>();
            latestCharacterArcs = new Dictionary<string, string>();
            Debug.Log("No save found. Starting new game with initial main story part");
            return;
        }

        string json = File.ReadAllText(SavePath);
        var saveData = JsonUtility.FromJson<DialogueProgressionSaveData>(json);

        // Load reached states and latest unlocked parts
        reachedStates = new HashSet<string>(saveData.reachedStates);
        latestMainStory = saveData.latestMainStory;
        latestCharacterArcs = new Dictionary<string, string>();

        // Load character arc nodes
        foreach (var arc in saveData.latestCharacterArcs)
        {
            latestCharacterArcs[arc.character] = arc.node;
        }

        Debug.Log("Progression loaded");
    }

    // Getter and Setter for reachedStates
    public HashSet<string> ReachedStates
    {
        get { return reachedStates; }
        set { reachedStates = value; }
    }

    // Getter and Setter for latestMainStory
    public string LatestMainStory
    {
        get { return latestMainStory; }
        set { latestMainStory = value; }
    }

    // Getter and Setter for latestCharacterArcs
    public Dictionary<string, string> LatestCharacterArcs
    {
        get { return latestCharacterArcs; }
        set { latestCharacterArcs = value; }
    }
}

// Embedded save data classes
[System.Serializable]
public class DialogueProgressionSaveData
{
    public string currentScene;
    public List<string> reachedStates;
    public string latestMainStory;
    public List<CharacterArcEntry> latestCharacterArcs;
}

[System.Serializable]
public class CharacterArcEntry
{
    public string character;
    public string node;
}
