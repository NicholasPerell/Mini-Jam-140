using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    GameObject main;
    [SerializeField]
    GameObject credits, controls, options;

    public event UnityAction onPlayRequested;

    private void Awake()
    {
        OpenMain();
    }

    private void ClearOutPanels()
    {
        main.SetActive(false);
        credits.SetActive(false);
        controls.SetActive(false);
        options.SetActive(false);
    }

    private void OpenPanel(GameObject panel)
    {
        ClearOutPanels();
        panel.SetActive(true);
    }

    public void OpenMain() => OpenPanel(main);
    public void OpenCredits() => OpenPanel(credits);
    public void OpenOptions() => OpenPanel(options);
    public void OpenControls() => OpenPanel(controls);

    public void RespondToClickPlay()
    {
        //TODO:
        onPlayRequested?.Invoke();
    }

    public void RespondToClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
