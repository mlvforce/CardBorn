
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterSaveController : MonoBehaviour
{
    // ========================================
    // CREATION SYSTEM REFERENCES
    // ========================================

    [Header("Monster Creation References")]
    [SerializeField] private MonsterSwapper monsterSwapper;
    [SerializeField] private MonsterStats monsterStats;
    [SerializeField] private EffectPicker effectPicker;

    // ========================================
    // FINISH SCREEN UI
    // ========================================

    [Header("Finish UI")]
    [SerializeField] private TMP_InputField monsterNameInput;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Optional Preview Text")]
    [SerializeField] private TMP_Text monsterSummaryText;

    [Header("Optional Save Button")]
    [SerializeField] private Button saveButton;

    // ========================================
    // CURRENT SAVED STATE
    // ========================================

    private MonsterData savedMonster;

    // ========================================
    // CREATE MONSTER DATA
    // ========================================

    private MonsterData BuildMonsterData()
    {
        if (monsterSwapper == null ||
            monsterStats == null ||
            effectPicker == null)
        {
            Debug.LogError(
                "MonsterSaveController is missing references."
            );

            return null;
        }

        MonsterData data = new MonsterData();

        // NAME
        string enteredName =
            monsterNameInput != null
                ? monsterNameInput.text.Trim()
                : "";

        data.monsterName =
            string.IsNullOrWhiteSpace(enteredName)
                ? "New Monster"
                : enteredName;

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

        // VALIDATE EFFECTS AGAINST CURRENT STATS
        effectPicker.RefreshAvailableEffects();

        // EQUIPPED EFFECTS
        data.moveOne = effectPicker.GetEffect1();
        data.moveTwo = effectPicker.GetEffect2();

        return data;
    }

    // ========================================
    // REFRESH FINISH SCREEN
    // ========================================

    public void RefreshSummary()
    {
        MonsterData data = BuildMonsterData();

        if (data == null)
        {
            SetFeedback(
                "Unable to read monster information."
            );

            return;
        }

        if (monsterSummaryText == null)
            return;

        monsterSummaryText.text =
            "Name: " + data.monsterName + "\n" +
            "HP: " + data.health + "\n" +
            "DEF: " + data.defence + "\n" +
            "ATK: " + data.attack + "\n" +
            "INT: " + data.intelligence + "\n" +
            "Summon Cost: " + data.summonCost + " SP\n" +
            "Effect 1: " + FormatEffect(data.moveOne) + "\n" +
            "Effect 2: " + FormatEffect(data.moveTwo);
    }

    // ========================================
    // SAVE MONSTER
    // ========================================

    public void SaveMonster()
    {
        // Avoid saving twice from accidental double-clicks.
        if (savedMonster != null)
        {
            SetFeedback(
                "Monster already saved!"
            );

            return;
        }

        MonsterData data = BuildMonsterData();

        if (data == null)
        {
            SetFeedback(
                "Cannot save: missing monster data."
            );

            return;
        }

        bool success = MonsterSaveSystem.SaveMonster(data);

        if (success)
        {
            savedMonster = data;

            SetFeedback(
                data.monsterName + " saved successfully!"
            );

            if (saveButton != null)
                saveButton.interactable = false;

            RefreshSummary();
        }
        else
        {
            SetFeedback(
                "Saving failed. Check the Unity Console."
            );
        }
    }

    // ========================================
    // HELPERS
    // ========================================

    private string FormatEffect(string effectName)
    {
        return string.IsNullOrEmpty(effectName)
            ? "None"
            : effectName;
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;

        Debug.Log(message);
    }
}
