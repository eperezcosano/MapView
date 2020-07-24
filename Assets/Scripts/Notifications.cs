using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Notifications : MonoBehaviour
{

    [SerializeField]
    public string sunset = "";
    [SerializeField]
    public string sunrise = "";

    void Start()
    {
        #if UNITY_IOS
        UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound);
        #endif
    }

    void OnApplicationPause(bool pause)
    {

        #if UNITY_IOS
        if (pause && sunrise != "" && sunset != "")
        {
            StartCoroutine("LaunchNotification");
        }   
        #endif

    }

    public void LaunchNotification()
    {
        UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
        UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
        DateTime dateToNotify = DateTime.Now.AddSeconds(10);
        UnityEngine.iOS.LocalNotification notif = new UnityEngine.iOS.LocalNotification
        {
            fireDate = dateToNotify,
            alertTitle = "Solar times",
            alertBody = "Sunset:" + sunset + "\nSunrise: " + sunrise,
        };
        UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(notif);
    }
}
