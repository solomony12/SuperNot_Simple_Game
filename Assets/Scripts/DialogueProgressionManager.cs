using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DialogueProgressionManager : MonoBehaviour
{
    // Saving
    private const string SaveFileName = "progression_save.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    // Data for story parts unlocking
    private UnlockRulesData unlockRulesData;

    // Story parts reached
    private HashSet<string> reachedStates = new();

    // Latest story parts
    private string latestMainStory;
    private Dictionary<string, string> latestCharacterArcs = new();

    public void Start()
    {
        LoadUnlockRules();
        LoadProgress();
    }

    /// <summary>
    /// Update a completed story part
    /// </summary>
    /// <param name="state"> The state/part that has been reached</param>
    public void ReachState(string state)
    {
        if (!reachedStates.Add(state)) return;

        Debug.Log($"Reached state: {state}");

        // Main Story
        if (state.StartsWith("M"))
        {
            UpdateLatestMain(state);
        }
        // Character Arc Story
        else if (state.StartsWith("C"))
        {
            UpdateLatestCharacter(state);
        }

        // Auto-Save
        SaveProgress();
    }

    /// <summary>
    /// Updates the latest main story part
    /// </summary>
    /// <param name="state">Latest main story state/part</param>
    private void UpdateLatestMain(string state)
    {
        // Get the number ID
        if (!TryExtractNumber(state, out int newNum)) return;

        // Update main story
        if (latestMainStory == null || TryExtractNumber(latestMainStory, out int currentNum) && newNum > currentNum)
        {
            latestMainStory = state;
        }
    }

    /// <summary>
    /// Updates the latest character arc part for that character
    /// </summary>
    /// <param name="state">Latest character arc state/part</param>
    private void UpdateLatestCharacter(string state)
    {
        // 0: C#, 1: Name
        // (Example: C3_HarutoSakuma)
        var parts = state.Split('_');
        if (parts.Length < 2 || !TryExtractNumber(parts[0], out int newNum)) return;

        string character = parts[1];
        // Update character arc for that character
        if (!latestCharacterArcs.TryGetValue(character, out string current) ||
            TryExtractNumber(current.Split('_')[0], out int currentNum) && newNum > currentNum)
        {
            latestCharacterArcs[character] = state;
        }
    }

    /// <summary>
    /// Pull the digit from the state/part name
    /// </summary>
    /// <param name="s">State name</param>
    /// <param name="number">ID for that part</param>
    /// <returns>Successfully pulled out ID</returns>
    private bool TryExtractNumber(string s, out int number)
    {
        string digits = new string(s.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out number);
    }

    public string GetLatestMainStory() => latestMainStory;

    public string GetLatestCharacterArc(string character) =>
        latestCharacterArcs.TryGetValue(character, out string node) ? node : null;


    // SAVE & LOAD

    /// <summary>
    /// Save current unlocked states of the game in a JSON file
    /// </summary>
    public void SaveProgress()
    {
        // Save data
        var saveData = new DialogueProgressionSaveData
        {
            reachedStates = reachedStates.ToList(),
            latestMainStory = latestMainStory,
            latestCharacterArcs = latestCharacterArcs
                .Select(kvp => new CharacterArcEntry { character = kvp.Key, node = kvp.Value })
                .ToList()
        };

        // Save to JSON
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Progression saved.");
    }

    /// <summary>
    /// Load current unlocked states of the game from a JSON file
    /// </summary>
    public void LoadProgress()
    {
        if (!File.Exists(SavePath)) return;

        string json = File.ReadAllText(SavePath);
        var saveData = JsonUtility.FromJson<DialogueProgressionSaveData>(json);

        // Load reached states and latest unlocked parts
        reachedStates = new HashSet<string>(saveData.reachedStates);
        latestMainStory = saveData.latestMainStory;
        latestCharacterArcs = new Dictionary<string, string>();

        // Load character arc nodes
        foreach (var arc in saveData.latestCharacterArcs)
        {
            latestCharacterArcs[arc.character] = arc.node;
        }

        Debug.Log("Progression loaded.");
    }

    /// <summary>
    /// Load in the unlock rules
    /// </summary>
    private void LoadUnlockRules()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "DialogueUnlockRules.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            unlockRulesData = JsonUtility.FromJson<UnlockRulesData>(json);
            Debug.Log($"Loaded {unlockRulesData.unlockRules.Count} unlock rules.");
        }
        else
        {
            Debug.LogError("DialogueUnlockRules.json not found!");
            unlockRulesData = new UnlockRulesData { unlockRules = new List<UnlockRule>() };
        }
    }
}

// Embedded save data classes
[System.Serializable]
public class DialogueProgressionSaveData
{
    public List<string> reachedStates;
    public string latestMainStory;
    public List<CharacterArcEntry> latestCharacterArcs;
}

[System.Serializable]
public class CharacterArcEntry
{
    public string character;
    public string node;
}
