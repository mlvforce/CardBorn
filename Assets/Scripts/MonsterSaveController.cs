
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MonsterSaveController : MonoBehaviour
{
    
    // MONSTER CREATION REFERENCES

    [Header("Monster Creation References")]
    [SerializeField] private MonsterSwapper monsterSwapper;
    [SerializeField] private MonsterStats monsterStats;
    [SerializeField] private EffectPicker effectPicker;

    // MONSTER NAME

    [Header("Monster Name")]
    [SerializeField] private TMP_InputField monsterNameInput;

    // STATS DISPLAY

    [Header("Final Stat Numbers")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text defenceText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text intelligenceText;

    [Header("Points Allocated")]
    [SerializeField] private TMP_Text pointsAllocatedText;

    // EFFECTS DISPLAY

    [Header("Equipped Effects")]
    [SerializeField] private TMP_Text effect1Text;
    [SerializeField] private TMP_Text effect2Text;

    // SUMMON COST

    [Header("Summon Cost")]
    [SerializeField] private TMP_Text summonCostText;

    // OPTIONAL UI

    [Header("Optional UI")]
    [SerializeField] private TMP_Text monsterSummaryText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button saveButton;

    // SAVE SETTINGS

    [Header("After Saving")]
    [SerializeField] private bool returnToHubAfterSave = true;
    [SerializeField] private string hubSceneName = "HubScene";

    private bool monsterSaved = false;

    // REFRESH SAVE SCREEN

    public void RefreshSummary()
    {
        if (monsterStats == null ||
            monsterSwapper == null ||
            effectPicker == null)
        {
            Debug.LogError(
                "MonsterSaveController: Missing creation references!"
            );
            return;
        }

        // Make sure effects still meet stat requirements.
        effectPicker.RefreshAvailableEffects();

        // STATS
         

        if (healthText != null)
            healthText.text = monsterStats.Health.ToString();

        if (defenceText != null)
            defenceText.text = monsterStats.Defence.ToString();

        if (attackText != null)
            attackText.text = monsterStats.Attack.ToString();

        if (intelligenceText != null)
            intelligenceText.text =
                monsterStats.Intelligence.ToString();

        // POINTS ALLOCATED

        if (pointsAllocatedText != null)
        {
            pointsAllocatedText.text =
                monsterStats.PointsSpent + "/100 Points Allocated";
        }

        // EQUIPPED EFFECTS

        string effectOne = effectPicker.GetEffect1();
        string effectTwo = effectPicker.GetEffect2();

        if (effect1Text != null)
            effect1Text.text = FormatEffect(effectOne);

        if (effect2Text != null)
            effect2Text.text = FormatEffect(effectTwo);

        // SUMMON COST

        if (summonCostText != null)
        {
            summonCostText.text =
                monsterStats.SummonCost + " SP";
        }

        // Optional combined summary field.
        if (monsterSummaryText != null)
        {
            monsterSummaryText.text =
                "HP: " + monsterStats.Health + "\n" +
                "DEF: " + monsterStats.Defence + "\n" +
                "ATK: " + monsterStats.Attack + "\n" +
                "INT: " + monsterStats.Intelligence + "\n" +
                "Effect 1: " + FormatEffect(effectOne) + "\n" +
                "Effect 2: " + FormatEffect(effectTwo) + "\n" +
                "Summon Cost: " + monsterStats.SummonCost + " SP";
        }

        Debug.Log("Save screen information refreshed.");
    }

    // BUILD MONSTER DATA

    private MonsterData BuildMonsterData()
    {
        if (monsterSwapper == null ||
            monsterStats == null ||
            effectPicker == null)
        {
            Debug.LogError(
                "Cannot build monster: missing references."
            );

            return null;
        }

        if (monsterNameInput == null)
        {
            Debug.LogError(
                "Monster Name Input has not been assigned!"
            );

            return null;
        }

        string enteredName = monsterNameInput.text.Trim();

        if (string.IsNullOrWhiteSpace(enteredName))
        {
            SetFeedback(
                "Please enter or generate a monster name!"
            );

            return null;
        }

        MonsterData data = new MonsterData();

        // NAME

        data.monsterName = enteredName;

        // APPEARANCE

        data.headIndex =
            monsterSwapper.GetSelectedHead();

        data.bodyIndex =
            monsterSwapper.GetSelectedBody();

        data.legsIndex =
            monsterSwapper.GetSelectedLegs();

        // STATS

        data.health = monsterStats.Health;
        data.defence = monsterStats.Defence;
        data.attack = monsterStats.Attack;
        data.intelligence = monsterStats.Intelligence;

        // SUMMON COST

        data.summonCost = monsterStats.SummonCost;

        // EFFECTS

        effectPicker.RefreshAvailableEffects();

        data.moveOne = effectPicker.GetEffect1();
        data.moveTwo = effectPicker.GetEffect2();

        return data;
    }

    
    // SAVE MONSTER

    public void SaveMonster()
    {
        if (monsterSaved)
        {
            SetFeedback("This monster has already been saved.");
            return;
        }

        MonsterData monster = BuildMonsterData();

        if (monster == null)
        {
            if (monsterNameInput == null)
            {
                SetFeedback("Save failed: missing name input.");
            }

            return;
        }

        bool saveSuccessful =
            MonsterSaveSystem.SaveMonster(monster);

        if (!saveSuccessful)
        {
            SetFeedback(
                "Failed to save monster. Check the Console."
            );

            return;
        }

        
        // SAVE SUCCESSFUL

        monsterSaved = true;

        SetFeedback(
            monster.monsterName + " saved successfully!"
        );

        Debug.Log(
            "Monster saved: " + monster.monsterName
        );

        Debug.Log(
            "Save location: " + MonsterSaveSystem.SavePath
        );

        if (saveButton != null)
            saveButton.interactable = false;

        // RETURN TO HUB

        if (returnToHubAfterSave)
        {
            if (Application.CanStreamedLevelBeLoaded(hubSceneName))
            {
                SceneManager.LoadScene(hubSceneName);
            }
            else
            {
                Debug.LogWarning(
                    "Monster saved, but Hub scene '" +
                    hubSceneName +
                    "' is not included in the Build Profile."
                );

                SetFeedback(
                    "Monster saved! Hub scene is not configured."
                );
            }
        }
    }

    // HELPER METHODS

    private string FormatEffect(string effectName)
    {
        if (string.IsNullOrEmpty(effectName))
            return "None";

        return effectName;
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;

        Debug.Log(message);
    }
}
