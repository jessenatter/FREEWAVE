using UnityEngine;

// 1. MUST be marked [System.Serializable] so Unity can populate fields
// 2. Kept outside the MonoBehaviour class so JsonUtility can see them
[System.Serializable]
public class DialogueLine
{
    public string speaker;
    public string text;
}

[System.Serializable]
public class DialogueData
{
    public DialogueLine[] lines; 
}

public class DialogueManager : MonoBehaviour
{
    private DialogueData currentDialogue;

    void Start()
    {
        LoadDialogue();
    }

    void LoadDialogue()
    {
        // Path matches: Assets/Resources/Dialouge/dialouge.json
        TextAsset jsonFile = Resources.Load<TextAsset>("Dialouge/dialouge");

        if (jsonFile == null)
        {
            Debug.LogError("Could not find dialogue file in Resources/Dialouge/dialouge!");
            return;
        }

        currentDialogue = JsonUtility.FromJson<DialogueData>(jsonFile.text);

        if (currentDialogue != null && currentDialogue.lines != null)
        {
            foreach (DialogueLine line in currentDialogue.lines)
            {
                Debug.Log($"{line.speaker}: {line.text}");
            }
        }
    }
}