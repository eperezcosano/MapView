using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    [SerializeField]
    Button button;

    void Start()
    {
        button.onClick.AddListener(SwitchScene);
    }

    public void SwitchScene()
    {
        if (SceneManager.GetActiveScene().name.Contains("CameraScene"))
        {
            SceneManager.LoadScene("MapView");
        }
        else
        {
            SceneManager.LoadScene("CameraScene");
        }
    }
}
