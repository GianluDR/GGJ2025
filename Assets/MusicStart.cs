using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<AudioManager>().StopPlaying("Background_Music#2");
        FindObjectOfType<AudioManager>().Play("Background_Music#1");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
