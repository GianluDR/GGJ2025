using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButtonsSounds : MonoBehaviour, IPointerEnterHandler
{
    bool firstTime;
    string ButtonName;
    public Button thisButton;

    void Awake()
    {
        ButtonName = gameObject.name;
    }

    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (ButtonName == "BtnStart" || ButtonName == "BtnSettings" || ButtonName == "BtnCredits" || ButtonName == "BtnQuit")
            FindObjectOfType<AudioManager>().Play("OnHoverMenu");
    }


    public void quit()
    {
        Application.Quit();
    }
}
