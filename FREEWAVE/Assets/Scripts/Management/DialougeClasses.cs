using System;
using System.Collections.Generic;
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
    public List<DialogueLine[]> conversations = new List<DialogueLine[]>();

    public static DialogueData FromJson(string json)
    {
        DialogueData data = new DialogueData();
        data.LoadSequentialConversations(json);
        return data;
    }

    void LoadSequentialConversations(string json)
    {
        int convoIndex = 1;

        while (true)
        {
            string convoKey = $"\"convo{convoIndex}\"";
            int keyIndex = json.IndexOf(convoKey, StringComparison.Ordinal);

            if (keyIndex < 0)
                break;

            int colonIndex = json.IndexOf(':', keyIndex + convoKey.Length);
            int arrayStart = json.IndexOf('[', colonIndex + 1);

            if (colonIndex < 0 || arrayStart < 0)
                break;

            int arrayEnd = FindMatchingArrayEnd(json, arrayStart);
            if (arrayEnd < 0)
                break;

            string convoArrayJson = json.Substring(arrayStart, arrayEnd - arrayStart + 1);
            DialogueLineArrayWrapper wrapper = JsonUtility.FromJson<DialogueLineArrayWrapper>($"{{\"lines\":{convoArrayJson}}}");

            if (wrapper != null && wrapper.lines != null)
                conversations.Add(wrapper.lines);
            else
                conversations.Add(new DialogueLine[0]);

            convoIndex++;
        }
    }

    static int FindMatchingArrayEnd(string json, int arrayStart)
    {
        int depth = 0;
        bool inString = false;
        bool escaped = false;

        for (int i = arrayStart; i < json.Length; i++)
        {
            char c = json[i];

            if (inString)
            {
                if (escaped)
                    escaped = false;
                else if (c == '\\')
                    escaped = true;
                else if (c == '"')
                    inString = false;

                continue;
            }

            if (c == '"')
            {
                inString = true;
                continue;
            }

            if (c == '[')
                depth++;
            else if (c == ']')
            {
                depth--;
                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }

    [System.Serializable]
    class DialogueLineArrayWrapper
    {
        public DialogueLine[] lines;
    }
}