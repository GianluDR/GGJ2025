using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void ExitButton(){
        Application.Quit();
        Debug.Log("Game closed");
    }

    public void MenuButton(){
        SceneManager.LoadScene("Menu");
    }

    public void StartButton(){
        SceneManager.LoadScene("Game");
    }
}
