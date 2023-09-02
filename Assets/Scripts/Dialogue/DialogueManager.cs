using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public string[] dialogueLines;
    private int currentLine = 0;
    private bool isDialogueActive = true;

    void Start()
    {
        dialogueText.text = "";
    }

    void Update()
    {
        if (isDialogueActive)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DisplayNextLine();
            }
        }
    }

    public void StartDialogue()
    {

        isDialogueActive = true;
        currentLine = 0;
        DisplayNextLine();
    }

    void DisplayNextLine()
    {
        if (currentLine < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLine];
            currentLine++;
        }
        else
        {
            currentLine = 0;
            DisplayNextLine();
        }
    }

    void EndDialogue()
    {
        dialogueText.text = "";
        isDialogueActive = false;
    }
}
