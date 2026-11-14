
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

    // DISPLAY MONSTER INFORMATION

    public void DisplayMonster(
        MonsterData monster,
        string effectDescription,
        bool isInDeck)
    {
        if (monster == null)
        {
            Debug.LogWarning(
                "MonsterCardView: Monster data is null.",
                this
            );
            return;
        }

        currentMonster = monster;

        if (monsterNameText != null)
            monsterNameText.text = monster.monsterName;

        if (healthText != null)
            healthText.text = "Health: " + monster.health;

        if (summonCostText != null)
        {
            summonCostText.text =
                "Summon Cost: " +
                monster.summonCost + " SP";
        }

        if (attackText != null)
            attackText.text = "ATK: " + monster.attack;

        if (defenceText != null)
            defenceText.text = "DEF: " + monster.defence;

        if (intelligenceText != null)
        {
            intelligenceText.text =
                "INT: " + monster.intelligence;
        }

        if (effectsText != null)
        {
            effectsText.text =
                string.IsNullOrWhiteSpace(effectDescription)
                ? "No effects equipped."
                : CleanEffectText(effectDescription);
        }

        if (inDeckText != null)
            inDeckText.text = isInDeck ? "IN DECK" : "";
    }

    // COMPACT CARD MODE

    public void SetCompact(bool compact)
    {
        

        SetVisible(effectsText, !compact);
        SetVisible(attackText, !compact);
        SetVisible(defenceText, !compact);
        SetVisible(intelligenceText, !compact);

        if (compact)
        {
            SetVisible(inDeckText, false);
        }
        else
        {
            SetVisible(inDeckText, true);
        }
    }

    // UPDATE BATTLE HP WITHOUT CHANGING SAVED DATA

    public void SetBattleHealth(int currentHP)
    {
        if (healthText == null || currentMonster == null)
            return;

        healthText.text =
            "HP: " + currentHP +
            "/" + currentMonster.health;
    }

    private void SetVisible(
        TMP_Text textComponent,
        bool visible)
    {
        if (textComponent != null)
            textComponent.gameObject.SetActive(visible);
    }

    // CLEAN EFFECT TEXT FORMATTING

    private string CleanEffectText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "No effects equipped.";

        text = text.Replace("\r\n", "\n");
        text = text.Replace("\r", "\n");
        text = text.Trim();

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

    // ACCESS CURRENT MONSTER

    public MonsterData GetMonster()
    {
        return currentMonster;
    }
}
