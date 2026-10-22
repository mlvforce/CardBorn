using UnityEngine;
using UnityEngine.SceneManagement;

public class MonsterCreationProgress : MonoBehaviour
{
   
    // CREATION UI SCREENS
   

    [Header("Creation Screens")]
    [SerializeField] private GameObject appearanceUI;
    [SerializeField] private GameObject statsUI;
    [SerializeField] private GameObject effectsUI;
    [SerializeField] private GameObject finishUI;


    
    // CAMERA SETTINGS
    

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [SerializeField] private Transform appearanceCameraPosition;
    [SerializeField] private Transform statsCameraPosition;
    [SerializeField] private Transform effectsCameraPosition;
    [SerializeField] private Transform finishCameraPosition;


    
    // CURRENT CREATION STAGE
    

  

    private int currentStage = 0;


    
    // START
   

    private void Start()
    {
        
        ShowStage(0);
    }


    
    // NEXT BUTTON
    

    public void NextStage()
    {
       
        if (currentStage == 0)
        {
            ShowStage(1);
        }

        
        else if (currentStage == 1)
        {
            ShowStage(2);
        }

        
        else if (currentStage == 2)
        {
       

            ShowStage(3);
        }

       
        else if (currentStage == 3)
        {
            Debug.Log("Monster is ready to be saved!");

            
        }
    }


   
    // BACK BUTTON
   

    public void PreviousStage()
    {
       
        if (currentStage == 0)
        {
            SceneManager.LoadScene("HubScene");
        }

       
        else if (currentStage == 1)
        {
            ShowStage(0);
        }

     
        else if (currentStage == 2)
        {
            ShowStage(1);
        }

       
        else if (currentStage == 3)
        {
            ShowStage(2);
        }
    }


    
    // CHANGE CREATION STAGE
    

    private void ShowStage(int stage)
    {
        currentStage = stage;


       
        // Disable all screens first
       

        if (appearanceUI != null)
            appearanceUI.SetActive(false);

        if (statsUI != null)
            statsUI.SetActive(false);

        if (effectsUI != null)
            effectsUI.SetActive(false);

        if (finishUI != null)
            finishUI.SetActive(false);


       
        // APPEARANCE
       

        if (currentStage == 0)
        {
            if (appearanceUI != null)
                appearanceUI.SetActive(true);

            MoveCamera(appearanceCameraPosition);
        }


      
        // STATS
        

        else if (currentStage == 1)
        {
            if (statsUI != null)
                statsUI.SetActive(true);

            MoveCamera(statsCameraPosition);
        }


        
        // EFFECTS
       

        else if (currentStage == 2)
        {
            if (effectsUI != null)
                effectsUI.SetActive(true);

            MoveCamera(effectsCameraPosition);
        }


        
        // FINISH
        

        else if (currentStage == 3)
        {
            if (finishUI != null)
                finishUI.SetActive(true);

            MoveCamera(finishCameraPosition);
        }
    }


    
    // CAMERA MOVEMENT
   

    private void MoveCamera(Transform targetPosition)
    {
        if (mainCamera == null || targetPosition == null)
        {
         
            return;
        }

        mainCamera.transform.position = targetPosition.position;
        mainCamera.transform.rotation = targetPosition.rotation;
    }


   
    // GET CURRENT STAGE

    public int GetCurrentStage()
    {
        return currentStage;
    }
}