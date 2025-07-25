using System.Collections.Generic;

[System.Serializable]
public class UnlockPart
{
    // Story State/Part
    public string node;

    // Either requires all states to unlock or just one (AND/OR)
    public List<string> requiresAll;
    public List<string> requiresAny;

    // The character to select for the dialogue to start
    public string startingCharacter;
    // The scene it's in for the story marker to show
    public string startingScene;

    // Written out unlock rule for what's needed
    public string rules;

    // Title of the episode/part
    public string title;
    // Description of the story part
    public string description;
}

[System.Serializable]
public class UnlockPartsData
{
    public List<UnlockPart> unlockParts;
}
