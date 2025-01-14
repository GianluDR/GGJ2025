using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour, IPointerEnterHandler
{
    public TMP_Dropdown ResDropDown;
    public Toggle FullScreenToggle;
    
    private bool isFullScreen;
    Resolution[] AllResolutions;
    int SelectedResolution;
    List<Resolution> SelectedResolutionList = new List<Resolution>();

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

    void Start()
    {
        isFullScreen = true;
        AllResolutions = Screen.resolutions;
    
        List<string> resolutionStringList = new List<string>();
        string newRes;

        // Monitor aspect ratio
        float monitorAspect = (float)Screen.currentResolution.width / Screen.currentResolution.height;
        Debug.Log("Monitor Aspect Ratio: " + monitorAspect);

        foreach (Resolution res in AllResolutions)
        {
            float resAspect = (float)res.width / res.height;
            if (Mathf.Abs(resAspect - monitorAspect) < 0.01f) 
            {
                newRes = res.width.ToString() + " x " + res.height.ToString();
                if(!resolutionStringList.Contains(newRes))
                {
                    resolutionStringList.Add(newRes);
                    SelectedResolutionList.Add(res);
                }
            }
        }

        ResDropDown.AddOptions(resolutionStringList);

        AdjustCameraToResolution();
    }

    public void ChangeResolution()
    {
        SelectedResolution = ResDropDown.value;
        Screen.SetResolution(
            SelectedResolutionList[SelectedResolution].width,
            SelectedResolutionList[SelectedResolution].height,
            isFullScreen);
    }

    public void ChangeFullScreen()
    {
        isFullScreen = FullScreenToggle.isOn;
        Debug.Log(isFullScreen);
        Screen.SetResolution(
            SelectedResolutionList[SelectedResolution].width,
            SelectedResolutionList[SelectedResolution].height,
            isFullScreen);
    }

    void AdjustCameraToResolution()
    {
        // Calcola l'aspect ratio corrente
        float targetAspect = (float)Screen.currentResolution.width / Screen.currentResolution.height;
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera camera = Camera.main;

        if (scaleHeight < 1.0f)
        {
            // Bordi neri sopra e sotto
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            camera.rect = rect;
        }
        else
        {
            // Bordi neri a sinistra e destra
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = camera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            camera.rect = rect;
        }
    }

}
