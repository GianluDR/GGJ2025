using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class Credits : MonoBehaviour
{
    public void ExitButton(){
        Application.Quit();
        Debug.Log("Game closed");
    }

    public void MenuButton(){
        SceneManager.LoadScene("Menu");
    }

    public void StartButton(){
        SceneManager.LoadScene("MainLevel");
    }
}
