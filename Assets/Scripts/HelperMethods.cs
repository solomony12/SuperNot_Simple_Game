using System;
using UnityEngine;
using UnityEngine.UI;

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
    /// Parse string into Vector2
    /// </summary>
    /// <param name="posString">String to be parsed</param>
    /// <param name="canvasRect">Canvas of the game</param>
    /// <returns>Vector2 position</returns>
    public static Vector2 ParseVector2OrDefaultZero(string posString)
    {
        if (string.IsNullOrEmpty(posString))
        {
            Debug.LogError("Position string is null or empty. Using Vector2.zero");
            return Vector2.zero;
        }

        string[] parts = posString.Split(',');
        if (parts.Length != 2)
        {
            Debug.LogError($"Invalid position string format: '{posString}'. Expected format 'x,y'. Using Vector2.zero");
            return Vector2.zero;
        }

        if (!float.TryParse(parts[0].Trim(), out float x) || !float.TryParse(parts[1].Trim(), out float y))
        {
            Debug.LogError($"Failed to parse position components from '{posString}'. Using Vector2.zero");
            return Vector2.zero;
        }

        // Since the position is relative to BottomPanel's anchor,
        // just return the parsed vector directly for anchoredPosition.
        return new Vector2(x, y);
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
