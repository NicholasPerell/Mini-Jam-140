using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;

public class DialogueManger : MonoBehaviour
{

    public TextAsset inkJSON;
    public GameObject textbox;
    public bool isTalking = false;

    private Story story;
    Text message;
    List<string> tags;

    void Start()
    {
        story = new Story(inkJSON.text);
        message = textbox.transform.GetChild(0).GetComponent<Text>();
        tags = new List<string>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (story.canContinue)
            {
                AdvanceDialogue();
            }
        }
    }

    void AdvanceDialogue()
    {
        string currentSentence = story.Continue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }

    IEnumerator TypeSentence (string sentence)
    {
        message.text = "";
        foreach(char letter in sentence.ToCharArray())
        {
            message.text += letter;
            yield return null;
        }
    } 
}
