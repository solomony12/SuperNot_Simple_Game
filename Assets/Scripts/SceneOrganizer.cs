using NUnit.Framework;
using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;
using System;

public class SceneOrganizer : MonoBehaviour
{

    private List<GameObject> temporaryObjList = new();
    // TODO: any one of the three values could be empty so we need to account for that when animating
    private List<GameObject> permanentGameObjectsList = new();
    private Dictionary<GameObject, (string, Vector3, bool)> permanentObjListData = new();

    // This is in SaveData:
    // A dictionary where key is a scene and values are lists of gameobjects per scene (Dictionary1 sceneToGameObjectsList<sceneName, List<GameObject>>)
    // Dictionary 2 has key of gameObject name and value of tuple (spriteName, position)
    // So use dictionary 1 to first select gameObjects names; then use those names in Dictionary 2 to store/view values

    // TODO: EXTEND THIS TO ALL SPRITE, POSITION, AND SETACTIVE FOR ALLLLLLL METHODS
    // 0. Store the original positions before the scene starts in a Dictionary (we don't need to do this as SaveLoad already has the current ones)
    // 1. The scene plays with any temporary animations. Store those game object names in a temporaryObjList: [YarnCommand] TemporaryUpdateObject(GameObject, Position)
    // 2. If it's a permanent animation, store the (GameObject, Position) in a permanentObjList: [YarnCommand] PermanentUpdateObject(GameObject, Position)
    // 3. When the scene ends, restore to the original positions by looking through original positions
    // 4. Then, loop through permanentList and update any of those game object's positions
    // 5. The SaveProgress() should now be called here or after BUT NOT BEFORE (See EndOfScene method in DialogueCommands script)

    [YarnCommand("TemporaryUpdateObject")]
    public void TemporaryUpdateObject(string gameObjName, string spriteImageName, string positionString, string durationString, string shouldBeEnabledString)
    {
        // Parse to GameObject and Vector3
        var (gameObject, position, duration) = ParseGameObjectAndPositionAndDuration(gameObjName, positionString, durationString);
        // Parse to bool
        bool shouldBeEnabled = shouldBeEnabledString.Equals(true.ToString());

        // Store GameObject in temporaryObjList
        temporaryObjList.Add(gameObject);

        // TODO: Update GameObject
    }

    [YarnCommand("PermanentUpdateObject")]
    public void PermanentUpdateObject(string gameObjName, string spriteImageName, string positionString, string durationString, string shouldBeEnabledString)
    {
        // Parse to GameObject and Vector3
        var (gameObject, position, duration) = ParseGameObjectAndPositionAndDuration(gameObjName, positionString, durationString);
        // Parse to bool
        bool shouldBeEnabled = shouldBeEnabledString.Equals(true.ToString());

        // Store data in permanentObjList for that object
        permanentGameObjectsList.Add(gameObject);
        permanentObjListData.Add(gameObject, (spriteImageName, position, shouldBeEnabled));

        // TODO: Update GameObject
    }

    // TODO :If animation is interrupted by next line/click before duration is up, automatically finish the position movement
    // Note: We will move them relative to the given position, which means additive.
    // CurrentVector = OldVector + NewVector rather than CurrentVector = OldVector -> NewVector
    //public void UpdateGameObject()

    /// <summary>
    /// Restores all game objects that were updated temporarily during a scene
    /// </summary>
    public void RestoreGameObjects()
    {
        foreach (GameObject movedObj in temporaryObjList)
        {
            // TODO: Restore position of that GameObject to the original data using SaveLoad's gameObjectDetails (string, Vector3, bool)
        }

        // At the end, clear temporaryObjList
        temporaryObjList.Clear();
    }

    /// <summary>
    /// Updates all game objects that need to be the new starting point of all future scenes until further permanent updates
    /// NOTE: It's possible to have a temporary move after a permanent move, hence why we need to update permanent moves after a scene regardlessly
    /// </summary>
    public void PermanentlyChangeGameObjects()
    {
        foreach (GameObject movedObj in permanentGameObjectsList)
        {
            // TODO: Update that GameObject to the original position using permanentObjListData
        }

        // At the end, clear lists
        permanentGameObjectsList.Clear();
        permanentObjListData.Clear();
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
        GameObject obj = GameObject.Find(gameObjName);
        if (obj == null)
        {
            throw new Exception($"GameObject with name '{gameObjName}' not found.");
        }

        // Parse the position string into Vector3
        Vector3 position = ParseVector3DefaultZero(positionString);

        // Parse the duration string into float
        float duration = ParseFloatDefaultOne(durationString);

        return (obj, position, duration);
    }

    /// <summary>
    /// Parse string into Vector3
    /// </summary>
    /// <param name="posString">String to be parsed</param>
    /// <returns>Vector3 position</returns>
    private static Vector3 ParseVector3DefaultZero(string posString)
    {
        if (string.IsNullOrEmpty(posString))
        {
            Debug.LogError("Position string is null or empty. Using Vector3.zero");
            return Vector3.zero;
        }

        string[] parts = posString.Split(',');
        if (parts.Length != 3)
        {
            Debug.LogError($"Invalid position string format: '{posString}'. Expected format 'x,y,z'. Using Vector3.zero");
            return Vector3.zero;
        }

        if (float.TryParse(parts[0], out float x) &&
            float.TryParse(parts[1], out float y) &&
            float.TryParse(parts[2], out float z))
        {
            return new Vector3(x, y, z);
        }
        else
        {
            Debug.LogError($"Failed to parse position components from '{posString}'. Using Vector3.zero");
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Parse string into float
    /// </summary>
    /// <param name="floatString">String to be parsed into</param>
    /// <returns>Float</returns>
    private static float ParseFloatDefaultOne(string floatString)
    {
        if (float.TryParse(floatString, out float number))
        {
            return number;
        }
        else
        {
            // Backup if failed to parse
            Debug.LogWarning($"Failed to parse '{floatString}' as float. Returning 1.");
            return 1f;
        }
    }
}
