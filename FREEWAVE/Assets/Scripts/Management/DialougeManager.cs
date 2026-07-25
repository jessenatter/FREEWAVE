using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    List<DialogueLine[]> conversations = new List<DialogueLine[]>();
    private DialogueData currentDialogue;

    TextMeshProUGUI text;

    int conversationIndex = 0,lineIndex = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        LoadDialogue();
        text = Manager.Instance.dialougeCanvas.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
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

    public void UpdateDialouge() //itterate to next line in current conversation
    {
        DialogueLine[] currentConversation = conversations[conversationIndex];

        if(lineIndex + 1 >= currentConversation.Length)
        {
            ResumeGameplay();
            lineIndex = 0;
            return;
        }

        lineIndex += 1;
        string nextDialougeLine = "";
        nextDialougeLine = currentConversation[lineIndex].text;
        text.text = nextDialougeLine;
        Manager.Instance.dialougeCanvas.SetActive(true);
    }

    void ResumeGameplay()
    {
        Manager.Instance.player.currentCharacterState = Character.characterState.movement;
        Manager.Instance.dialougeCanvas.SetActive(false);
    }
}