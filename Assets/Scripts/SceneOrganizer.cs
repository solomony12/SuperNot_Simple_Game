using NUnit.Framework;
using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.IO;
using UnityEngine.Windows;
using static UnityEditor.PlayerSettings;

public class SceneOrganizer : MonoBehaviour
{

    public static SceneOrganizer Instance;

    private List<string> copyOfGameObjects = new();

    private static List<string> temporaryGameObjectsList = new();
    private static Dictionary<string, InteractableObjectData> temporaryGameObjectDetails = new();

    // TODO: any one of the three values could be empty so we need to account for that when animating
    private static List<string> permanentGameObjectsList = new();
    private static Dictionary<string, InteractableObjectData> permanentObjListData = new();

    private Dictionary<GameObject, Coroutine> activeAnimations = new Dictionary<GameObject, Coroutine>();
    private Dictionary<GameObject, Vector2> targetPositions = new Dictionary<GameObject, Vector2>();

    public event Action OnGameObjectsLoaded;

    private void Awake()
    {
        Instance = this;

        // Upon Unity Scene load, wait for data to come before setting markers
        Debug.Log("Subscribing to OnDataInitialized");
        DialogueProgressionManager.Instance.OnDataInitialized += OnSceneLoaded;
        DialogueCommands.Instance.OnNextDialogueLinePlayed += FinishAllAnimationsImmediately;
    }

