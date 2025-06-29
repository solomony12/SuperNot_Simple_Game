using NUnit.Framework;
using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class SceneOrganizer : MonoBehaviour
{

    public static SceneOrganizer Instance;

    private List<string> temporaryGameObjectsList = new();
    private Dictionary<string, InteractableObjectData> temporaryGameObjectDetails = new();

    // TODO: any one of the three values could be empty so we need to account for that when animating
    private List<string> permanentGameObjectsList = new();
    private Dictionary<string, InteractableObjectData> permanentObjListData = new();

    private void Awake()
    {
        Instance = this;
    }

    // This is in SaveData:
    // A dictionary where key is a scene and values are lists of gameobjects per scene (Dictionary1 sceneToGameObjectsList<sceneName, List<GameObject>>)
    // Dictionary 2 has key of gameObject name and value of tuple (spriteName, position)
    // So use dictionary 1 to first select gameObjects names; then use those names in Dictionary 2 to store/view values

    // 0. When a scene starts, make a copy of gameObjectDetails. This will be what we use to update game objects temporarily during an active scene.
    // 1. The scene plays with any temporary animations. We update those game object names in the temporaryGameObjectDetails: [YarnCommand] TemporaryUpdateObject
    // 2. If it's a permanent animation, we also store the whole data in a permanentObjListData: [YarnCommand] PermanentUpdateObject
    // 3. When the scene ends, restore temps and update any permanent game object data into SaveLoad, which is done in EndofScene() in DialogueCommands script BEFORE the ReachState() is called
    // 4. ReachState() is called to SaveProgress()

    public void StoreOriginalGameObjectsData()
    {
        // Clear all data from last scene
        temporaryGameObjectsList.Clear();
        temporaryGameObjectDetails.Clear();
        permanentGameObjectsList.Clear();
        permanentObjListData.Clear();
        
        string currentScene = SaveLoad.Instance.CurrentScene;

        // If there's no key/scene in the dictionary, this means it's the first time we've visited the scene so we store its data
        if (!SaveLoad.Instance.SceneNameToGameObjectsList.ContainsKey(currentScene))
        {
            List<string> interactableObjects = GameObject
                .FindGameObjectsWithTag(ScriptConstants.interactableObjectString)
                .Select(obj => obj.name)
                .ToList();
            // Save the list of game objects in that scene
            SaveLoad.Instance.SceneNameToGameObjectsList[currentScene] = interactableObjects;
            foreach (string gameObjStr in interactableObjects)
            {
                // Store the game object's data
                GameObject gameObj = HelperMethods.ParseGameObject(gameObjStr);
                SaveLoad.Instance.GameObjectDetails[gameObjStr] = new InteractableObjectData {
                    spriteImageName = gameObj.GetComponent<Image>().sprite.name, 
                    position = gameObj.transform.position, 
                    shouldBeActive = gameObj.activeSelf };
            }
        }


        // Grab all game objects in the current scene (safe copy (shallow copy of list elements))
        temporaryGameObjectsList = new List<string>(SaveLoad.Instance.SceneNameToGameObjectsList[currentScene]);
        // Grab a copy of their data too
        temporaryGameObjectDetails = new Dictionary<string, InteractableObjectData>(SaveLoad.Instance.GameObjectDetails);
    }

    [YarnCommand("TemporaryUpdateObject")]
    public void TemporaryUpdateObject(string gameObjName, string spriteImageName, string positionString, string durationString, string shouldBeEnabledString)
    {
        // Parse to GameObject and Vector3
        var (gameObject, position, duration) = ParseGameObjectAndPositionAndDuration(gameObjName, positionString, durationString);
        // Parse to bool
        bool shouldBeEnabled = shouldBeEnabledString.Equals(true.ToString());

        // Store GameObject in temporaryObjList
        temporaryGameObjectsList.Add(gameObjName);

        // Update GameObject
        UpdateGameObject(gameObject, spriteImageName, position, duration, shouldBeEnabled);
    }

    [YarnCommand("PermanentUpdateObject")]
    public void PermanentUpdateObject(string gameObjName, string spriteImageName, string positionString, string durationString, string shouldBeEnabledString)
    {
        // Parse to GameObject and Vector3
        var (gameObject, position, duration) = ParseGameObjectAndPositionAndDuration(gameObjName, positionString, durationString);
        // Parse to bool
        bool shouldBeEnabled = shouldBeEnabledString.Equals(true.ToString());

        // Store data in permanentObjList for that object
        permanentGameObjectsList.Add(gameObjName);
        permanentObjListData.Add(gameObjName, new InteractableObjectData {
            spriteImageName = spriteImageName,
            position = position,
            shouldBeActive = shouldBeEnabled });

        // Update GameObject
        UpdateGameObject(gameObject, spriteImageName, position, duration, shouldBeEnabled);
    }

    // TODO: If animation is interrupted by next line/click before duration is up, automatically finish the position movement
    // Note: The Vector3 positions are absolute so it's not like (Move left by 10px) but (new position is here, here, and here)
    // Note: We can have multiple animations playing at once so we can only auto-finish when the next click for next dialogue
    // was pressed and ALL animations haven't been completed yet
    // TODO: This means we need to store all on-currently going on animations and check their timers to see if they're still running
    // CurrentVector = OldVector + NewVector rather than CurrentVector = OldVector -> NewVector
    public void UpdateGameObject(GameObject gameObj, string spriteImage, Vector3 pos, float duration, bool isActive)
    {
        // TODO: We update the data in temporaryGameObjectDetails and use that to update the GameObject
        if (duration == 0f)
        {
            // TODO: Update immediately
        }
        else
        {
            // TODO: Coroutine
        }
    }

    /// <summary>
    /// Restores all game objects that were updated temporarily during a scene
    /// </summary>
    public void RestoreGameObjects()
    {
        foreach (string movedObj in temporaryGameObjectsList)
        {
            // Restore position of that GameObject to the original data using SaveLoad's gameObjectDetails (string, Vector3, bool)
            GameObject gameObj = HelperMethods.ParseGameObject(movedObj);
            InteractableObjectData objData = SaveLoad.Instance.GameObjectDetails[movedObj];
            UpdateGameObject(gameObj, objData.spriteImageName, objData.position, 0f, objData.shouldBeActive); // 0f for duration since instant
        }
    }

    /// <summary>
    /// Updates all game objects that need to be the new starting point of all future scenes until further permanent updates
    /// NOTE: It's possible to have a temporary move after a permanent move, hence why we need to update permanent moves after a scene regardlessly
    /// </summary>
    public void PermanentlyChangeGameObjects()
    {
        foreach (string movedObj in permanentGameObjectsList)
        {
            // Update the SaveLoad data for that game object
            SaveLoad.Instance.GameObjectDetails[movedObj] = permanentObjListData[movedObj];
            // Update that GameObject to the new original position using permanentObjListData
            GameObject gameObj = HelperMethods.ParseGameObject(movedObj);
            InteractableObjectData objData = permanentObjListData[movedObj];
            UpdateGameObject(gameObj, objData.spriteImageName, objData.position, 0f, objData.shouldBeActive); // 0f for duration since instant
        }
    }

    /// <summary>
    /// Save the name of the current scene in SaveLoad
    /// </summary>
    public void SaveCurrentSceneName()
    {
        SaveLoad.Instance.CurrentScene = SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// Parses strings to GameObject and Vector3
    /// </summary>
    /// <param name="gameObjName">String to parse into GameObject</param>
    /// <param name="positionString">String to parse into Vector3</param>
    /// <returns>(GameObject, Vector3)</returns>
    /// <exception cref="Exception">Incorrect format or wrong name/GameObject found</exception>
    public static (GameObject, Vector3, float) ParseGameObjectAndPositionAndDuration(string gameObjName, string positionString, string durationString)
    {
        // Find the GameObject by name
        GameObject obj = HelperMethods.ParseGameObject(gameObjName);

        // Parse the position string into Vector3
        Vector3 position = HelperMethods.ParseVector3DefaultZero(positionString);

        // Parse the duration string into float
        float duration = HelperMethods.ParseFloatDefaultOne(durationString);

        return (obj, position, duration);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This method is called AFTER the scene has loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SaveCurrentSceneName();

        // Load game data for that scene
        try
        {
            List<string> interactableObjects = GameObject
                    .FindGameObjectsWithTag(ScriptConstants.interactableObjectString)
                    .Select(obj => obj.name)
                    .ToList();
            // For each interactable game object in the scene
            foreach (string gameObjStr in interactableObjects)
            {
                // Data only exists if it has been permanetly modified before
                if (SaveLoad.Instance.GameObjectDetails.TryGetValue(gameObjStr, out InteractableObjectData objData))
                {
                    Debug.Log("Modified info");
                    GameObject gameObj = HelperMethods.ParseGameObject(gameObjStr);
                    // Update image, position, and isActive
                    gameObj.GetComponent<Image>().sprite.name = objData.spriteImageName;
                    gameObj.transform.position = objData.position;
                    gameObj.SetActive(objData.shouldBeActive);
                }

            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }

        // Set markers
        BottomToTopInteraction.Instance.SetMarkers();
    }
}
