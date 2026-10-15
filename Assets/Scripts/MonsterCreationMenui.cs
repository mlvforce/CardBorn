using UnityEngine;
using UnityEngine.SceneManagement;

public class MonsterCreation : MonoBehaviour
{
    public void ReturnToMainMenu() // Returns the player to the main menu scene
    {
        SceneManager.LoadScene("MainMenuScene");

    }
  

}
