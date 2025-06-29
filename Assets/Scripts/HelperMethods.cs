using System;
using UnityEngine;

public static class HelperMethods
{
    /// <summary>
    /// Checks to see if <paramref name="parent"/> has a child with tag <paramref name="tag"/>
    /// </summary>
    /// <param name="parent">The game object</param>
    /// <param name="tag">The tag to search for on its children</param>
    /// <returns>True if <paramref name="parent"/> has a child with <paramref name="tag"/></returns>
    public static bool HasChildWithTag(GameObject parent, string tag)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Parse string into GameObject by finding the GameObject with that name
    /// </summary>
    /// <param name="gameObjStr">String to find GameObject</param>
    /// <returns>GameObject</returns>
    /// <exception cref="Exception">No GameObject with name of <paramref name="gameObjStr"/> found</exception>
    public static GameObject ParseGameObject(string gameObjStr)
    {
        GameObject obj = GameObject.Find(gameObjStr);
        if (obj == null)
        {
            throw new Exception($"GameObject with name '{gameObjStr}' not found.");
        }

        return obj;
    }

    /// <summary>
    /// Parse string into Vector3
    /// </summary>
    /// <param name="posString">String to be parsed</param>
    /// <returns>Vector3 position</returns>
    public static Vector3 ParseVector3DefaultZero(string posString)
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
    public static float ParseFloatDefaultOne(string floatString)
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
