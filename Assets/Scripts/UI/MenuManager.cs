using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour, IPointerEnterHandler
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

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        // play sound when mouse hover a btn
        FindObjectOfType<AudioManager>().Play("OnHoverMenu");
    }
}
