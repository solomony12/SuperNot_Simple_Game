using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueProgressionManager : MonoBehaviour
{
    public static DialogueProgressionManager Instance;

    // Data for story parts unlocking
    private UnlockPartsData unlockPartsData;
    private List<UnlockPart> unlockParts = new();

    // Efficient unlocking new parts lists
    private SortedDictionary<int, UnlockPart> mainStoryPartsByNumber = new();
    private Dictionary<string, SortedDictionary<int, UnlockPart>> characterArcPartsByCharacter = new();
    private Dictionary<string, SortedDictionary<int, UnlockPart>> randomPartsByGroup = new();

    public event Action OnDataInitialized;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        LoadUnlockParts();
        SaveLoad.Instance.LoadProgress();
        //Debug.Log("OnDataInitialized Invoked");
        OnDataInitialized?.Invoke();
        //Debug.Log("After OnDataInitialized Invoked");
    }

    /// <summary>
    /// Update a completed story part
    /// </summary>
    /// <param name="state"> The state/part that has been reached</param>
    public void ReachState(string state)
    {
        // Tries to add to reachedStates
        if (!SaveLoad.Instance.ReachedStates.Add(state))
        {
            // If fails to add, it was already there so return
            return;
        }

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
        // Random
        else
        {
            // Don't save so early return
            return;
        }

        DiscoverNewlyUnlockedParts();

        Debug.Log($"Reached state: {state}");


        // Auto-Save
        SaveLoad.Instance.SaveProgress();
    }

    /// <summary>
    /// Updates the latest main story part
    /// </summary>
    /// <param name="state">Latest main story state/part</param>
    private void UpdateLatestMain(string state)
    {
        // Get the number ID
        if (!TryExtractNumber(state, out int newNum)) return;

        // Add one episode
        newNum += 1;

        // The latest episode now is one up
        // (Example: M05) (The :D2 is for the leading 0 if newNum is a single digit)
        string newLatestMain = $"{ScriptConstants.mainStoryMarkerID}{newNum:D2}";

        // Update main story
        if (SaveLoad.Instance.LatestMainStory == null || TryExtractNumber(SaveLoad.Instance.LatestMainStory, out int currentNum) && newNum > currentNum)
        {
            SaveLoad.Instance.LatestMainStory = newLatestMain;
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
        string[] parts = state.Split('_');
        if (parts.Length < 2 || !TryExtractNumber(parts[0], out int newNum)) return;

        string character = parts[1];

        // Currently the character didn't have their arc ran yet so we add the first one
        if (!SaveLoad.Instance.LatestCharacterArcs.ContainsKey(character))
        {
            SaveLoad.Instance.LatestCharacterArcs[character] = state;
        }

        // Add one episode
        newNum += 1;

        // The latest episode now is one up
        // (Example: C08_SoraHino) (The :D2 is for the leading 0 if newNum is a single digit)
        string newLatestCharArc = $"{ScriptConstants.characterArcStoryMarkerID}{newNum:D2}_{character}";

        // Update character arc for that character
        if (!SaveLoad.Instance.LatestCharacterArcs.TryGetValue(character, out string current) ||
            TryExtractNumber(current.Split('_')[0], out int currentNum) && newNum > currentNum)
        {
            SaveLoad.Instance.LatestCharacterArcs[character] = newLatestCharArc;
        }
    }

    /// <summary>
    /// Creates and stores all parts/nodes in dictionaries for faster unlocking
    /// </summary>
    private void IndexUnlockParts()
    {
        mainStoryPartsByNumber.Clear();
        characterArcPartsByCharacter.Clear();

        foreach (var part in unlockParts)
        {
            if (string.IsNullOrEmpty(part.node))
                continue;

            // Main story
            if (part.node.StartsWith(ScriptConstants.mainStoryMarkerID))
            {
                // Index that part with the number following the ID
                if (TryExtractNumber(part.node, out int num))
                {
                    mainStoryPartsByNumber[num] = part;
                }
            }
            // Character Arc story
            else if (part.node.StartsWith(ScriptConstants.characterArcStoryMarkerID))
            {
                var parts = part.node.Split('_');
                if (parts.Length >= 2 && TryExtractNumber(parts[0], out int num))
                {
                    string character = parts[1];
                    // Make a new dictionary if that character doesn't have one already
                    if (!characterArcPartsByCharacter.ContainsKey(character))
                    {
                        characterArcPartsByCharacter[character] = new SortedDictionary<int, UnlockPart>();
                    }

                    // Index that part with the number following the ID
                    characterArcPartsByCharacter[character][num] = part;
                }
            }
            // Random dialogue
            else
            {
                var parts = part.node.Split('_');
                if (parts.Length >= 2 && TryExtractNumber(parts[0], out int num))
                {
                    string group = parts[1];

                    // Make a new dictionary if that character doesn't have one already
                    if (!randomPartsByGroup.ContainsKey(group))
                    {
                        randomPartsByGroup[group] = new SortedDictionary<int, UnlockPart>();
                    }

                    // Index that part with the number following the ID
                    randomPartsByGroup[group][num] = part;
                }
            }
        }
    }

    /// <summary>
    /// Pull the digit from the state/part name
    /// </summary>
    /// <param name="s">State name</param>
    /// <param name="number">ID for that part</param>
    /// <returns>Successfully pulled out ID</returns>
    public bool TryExtractNumber(string s, out int number)
    {
        string digits = new string(s.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out number);
    }

    /// <summary>
    /// Set new unlocked parts after a scene was finished
    /// </summary>
    private void DiscoverNewlyUnlockedParts()
    {
        // Main story
        if (TryExtractNumber(SaveLoad.Instance.LatestMainStory, out int currentMainNum))
        {
            int nextMainNum = currentMainNum + 1;
            if (mainStoryPartsByNumber.TryGetValue(nextMainNum, out var nextMainPart))
            {
                if (!SaveLoad.Instance.ReachedStates.Contains(nextMainPart.node) && IsNodeUnlocked(nextMainPart.node))
                {
                    SaveLoad.Instance.LatestMainStory = nextMainPart.node;
                    //Debug.Log($"Discovered new main story part: {nextMainPart.node}");
                }
            }
        }

        // Character arcs
        foreach (var kvp in characterArcPartsByCharacter)
        {
            string character = kvp.Key;
            var arcDict = kvp.Value;

            // If we have a latest node already, extract its number, otherwise start from -1 (i.e., check from 0)
            int latestNum = -1;
            if (SaveLoad.Instance.LatestCharacterArcs.TryGetValue(character, out string currentNode))
            {
                if (TryExtractNumber(currentNode.Split('_')[0], out int currentCharNum))
                    latestNum = currentCharNum;
            }

            // Try to find the next part after the latest one (or the first part if none tracked yet)
            int nextNum = latestNum + 1;
            if (arcDict.TryGetValue(nextNum, out var nextCharPart))
            {
                if (!SaveLoad.Instance.ReachedStates.Contains(nextCharPart.node) && IsNodeUnlocked(nextCharPart.node))
                {
                    SaveLoad.Instance.LatestCharacterArcs[character] = nextCharPart.node;
                    //Debug.Log($"Discovered new character arc for {character}: {nextCharPart.node}");
                }
            }
        }

        // Random dialogue
        foreach (var groupEntry in randomPartsByGroup)
        {
            var partsByNum = groupEntry.Value;

            foreach (var kvp in partsByNum)
            {
                var node = kvp.Value.node;
                if (SaveLoad.Instance.ReachedStates.Contains(node)) continue;

                if (IsNodeUnlocked(node))
                {
                    //Debug.Log($"Discovered untracked unlocked random node: {node}");
                }
            }
        }
    }


    public string GetLatestMainStory() => SaveLoad.Instance.LatestMainStory;

    /// <summary>
    /// Gets the latest unlocked main story part as an UnlockPart, or null if none found.
    /// </summary>
    public UnlockPart GetLatestMainStoryPart()
    {
        if (string.IsNullOrEmpty(SaveLoad.Instance.LatestMainStory)) return null;

        if (TryExtractNumber(SaveLoad.Instance.LatestMainStory, out int num) && mainStoryPartsByNumber.TryGetValue(num, out var part))
        {
            if (IsNodeUnlocked(part.node)) return part;
        }

        return null;
    }

    public string GetLatestCharacterArc(string character) =>
        SaveLoad.Instance.LatestCharacterArcs.TryGetValue(character, out string node) ? node : null;

    /// <summary>
    /// Returns a list of the latest unlocked character arc parts for each character.
    /// </summary>
    /// <returns>List of Unlockpart for latest character arcs unlocked</returns>
    public List<UnlockPart> GetAllLatestCharacterArcParts()
    {
        List<UnlockPart> latestParts = new();

        // Get latest arcs for all characters
        foreach (var kvp in SaveLoad.Instance.LatestCharacterArcs)
        {
            string character = kvp.Key;
            string latestNode = kvp.Value;

            // Check if node is unlocked for that latest part
            if (TryExtractNumber(latestNode.Split('_')[0], out int num) &&
                characterArcPartsByCharacter.TryGetValue(character, out var dict) &&
                dict.TryGetValue(num, out var part) &&
                IsNodeUnlocked(part.node))
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
        bool allConditionsMet = part.requiresAll.Count == 0 || part.requiresAll.All(state => SaveLoad.Instance.ReachedStates.Contains(state));
        bool anyConditionMet = part.requiresAny.Count == 0 || part.requiresAny.Any(state => SaveLoad.Instance.ReachedStates.Contains(state));

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
        if (SaveLoad.Instance.LatestCharacterArcs.TryGetValue(characterName, out string latestNode))
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
        if (!string.IsNullOrEmpty(SaveLoad.Instance.LatestMainStory))
        {

            var mainPart = unlockParts.FirstOrDefault(part =>
                part.node == SaveLoad.Instance.LatestMainStory && part.startingScene == currentScene);

            // Found the main part in this scene and it's unlocked
            if (mainPart != null && IsNodeUnlocked(mainPart.node))
            {
                result.Add(mainPart);
            }
        }

        // 2. Add latest character arcs if they're in this scene
        foreach (var kvp in SaveLoad.Instance.LatestCharacterArcs)
        {
            string latestNode = kvp.Value;
            // Found the character part in this scene and it's unlocked
            UnlockPart part = unlockParts.FirstOrDefault(r =>
            r.node == latestNode && r.startingScene == currentScene);
            if (part != null && IsNodeUnlocked(part.node))
            {
                result.Add(part);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets all unlock parts whose node starts with a given story ID
    /// </summary>
    /// <param name="id">The story ID to filter nodes by</param>
    /// <returns>List of matching UnlockParts</returns>
    public List<UnlockPart> GetPartsByStoryID(string id)
    {
        // Nothing
        if (string.IsNullOrEmpty(id))
        {
            return new List<UnlockPart>();
        }

        return unlockParts
            .Where(part => !string.IsNullOrEmpty(part.node) && part.node.StartsWith(id)).ToList();
    }

    /// <summary>
    /// Gets all UnlockParts associated with a specific character/object name
    /// </summary>
    /// <param name="name">The name of the character or object</param>
    /// <returns>List of matching UnlockParts</returns>
    public List<UnlockPart> GetAllCharacterArcPartsForName(string name)
    {
        // Nothing
        if (string.IsNullOrEmpty(name))
        {
            return new List<UnlockPart>();
        }

        return unlockParts
            .Where(part =>
                !string.IsNullOrEmpty(part.node) &&
                part.node.StartsWith(ScriptConstants.characterArcStoryMarkerID) && // starts with "C"
                part.node.EndsWith("_" + name)) // ends with _Name
            .ToList();
    }

    /// <summary>
    /// Gets all UnlockParts whose node starts with a given story ID and ends with the given character/object name.
    /// </summary>
    /// <param name="id">The story ID </param>
    /// <param name="name">The character or object name</param>
    /// <returns>List of matching UnlockParts</returns>
    public List<UnlockPart> GetPartsByIDAndName(string id, string name)
    {
        // Defensive null/empty check
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
        {
            return new List<UnlockPart>();
        }

        return unlockParts
            .Where(part =>
                !string.IsNullOrEmpty(part.node) &&
                part.node.StartsWith(id) &&
                part.node.EndsWith("_" + name))
            .ToList();
    }

    /// <summary>
    /// Gets all unlocked UnlockParts whose node starts with the given story ID and ends with the given character/object name
    /// </summary>
    /// <param name="id">The story ID to filter nodes by</param>
    /// <param name="name">The character or object name to filter nodes by</param>
    /// <returns>A list of matching UnlockParts that are unlocked</returns>
    public List<UnlockPart> GetUnlockedPartsByIDAndName(string id, string name)
    {
        // Defensive null/empty check
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
        {
            return new List<UnlockPart>();
        }

        return unlockParts
            .Where(part =>
                !string.IsNullOrEmpty(part.node) &&
                part.node.StartsWith(id) &&
                part.node.EndsWith("_" + name) &&
                IsNodeUnlocked(part.node))
            .ToList();
    }



    // Load parts

    /// <summary>
    /// Load in the unlock parts
    /// </summary>

    public void LoadUnlockParts()
    {
        string path = Path.Combine(Application.streamingAssetsPath, ScriptConstants.dialogueUnlockRulesString);

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
        IndexUnlockParts();
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