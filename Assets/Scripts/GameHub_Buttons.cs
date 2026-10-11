using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHub_Buttons : MonoBehaviour
{
   public void OpenMonsterCreation() // loads the player into the monster creation menu page from the hub page
    {
        SceneManager.LoadScene("MonsterCreationScene");

    }
    public void OpenDeckBuilder() // Loads the player into the Deck Builder Scene
    {
        SceneManager.LoadScene("DeckBuilderScene");

    }
    public void OpenBattleScene() // Loads the player into the Battle Lobby Scene
    { 
        SceneManager.LoadScene("LobbyScene");

    }
    public void ReturnToMainMenu() // Returns the player to the main menu scene
    {
        SceneManager.LoadScene("MainMenuScene");

    }
   
}
