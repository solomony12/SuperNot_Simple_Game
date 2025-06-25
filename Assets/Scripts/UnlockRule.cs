using System.Collections.Generic;

[System.Serializable]
public class UnlockRule
{
    // Story State/Part
    public string node;

    // Either requires all states to unlock or just one (AND/OR)
    public List<string> requiresAll;
    public List<string> requiresAny;

    // The character to select for the dialogue to start
    public string startingCharacter;

    // Written out unlock rule for what's needed
    public string rules;

    // Title of the episode/part
    public string title;
    // Description of the story part
    public string description;
}

[System.Serializable]
public class UnlockRulesData
{
    public List<UnlockRule> unlockRules;
}
