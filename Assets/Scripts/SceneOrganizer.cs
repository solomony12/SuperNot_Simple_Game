using NUnit.Framework;
using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;
using System;

public class SceneOrganizer : MonoBehaviour
{

    private List<GameObject> temporaryMoveList = new();
    private List<GameObject> permanentMoveList = new();

    // This is in SaveData:
    // A dictionary where key is a scene and values are lists of gameobjects per scene (Dictionary1 sceneToGameObjectsList<sceneName, List<GameObject>>)
    // Dictionary 2 has key of gameObject name and value of tuple (spriteName, position)
    // So use dictionary 1 to first select gameObjects names; then use those names in Dictionary 2 to store/view values

    // TODO: EXTEND THIS TO ALL SPRITE, POSITION, AND SETACTIVE FOR ALLLLLLL METHODS
    // 0. Store the original positions before the scene starts in a Dictionary (we don't need to do this as SaveLoad already has the current ones)
    // 1. The scene plays with any temporary animations. Store those game object names in a temporaryMoveList: [YarnCommand] TemporaryMoveObject(GameObject, Position)
    // 2. If it's a permanent animation, store the (GameObject, Position) in a permanentMoveList: [YarnCommand] PermanentMoveObject(GameObject, Position)
    // 3. When the scene ends, restore to the original positions by looking through original positions
    // 4. Then, loop through permanentList and update any of those game object's positions
    // 5. The SaveProgress() should now be called here or after BUT NOT BEFORE (See EndOfScene method in DialogueCommands script)

    [YarnCommand("TemporaryMoveObject")]
    public void TemporaryMoveObject(string gameObjName, string positionString)
    {
        // Parse to GameObject and Vector3
        var (gameObject, position) = ParseGameObjectAndPosition(gameObjName, positionString);
        // Store GameObject in temporaryMoveList
        temporaryMoveList.Add(gameObject);
        // TODO: Move GameObject to Vector3
    }

    [YarnCommand("PermanentMoveObject")]
    public void PermanentMoveObject(string gameObjName, string positionString)
    {
        // Parse to GameObject and Vector3
        var (gameObject, position) = ParseGameObjectAndPosition(gameObjName, positionString);
        // Store GameObject in permanent MoveList
        permanentMoveList.Add(gameObject);
        // TODO: Move GameObject to Vector3
    }

    /// <summary>
    /// Restores the positions of all game objects that were moved temporarily during a scene
    /// </summary>
    public void RestorePositions()
    {
        foreach (GameObject movedObj in temporaryMoveList)
        {
            // TODO: Restore position of that GameObject to the original position using SaveLoad's gameObjectDetails (string, Vector3, bool)
        }
        // At the end, clear temporaryMoveList
        temporaryMoveList.Clear();
    }

    public void UpdatePermanentPositions()
    {
        // TODO:
        // 
        // At the end, clear permanentMoveList
        permanentMoveList.Clear();
    }

    /// <summary>
    /// Parses strings to GameObject and Vector3
    /// </summary>
    /// <param name="gameObjName">String to parse into GameObject</param>
    /// <param name="positionString">String to parse into Vector3</param>
    /// <returns>(GameObject, Vector3)</returns>
    /// <exception cref="Exception">Incorrect format or wrong name/GameObject found</exception>
    public static (GameObject, Vector3) ParseGameObjectAndPosition(string gameObjName, string positionString)
    {
        // Find the GameObject by name
        GameObject obj = GameObject.Find(gameObjName);
        if (obj == null)
        {
            throw new Exception($"GameObject with name '{gameObjName}' not found.");
        }

        // Parse the position string into Vector3
        Vector3 position = ParseVector3(positionString);

        return (obj, position);
    }

    /// <summary>
    /// Parse string into Vector3
    /// </summary>
    /// <param name="posString">String to be parsed</param>
    /// <returns>Vector3 position</returns>
    private static Vector3 ParseVector3(string posString)
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
}
