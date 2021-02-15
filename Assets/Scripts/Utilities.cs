using UnityEngine;




public class Utilities 
{

    /// <summary>
    /// Makes sure a gameobject and all of it's ancestors will persist across scene changes. 
    /// What is the difference between this and Unity's function? This also makes all of it's 
    /// parents in the heirarchy persist as well, whereas Unity's function does not, aka Unity's
    /// funciton only works on root GameObjects
    /// </summary>
    /// <param name="go">The game object to not be destroyed.</param>
    public static void DontDestroyOnLoad(GameObject go)
    {
        Transform parentTransform = go.transform;

        // If this object doesn't have a parent then its the root transform.
        while (parentTransform.parent != null)
        {
            // Keep going up the chain.
            parentTransform = parentTransform.parent;
        }
        GameObject.DontDestroyOnLoad(parentTransform.gameObject);
    }
}

public static class StringExt
{
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}





