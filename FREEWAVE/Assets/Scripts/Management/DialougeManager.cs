using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    List<DialogueLine[]> conversations = new List<DialogueLine[]>();
    private DialogueData currentDialogue;

    void Start()
    {
        LoadDialogue();
    }

    void LoadDialogue()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        TextAsset jsonFile = Resources.Load<TextAsset>($"Dialouge/{sceneName}");
        if (jsonFile == null)
            return;

        currentDialogue = DialogueData.FromJson(jsonFile.text);
        conversations.Clear();
        conversations.AddRange(currentDialogue.conversations);

        foreach (DialogueLine[] conversation in conversations)
        {
            foreach (DialogueLine line in conversation)
            {
                Debug.Log($"{line.speaker}: {line.text}");
            }
        }
    }
}