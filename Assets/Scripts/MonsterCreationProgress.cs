
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

    // NAVIGATION BUTTONS

    [Header("Navigation Buttons")]
    [SerializeField] private GameObject nextButton;

    // SAVE SCREEN CONTROLLER

    [Header("Save Screen Controller")]
    [SerializeField] private MonsterSaveController saveController;

    // CURRENT STAGE
    

    private int currentStage = 0;

    // START

    private void Start()
    {
        ShowStage(0);
    }

    // NEXT BUTTON

    public void NextStage()
    {
        switch (currentStage)
        {
            case 0:
                ShowStage(1);
                break;

            case 1:
                ShowStage(2);
                break;

            case 2:
                ShowStage(3);
                break;

            case 3:
                
                Debug.Log(
                    "You are at the final stage. Save your monster!"
                );
                break;
        }
    }

    // BACK BUTTON

    public void PreviousStage()
    {
        switch (currentStage)
        {
            case 0:
                // Appearance -> Hub
                SceneManager.LoadScene("HubScene");
                break;

            case 1:
                // Stats -> Appearance
                ShowStage(0);
                break;

            case 2:
                // Effects -> Stats
                ShowStage(1);
                break;

            case 3:
                // Save Creation -> Effects
                ShowStage(2);
                break;
        }
    }

    // CHANGE CREATION STAGE

    private void ShowStage(int stage)
    {
        // Prevent invalid stage numbers.
        currentStage = Mathf.Clamp(stage, 0, 3);

        // DISABLE ALL CREATION SCREENS

        if (appearanceUI != null)
            appearanceUI.SetActive(false);

        if (statsUI != null)
            statsUI.SetActive(false);

        if (effectsUI != null)
            effectsUI.SetActive(false);

        if (finishUI != null)
            finishUI.SetActive(false);

        // UPDATE NEXT BUTTON

        if (nextButton != null)
        {
            
            nextButton.SetActive(currentStage != 3);
        }
        else
        {
            Debug.LogWarning(
                "MonsterCreationProgress: Next Button is not assigned!"
            );
        }

        // APPEARANCE STAGE

        if (currentStage == 0)
        {
            if (appearanceUI != null)
            {
                appearanceUI.SetActive(true);
            }

            MoveCamera(appearanceCameraPosition);

            Debug.Log("Creation Stage: Appearance");
        }

        
        // STATS STAGE

        else if (currentStage == 1)
        {
            if (statsUI != null)
            {
                statsUI.SetActive(true);
            }

            MoveCamera(statsCameraPosition);

            Debug.Log("Creation Stage: Stats");
        }

        // EFFECTS STAGE

        else if (currentStage == 2)
        {
            if (effectsUI != null)
            {
                effectsUI.SetActive(true);
            }

            MoveCamera(effectsCameraPosition);

            Debug.Log("Creation Stage: Effects");
        }

        // SAVE CREATION STAGE

        else if (currentStage == 3)
        {
            if (finishUI != null)
            {
                finishUI.SetActive(true);
            }

            MoveCamera(finishCameraPosition);

            
            if (saveController != null)
            {
                saveController.RefreshSummary();

                Debug.Log(
                    "Save Creation page refreshed successfully."
                );
            }
            else
            {
                Debug.LogWarning(
                    "MonsterCreationProgress: Save Controller is not assigned!"
                );
            }

            Debug.Log("Creation Stage: Save");
        }
    }

    // CAMERA MOVEMENT

    private void MoveCamera(Transform targetPosition)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning(
                "MonsterCreationProgress: Main Camera is missing!"
            );
            return;
        }

        if (targetPosition == null)
        {
            Debug.LogWarning(
                "MonsterCreationProgress: Camera position is missing!"
            );
            return;
        }

        // Move the camera to the position for this stage.
        mainCamera.transform.position = targetPosition.position;
        mainCamera.transform.rotation = targetPosition.rotation;
    }

    // GET CURRENT CREATION STAGE

    public int GetCurrentStage()
    {
        return currentStage;
    }
}
