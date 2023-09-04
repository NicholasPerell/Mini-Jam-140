using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertActions : MonoBehaviour
{
    public GameObject alertIcon;
    public GameObject questionIcon;
    private float timeToAppear = 1f;
    private float timeWhenDisappear;

    public void EnableAlertIcon(bool show)
    {
        Debug.Log("Alert");
        alertIcon.SetActive(show);
        AudioSystem.Instance?.RequestSound("EnemyNoticePlayer01");
        timeWhenDisappear = Time.time + timeToAppear;
    }

    public void EnableQuestionIcon(bool show)
    {
        Debug.Log("Question");
        questionIcon.SetActive(show);
        AudioSystem.Instance?.RequestSound("EnemyNoticeCoin01");
        timeWhenDisappear = Time.time + timeToAppear;
    }

    void Update()
    {
        if (alertIcon && (Time.time >= timeWhenDisappear))
        {
            alertIcon.SetActive(false);
        }
        if (questionIcon && (Time.time >= timeWhenDisappear))
        {
            questionIcon.SetActive(false);
        }
    }
}
