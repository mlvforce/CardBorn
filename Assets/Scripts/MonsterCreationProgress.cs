using UnityEngine;
using UnityEngine.SceneManagement;

public class MonsterCreationProgress : MonoBehaviour
{
    // =========================
    // CREATION UI SCREENS
    // =========================

    [Header("Creation Screens")]
    [SerializeField] private GameObject appearanceUI;
    [SerializeField] private GameObject statsUI;


    // =========================
    // CAMERA SETTINGS
    // =========================

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [SerializeField] private Transform appearanceCameraPosition;
    [SerializeField] private Transform statsCameraPosition;


    // =========================
    // CURRENT CREATION STAGE
    // =========================

    // 0 = Appearance
    // 1 = Stats
    // 2 = Moves (later)
    // 3 = Finish (later)

    private int currentStage = 0;


    // =========================
    // START
    // =========================

    private void Start()
    {
        // Always start Monster Creation
        // on the Appearance screen.
        ShowStage(0);
    }


    // =========================
    // NEXT BUTTON
    // =========================

    public void NextStage()
    {
        // Appearance -> Stats
        if (currentStage == 0)
        {
            ShowStage(1);
        }

        // Stats -> Moves
        else if (currentStage == 1)
        {
            Debug.Log("Move Selection screen will go here later.");
        }

        // Moves -> Finish
        else if (currentStage == 2)
        {
            Debug.Log("Finish screen will go here later.");
        }
    }


    // =========================
    // BACK BUTTON
    // =========================

    public void PreviousStage()
    {
        // If we're on Appearance,
        // Back returns to the Hub.
        if (currentStage == 0)
        {
            SceneManager.LoadScene("HubScene");
        }

        // Stats -> Appearance
        else if (currentStage == 1)
        {
            ShowStage(0);
        }

        // Moves -> Stats
        else if (currentStage == 2)
        {
            ShowStage(1);
        }

        // Finish -> Moves
        else if (currentStage == 3)
        {
            ShowStage(2);
        }
    }


    // =========================
    // CHANGE CREATION STAGE
    // =========================

    private void ShowStage(int stage)
    {
        currentStage = stage;

        // -------------------------
        // APPEARANCE
        // -------------------------

        if (currentStage == 0)
        {
            appearanceUI.SetActive(true);
            statsUI.SetActive(false);

            MoveCamera(appearanceCameraPosition);
        }

        // -------------------------
        // STATS
        // -------------------------

        else if (currentStage == 1)
        {
            appearanceUI.SetActive(false);
            statsUI.SetActive(true);

            MoveCamera(statsCameraPosition);
        }

        // -------------------------
        // MOVES
        // -------------------------

        else if (currentStage == 2)
        {
            appearanceUI.SetActive(false);
            statsUI.SetActive(false);

            // Move UI will be added later.
        }

        // -------------------------
        // FINISH
        // -------------------------

        else if (currentStage == 3)
        {
            appearanceUI.SetActive(false);
            statsUI.SetActive(false);

            // Finish UI will be added later.
        }
    }


    // =========================
    // CAMERA MOVEMENT
    // =========================

    private void MoveCamera(Transform targetPosition)
    {
        if (mainCamera == null || targetPosition == null)
        {
            Debug.LogWarning("Camera or Camera Position has not been assigned!");
            return;
        }

        mainCamera.transform.position = targetPosition.position;
        mainCamera.transform.rotation = targetPosition.rotation;
    }
}