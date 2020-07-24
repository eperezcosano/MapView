using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GpsHandler : MonoBehaviour
{
#if UNITY_EDITOR
    void Start()
    {
        GameObject objectm = GameObject.Find("MapHandler");
        objectm.SendMessage("DownloadMap");
    }
#else
    IEnumerator Start()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield break;
        // Start service before querying location
        Input.location.Start();
        Input.compass.enabled = true;

        // Wait until service initializes
        int maxWait = 5;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in X seconds
        if (maxWait < 1)
        {
            print("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            GameObject objectM = GameObject.Find("MapHandler");
            objectM.SendMessage("DownloadMap");
        }
    }
#endif
}