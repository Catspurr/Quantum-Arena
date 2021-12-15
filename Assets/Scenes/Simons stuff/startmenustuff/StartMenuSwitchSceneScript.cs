using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuSwitchSceneScript : MonoBehaviour
{
    
    void LoadthisScene()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