    private void Start()
    {

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
        copyOfGameObjects.Clear();
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
                    position = gameObj.GetComponent<RectTransform>().anchoredPosition, 
                    shouldBeActive = gameObj.activeSelf };
            }
        }


        // Grab all game objects in the current scene (safe copy (shallow copy of list elements))
        copyOfGameObjects = new List<string>(SaveLoad.Instance.SceneNameToGameObjectsList[currentScene]);
        // Grab a copy of their data too
        temporaryGameObjectDetails = new Dictionary<string, InteractableObjectData>(SaveLoad.Instance.GameObjectDetails);
    }

    [YarnCommand("TemporaryUpdateObject")]
    public static void TemporaryUpdateObject(string gameObjName, string spriteImageName, string positionString, string durationString, string shouldBeEnabledString)
    {
        // Parse to GameObject and Vector2
        var (gameObject, position, duration) = ParseGameObjectAndPositionAndDuration(gameObjName, positionString, durationString);
        // Parse to bool
        bool shouldBeEnabled = string.IsNullOrEmpty(shouldBeEnabledString) ||
                       shouldBeEnabledString.Equals("true", StringComparison.OrdinalIgnoreCase);

        // Store GameObject in temporaryObjList
        temporaryGameObjectsList.Add(gameObjName);

        // Update GameObject
        Instance.UpdateGameObject(gameObject, spriteImageName, position, duration, shouldBeEnabled);
    }

    [YarnCommand("PermanentUpdateObject")]
    public static void PermanentUpdateObject(string gameObjName, string spriteImageName, string positionString, string durationString, string shouldBeEnabledString)
    {
        // Parse to GameObject and Vector2
        var (gameObject, position, duration) = ParseGameObjectAndPositionAndDuration(gameObjName, positionString, durationString);
        // Parse to bool
        bool shouldBeEnabled = string.IsNullOrEmpty(shouldBeEnabledString) ||
                       shouldBeEnabledString.Equals("true", StringComparison.OrdinalIgnoreCase);

        // Store data in permanentObjList for that object
        permanentGameObjectsList.Add(gameObjName);
        permanentObjListData.Add(gameObjName, new InteractableObjectData {
            spriteImageName = spriteImageName,
            position = position,
            shouldBeActive = shouldBeEnabled });

        // Update GameObject
        Instance.UpdateGameObject(gameObject, spriteImageName, position, duration, shouldBeEnabled);
    }

    // TODO: If animation is interrupted by next line/click before duration is up, automatically finish the position movement
    // Note: The Vector2 positions are absolute so it's not like (Move left by 10px) but (new position is here, here, and here)
    // Note: We can have multiple animations playing at once so we can only auto-finish when the next click for next dialogue
    // was pressed and ALL animations haven't been completed yet
    // TODO: This means we need to store all on-currently going on animations and check their timers to see if they're still running
    // CurrentVector = OldVector + NewVector rather than CurrentVector = OldVector -> NewVector
    public void UpdateGameObject(GameObject gameObj, string spriteImage, Vector2 pos, float duration, bool isActive)
    {
        Debug.Log($"Updating game object '{gameObj.name}' with image '{spriteImage}' to position '{pos.ToString()} (Anchored)' and isActive = {isActive}");

        // Image and isActive are updated immediately regardless of coroutine or not
        if (!string.IsNullOrEmpty(spriteImage))
        {
            string path = $"{ScriptConstants.bottomPanelArtPath}{SaveLoad.Instance.CurrentScene}/{spriteImage}";
            Sprite newSprite = Resources.Load<Sprite>(path);
            if (newSprite == null)
            {
                throw new Exception($"Sprite not found at Resources/{path}");
            }
            gameObj.GetComponent<Image>().sprite = newSprite;
        }
        gameObj.SetActive(isActive);

        // If a position was added
        if (pos != Vector2.zero)
        {
            // Get the transform for relative positioning
            RectTransform gameObjTransform = gameObj.GetComponent<RectTransform>();

            // We update the data in temporaryGameObjectDetails and use that to update the GameObject
            if (duration == 0f)
            {
                // Update position immediately
                gameObjTransform.anchoredPosition = pos;
            }
            // Animate position otherwise
            else
            {
                // If a previous animation is still running
                if (activeAnimations.ContainsKey(gameObj))
                {
                    // Snap to the final position of the previous animation
                    if (targetPositions.ContainsKey(gameObj))
                    {
                        gameObjTransform.anchoredPosition = targetPositions[gameObj];
                    }

                    // Stop the previous animation
                    StopCoroutine(activeAnimations[gameObj]);

                    // Clean up the previous animatino
                    activeAnimations.Remove(gameObj);
                    targetPositions.Remove(gameObj);
                }

                // Store new target position for this upcoming animation
                targetPositions[gameObj] = pos;

                // Start the new animation
                Coroutine anim = StartCoroutine(AnimateMovement(gameObj, pos, duration));
                activeAnimations[gameObj] = anim;
            }
        }
    }

    /// <summary>
    /// Moves <paramref name="gameObj"/> to <paramref name="targetPos"/> in <paramref name="duration"/> sections
    /// </summary>
    /// <param name="gameObj">GameObject to move</param>
    /// <param name="targetPos">Absolute location to move it to</param>
    /// <param name="duration">Aniation time in seconds</param>
    /// <returns>IEnumerator for animation coroutine</returns>
    private IEnumerator AnimateMovement(GameObject gameObj, Vector3 targetPos, float duration)
    {
        // Get the transform for relative positioning
        RectTransform gameObjTransform = gameObj.GetComponent<RectTransform>();

        Vector3 startPos = gameObjTransform.anchoredPosition;
        float timeElapsed = 0f;

        // Move
        while (timeElapsed < duration)
        {
            gameObjTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Set final position
        gameObjTransform.anchoredPosition = targetPos;

        // Remove from active list
        activeAnimations.Remove(gameObj);
        targetPositions.Remove(gameObj);
    }


    void OnEnable()
    {
        DialogueCommands.Instance.OnSceneEndMid += RunCleanUp;
    }

    void OnDisable()
    {
        DialogueCommands.Instance.OnSceneEndMid -= RunCleanUp;
    }

    void RunCleanUp()
    {
        // SceneOrganizer needs to restore temp and update permanent data BEFORE ReachState is called to save progress
        FinishAllAnimationsImmediately();
        RestoreGameObjects();
        PermanentlyChangeGameObjects();
    }

    // TODO: We need to be able to call this every time a dialogue line is completed
    /// <summary>
    /// Finishes all currently running animations
    /// </summary>
    public void FinishAllAnimationsImmediately()
    {
        Debug.Log("Finishing all current animations");
        // For all current animations
        foreach (var kvp in activeAnimations)
        {
            GameObject obj = kvp.Key;
            Coroutine anim = kvp.Value;

            // Stop animation
            StopCoroutine(anim);

            // Snap to final position
            if (targetPositions.ContainsKey(obj))
            {
                // Get the transform for relative positioning
                RectTransform gameObjTransform = obj.GetComponent<RectTransform>();
                gameObjTransform.anchoredPosition = targetPositions[obj];
            }
        }

        // Clean up
        activeAnimations.Clear();
        targetPositions.Clear();
    }

    /// <summary>
    /// Restores all game objects that were updated temporarily during a scene
    /// </summary>
    public void RestoreGameObjects()
    {
        //Debug.Log("RESTORE GAME OBJECTS");
        foreach (string movedObj in temporaryGameObjectsList)
        {
            // Restore position of that GameObject to the original data using SaveLoad's gameObjectDetails (string, Vector2, bool)
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
        //Debug.Log("PERMANENT GAME OBJECTS");
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
        //Debug.Log($"Saving current scene '{SceneManager.GetActiveScene().name}'");
        SaveLoad.Instance.CurrentScene = SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// Parses strings to GameObject and Vector2
    /// </summary>
    /// <param name="gameObjName">String to parse into GameObject</param>
    /// <param name="positionString">String to parse into Vector2</param>
    /// <returns>(GameObject, Vector2)</returns>
    /// <exception cref="Exception">Incorrect format or wrong name/GameObject found</exception>
    public static (GameObject, Vector2, float) ParseGameObjectAndPositionAndDuration(string gameObjName, string positionString, string durationString)
    {
        // Find the GameObject by name
        GameObject obj = HelperMethods.ParseGameObject(gameObjName);

        // Parse the position string into Vector2 (in relation to bottomPanel)
        Vector2 position = HelperMethods.ParseVector2OrDefaultZero(positionString);

        // Parse the duration string into float
        float duration = HelperMethods.ParseFloatDefaultOne(durationString);

        return (obj, position, duration);
    }

    // This method is called AFTER the scene has loaded and data has loaded in
    private void OnSceneLoaded()
    {
        Debug.Log($"Loading scene data");
        SaveCurrentSceneName();

        try
        {
            // Load game data for that scene if it exists (was modified permanently before)
            if (SaveLoad.Instance.SceneNameToGameObjectsList.TryGetValue(SaveLoad.Instance.CurrentScene, out temporaryGameObjectsList)
                && temporaryGameObjectsList != null
                && temporaryGameObjectsList.Count > 0)
            {
                Debug.Log($"Restoring Game Objects");
                RestoreGameObjects();
            }
            else
            {
                temporaryGameObjectsList = new();
            }
            Debug.Log($"After potential game object data loaded and restored");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }

        OnGameObjectsLoaded?.Invoke();
    }
}
