using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : MonoBehaviour
{
   public void PlayButton()
    {
        SceneManager.LoadSceneAsync("HubScene");
 
   }
    public void StopButton() 
    { 
        Application.Quit();
    }
}
