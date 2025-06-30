using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoad : MonoBehaviour
{

    public static SaveLoad Instance;

    // Saving
    private const string SaveFileName = "progression_save.json";
    public string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    // ----- DIALOGUE PROGRESSION MANAGER DATA -----

    // Story parts reached
    private HashSet<string> reachedStates = new();

    // Latest story parts
    private string latestMainStory;
    private Dictionary<string, string> latestCharacterArcs = new();

    // ----- SCENE ORGANIZER DATA -----

    private string currentScene;

    // Stores a list of game objects for each scene
    private Dictionary<string, List<string>> sceneNameToGameObjectsList = new();

    // Stores the current image 'sprite name' (string), 'vector position' of each game object, and 'shouldBeSetActive' (bool)
    private Dictionary<string, InteractableObjectData> gameObjectDetails = new();


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
        var saveData = new SaveData
        {
            reachedStates = reachedStates.ToList(),
            latestMainStory = latestMainStory,
            latestCharacterArcs = latestCharacterArcs
                .Select(kvp => new CharacterArcEntry { character = kvp.Key, node = kvp.Value })
                .ToList(),

            currentScene = currentScene,
            sceneNameToGameObjectsList = sceneNameToGameObjectsList
                .Select(kvp => new SceneGameObjectsEntry { sceneName = kvp.Key, gameObjects = kvp.Value })
                .ToList(),

            gameObjectDetails = gameObjectDetails
                .Select(kvp => new GameObjectDetailsEntry { objectName = kvp.Key, data = kvp.Value })
                .ToList()
        };

        // Save to JSON
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Progression saved");
        //Debug.Log("Save path: " + Application.persistentDataPath);
    }

    /// <summary>
    /// Load current unlocked states of the game from a JSON file
    /// </summary>
    public void LoadProgress()
    {
        // New game - set initial main story part here
        if (!File.Exists(SavePath))
        {
            // ----- DIALOGUE PROGRESSION MANAGER DATA -----
            latestMainStory = ScriptConstants.startingStoryID;
            reachedStates = new HashSet<string>();
            latestCharacterArcs = new Dictionary<string, string>();

            // ----- SCENE ORGANIZER DATA -----
            currentScene = ScriptConstants.newGameFirstScene;
            sceneNameToGameObjectsList = new Dictionary<string, List<string>>();
            gameObjectDetails = new Dictionary<string, InteractableObjectData>();

            Debug.Log("No save found. Starting new game with initial main story part");
            return;
        }

        // Load data:
        string json = File.ReadAllText(SavePath);
        var saveData = JsonUtility.FromJson<SaveData>(json);

        // ----- DIALOGUE PROGRESSION MANAGER DATA -----

        // Load reached states and latest unlocked parts
        reachedStates = new HashSet<string>(saveData.reachedStates);
        latestMainStory = saveData.latestMainStory;
        latestCharacterArcs = new Dictionary<string, string>();
        // Load character arc nodes
        foreach (var arc in saveData.latestCharacterArcs)
        {
            latestCharacterArcs[arc.character] = arc.node;
        }

        // ----- SCENE ORGANIZER DATA -----

        currentScene = saveData.currentScene;

        // Could be empty if no game objects have ever been moved in any scenes ever
        sceneNameToGameObjectsList = saveData.sceneNameToGameObjectsList != null
            ? saveData.sceneNameToGameObjectsList.ToDictionary(entry => entry.sceneName, entry => new List<string>(entry.gameObjects))
            : new Dictionary<string, List<string>>();

        // Could also be empty if no game objects have ever been moved in any scenes ever
        gameObjectDetails = saveData.gameObjectDetails != null
            ? saveData.gameObjectDetails.ToDictionary(entry => entry.objectName, entry => entry.data)
            : new Dictionary<string, InteractableObjectData>();

        //Debug.Log("Progression loaded");
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

    // Getter and Setter for currentScene
    public string CurrentScene
    {
        get { return currentScene; }
        set { currentScene = value; }
    }

    // Getter and Setter for sceneNameToGameObjectsList
    public Dictionary<string, List<string>> SceneNameToGameObjectsList
    {
        get { return sceneNameToGameObjectsList; }
        set { sceneNameToGameObjectsList = value; }
    }

    // Getter and Setter for gameObjectDetails
    public Dictionary<string, InteractableObjectData> GameObjectDetails
    {
        get { return gameObjectDetails; }
        set { gameObjectDetails = value; }
    }
}

// Embedded save data classes
[System.Serializable]
public class SaveData
{
    public List<string> reachedStates;
    public string latestMainStory;
    public List<CharacterArcEntry> latestCharacterArcs;

    public string currentScene;
    public List<SceneGameObjectsEntry> sceneNameToGameObjectsList;
    public List<GameObjectDetailsEntry> gameObjectDetails;
}

[System.Serializable]
public class CharacterArcEntry
{
    public string character;
    public string node;
}

[System.Serializable]
public class SceneGameObjectsEntry
{
    public string sceneName;
    public List<string> gameObjects;
}

[System.Serializable]
public class GameObjectDetailsEntry
{
    public string objectName;
    public InteractableObjectData data;
}

[System.Serializable]
public class InteractableObjectData
{
    public string spriteImageName;
    public Vector2 position;
    public bool shouldBeActive;
}
