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
}
