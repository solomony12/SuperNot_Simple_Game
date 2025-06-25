using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueProgressionManager : MonoBehaviour
{
    // Saving
    private const string SaveFileName = "progression_save.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    // Data for story parts unlocking
    private UnlockPartsData unlockPartsData;
    private List<UnlockPart> unlockParts = new();

    // Story parts reached
    private HashSet<string> reachedStates = new();

    // Latest story parts
    private string latestMainStory;
    private Dictionary<string, string> latestCharacterArcs = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        LoadUnlockParts();
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
        if (state.StartsWith(ScriptConstants.mainStoryMarkerID))
        {
            UpdateLatestMain(state);
        }
        // Character Arc Story
        else if (state.StartsWith(ScriptConstants.characterArcStoryMarkerID))
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

    /// <summary>
    /// Gets the latest unlocked main story part as an UnlockPart, or null if none found.
    /// </summary>
    public UnlockPart GetLatestMainStoryPart()
    {
        if (string.IsNullOrEmpty(latestMainStory))
            return null;

        var mainPart = unlockParts.FirstOrDefault(part =>
            part.node == latestMainStory &&
            !string.IsNullOrEmpty(part.node) &&
            part.node.StartsWith(ScriptConstants.mainStoryMarkerID) &&
            IsNodeUnlocked(part.node));

        return mainPart;
    }

    public string GetLatestCharacterArc(string character) =>
        latestCharacterArcs.TryGetValue(character, out string node) ? node : null;

    /// <summary>
    /// Returns a list of the latest unlocked character arc parts for each character.
    /// </summary>
    /// <returns>List of Unlockpart for latest character arcs unlocked</returns>
    public List<UnlockPart> GetAllLatestCharacterArcParts()
    {
        List<UnlockPart> latestParts = new();

        foreach (var kvp in latestCharacterArcs)
        {
            string latestNode = kvp.Value;
            var part = unlockParts.FirstOrDefault(r => r.node == latestNode);

            if (part != null && IsNodeUnlocked(part.node))
            {
                latestParts.Add(part);
            }
        }

        return latestParts;
    }

    /// <summary>
    /// Checks if <paramref name="storyParts"/> contains a main story part
    /// </summary>
    /// <param name="storyParts">List of story parts</param>
    /// <returns>True if contains a main story part</returns>
    public bool HasMainStory(List<UnlockPart> storyParts)
    {
        return storyParts.Any(part =>
            !string.IsNullOrEmpty(part.node) &&
            part.node.StartsWith(ScriptConstants.mainStoryMarkerID));
    }

    /// <summary>
    /// Checks if <paramref name="storyParts"/> contains a character arc story part
    /// </summary>
    /// <param name="storyParts">List of story parts</param>
    /// <returns>True if contains a character arc story part</returns>
    public bool HasCharacterArcStory(List<UnlockPart> storyParts)
    {
        return storyParts.Any(part =>
            !string.IsNullOrEmpty(part.node) &&
            part.node.StartsWith(ScriptConstants.characterArcStoryMarkerID));
    }

    /// <summary>
    /// Check to see if a state/part is unlocked or not
    /// </summary>
    /// <param name="node">State/part to check if it's unlocked</param>
    /// <returns>True if unlocked</returns>
    public bool IsNodeUnlocked(string node)
    {
        var part = unlockParts.FirstOrDefault(r => r.node == node);

        // If no part exists, assume it's unlocked by default
        if (part == null)
        {
            return true;
        }

        // Conditions
        bool allConditionsMet = part.requiresAll.Count == 0 || part.requiresAll.All(state => reachedStates.Contains(state));
        bool anyConditionMet = part.requiresAny.Count == 0 || part.requiresAny.Any(state => reachedStates.Contains(state));

        return allConditionsMet && anyConditionMet;
    }

    /// <summary>
    /// Gets the latest unlocked story part for <paramref name="characterName"/> in the current scene.
    /// </summary>
    /// <param name="characterName">The character to check</param>
    /// <returns>The latest unlocked story part for that character in the current scene, or null if none</returns>
    public UnlockPart GetLatestPartForCharacter(string characterName)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Check if there's a recorded latest character arc for this character
        if (latestCharacterArcs.TryGetValue(characterName, out string latestNode))
        {
            // Find the matching part for this character, scene, and node
            var part = unlockParts.FirstOrDefault(r =>
                r.node == latestNode &&
                r.startingCharacter == characterName &&
                r.startingScene == currentScene &&
                IsNodeUnlocked(r.node));

            return part; // may be null if scene doesn't match
        }

        return null;
    }


    /// <summary>
    /// Gets all latest unlocked stories in the current scene
    /// </summary>
    /// <returns>List of latest story parts</returns>
    public List<UnlockPart> GetLatestStoryPartsInScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        List<UnlockPart> result = new();

        // 1. Add latest main story if it's in this scene
        if (!string.IsNullOrEmpty(latestMainStory))
        {

            var mainPart = unlockParts.FirstOrDefault(part =>
                part.node == latestMainStory && part.startingScene == currentScene);

            // Found the main part in this scene and it's unlocked
            if (mainPart != null && IsNodeUnlocked(mainPart.node))
            {
                result.Add(mainPart);
            }
        }

        // 2. Add latest character arcs if they're in this scene
        foreach (var kvp in latestCharacterArcs)
        {
            string latestNode = kvp.Value;
            UnlockPart part = unlockParts.FirstOrDefault(r =>
            r.node == latestNode && r.startingScene == currentScene);
            if (part != null && IsNodeUnlocked(part.node))
            {
                result.Add(part);
            }
        }

        return result;
    }



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
        Debug.Log("Progression saved");
        Debug.Log("Save path: " + Application.persistentDataPath);
    }

    /// <summary>
    /// Load current unlocked states of the game from a JSON file
    /// </summary>
    public void LoadProgress()
    {
        // New game - set initial main story part here
        if (!File.Exists(SavePath))
        {
            latestMainStory = ScriptConstants.startingStoryID;
            reachedStates = new HashSet<string>();
            latestCharacterArcs = new Dictionary<string, string>();
            Debug.Log("No save found. Starting new game with initial main story part");
            return;
        }

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

        Debug.Log("Progression loaded");
    }

    /// <summary>
    /// Load in the unlock parts
    /// </summary>

    public void LoadUnlockParts()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "DialogueUnlockParts.json");

    #if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(LoadPartsAndroid(path));
    #else
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<UnlockPartsData>(json);
            unlockParts = data.unlockParts;
            Debug.Log($"Loaded {unlockParts.Count} total parts");
            /*foreach (var p in unlockParts)
            {
                Debug.Log($"Part Node: {p.node}, StartingCharacter: {p.startingCharacter}, Scene: {p.startingScene}, RequiresAll: [{string.Join(", ", p.requiresAll ?? new List<string>())}], RequiresAny: [{string.Join(", ", p.requiresAny ?? new List<string>())}]");
            }*/
        }
    #endif
    }

    /// <summary>
    /// Load in the unlock parts for Android devices
    /// </summary>
    /// <param name="path">Path to JSON</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator LoadPartsAndroid(string path)
    {
        using UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(path);
        yield return www.SendWebRequest();

        if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text;
            var data = JsonUtility.FromJson<UnlockPartsData>(json);
            unlockParts = data.unlockParts;
            Debug.Log($"Loaded {unlockParts.Count} total parts (Android)");
        }
        else
        {
            Debug.LogError("Failed to load unlock parts from StreamingAssets");
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
