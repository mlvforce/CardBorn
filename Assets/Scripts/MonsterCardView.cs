
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterCardView : MonoBehaviour
{
    [Header("Monster Name")]
    [SerializeField] private TMP_Text monsterNameText;

    [Header("Health")]
    [SerializeField] private TMP_Text healthText;

    [Header("Summon Cost")]
    [SerializeField] private TMP_Text summonCostText;

    [Header("Bottom Stats")]
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text defenceText;
    [SerializeField] private TMP_Text intelligenceText;

    [Header("Effects Description")]
    [SerializeField] private TMP_Text effectsText;

    [Header("Optional")]
    [SerializeField] private TMP_Text inDeckText;
    [SerializeField] private Image monsterPortrait;

    private MonsterData currentMonster;

    // =========================================
    // DISPLAY MONSTER INFORMATION
    // =========================================

    public void DisplayMonster(
        MonsterData monster,
        string effectDescription,
        bool isInDeck)
    {
        if (monster == null)
        {
            Debug.LogWarning(
                "MonsterCardView: Monster data is null."
            );
            return;
        }

        currentMonster = monster;

        // MONSTER NAME
        if (monsterNameText != null)
        {
            monsterNameText.text = monster.monsterName;
        }

        // HEALTH
        if (healthText != null)
        {
            healthText.text = "Health: " + monster.health;
        }

        // SUMMON COST
        if (summonCostText != null)
        {
            summonCostText.text =
                "Summon Cost: " +
                monster.summonCost + " SP";
        }

        // ATTACK
        if (attackText != null)
        {
            attackText.text = "ATK: " + monster.attack;
        }

        // DEFENCE
        if (defenceText != null)
        {
            defenceText.text = "DEF: " + monster.defence;
        }

        // INTELLIGENCE
        if (intelligenceText != null)
        {
            intelligenceText.text =
                "INT: " + monster.intelligence;
        }

        // EFFECT DESCRIPTIONS
        if (effectsText != null)
        {
            effectsText.text =
                string.IsNullOrWhiteSpace(effectDescription)
                ? "No effects equipped."
                : CleanEffectText(effectDescription);
        }

        // OPTIONAL IN-DECK INDICATOR
        if (inDeckText != null)
        {
            inDeckText.text = isInDeck
                ? "IN DECK"
                : "";

        }

        // OPTIONAL MONSTER PORTRAIT
        // The appearance portrait can be
        // implemented later.
    }

    // =========================================
    // CLEAN EFFECT TEXT FORMATTING
    // =========================================

    private string CleanEffectText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "No effects equipped.";

        // Standardise line endings.
        text = text.Replace("\r\n", "\n");
        text = text.Replace("\r", "\n");

        // Remove whitespace at the beginning/end.
        text = text.Trim();

        // Remove repeated blank lines while
        // preserving one blank line between effects.
        string[] lines = text.Split('\n');

        System.Text.StringBuilder result =
            new System.Text.StringBuilder();

        bool previousLineWasBlank = false;

        foreach (string line in lines)
        {
            string cleanedLine = line.Trim();

            bool isBlank =
                string.IsNullOrWhiteSpace(cleanedLine);

            if (isBlank && previousLineWasBlank)
                continue;

            if (result.Length > 0)
                result.Append("\n");

            if (!isBlank)
                result.Append(cleanedLine);

            previousLineWasBlank = isBlank;
        }

        return result.ToString().Trim();
    }

    // =========================================
    // ACCESS CURRENT MONSTER
    // =========================================

    public MonsterData GetMonster()
    {
        return currentMonster;
    }
}
