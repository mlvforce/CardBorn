
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BattleDeckLoader : MonoBehaviour
{
    [Serializable]
    private class SavedDeckFile
    {
        public int activeSlot = 1;
        public List<SavedDeck> decks = new List<SavedDeck>();
    }

    [Serializable]
    private class SavedDeck
    {
        public string deckId;
        public string deckName;
        public List<string> monsterIds = new List<string>();
    }

    [Header("Spell Library")]
    [SerializeField] private SpellLibrary spellLibrary;

    [Header("Deck Selection")]
    [Tooltip("0 = active saved deck, or enter 1-4.")]
    [SerializeField, Range(0, 4)]
    private int playerDeckSlot = 0;

    [SerializeField, Range(1, 4)]
    private int aiDeckSlot = 2;

    public List<BattleCard> LoadPlayerDeck()
    {
        SavedDeckFile file = ReadDeckFile();

        if (file == null)
            return new List<BattleCard>();

        int slot = playerDeckSlot == 0
            ? file.activeSlot
            : playerDeckSlot;

        return BuildDeck(GetSlot(file, slot));
    }

    public List<BattleCard> LoadAIDeck()
    {
        SavedDeckFile file = ReadDeckFile();

        if (file == null)
            return new List<BattleCard>();

        return BuildDeck(GetSlot(file, aiDeckSlot));
    }

    private SavedDeckFile ReadDeckFile()
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            "deck_slots.json"
        );

        if (!File.Exists(path))
        {
            Debug.LogError(
                "Battle: Cannot find deck_slots.json at " + path
            );
            return null;
        }

        try
        {
            SavedDeckFile file = JsonUtility.FromJson<SavedDeckFile>(
                File.ReadAllText(path)
            );

            if (file == null || file.decks == null)
            {
                Debug.LogError("Battle: Invalid deck save.");
                return null;
            }

            return file;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "Battle: Could not read deck save. " +
                exception.Message
            );
            return null;
        }
    }

    private SavedDeck GetSlot(SavedDeckFile file, int number)
    {
        string wantedId = "deck-slot-" + number;

        foreach (SavedDeck deck in file.decks)
        {
            if (deck != null &&
                string.Equals(
                    deck.deckId,
                    wantedId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return deck;
            }
        }

        Debug.LogError(
            "Battle: Could not find saved deck slot " + number
        );

        return null;
    }

    private List<BattleCard> BuildDeck(SavedDeck savedDeck)
    {
        List<BattleCard> cards = new List<BattleCard>();

        if (savedDeck == null || savedDeck.monsterIds == null)
            return cards;

        List<MonsterData> monsters = MonsterSaveSystem.LoadMonsters();

        if (monsters == null)
            monsters = new List<MonsterData>();

        foreach (string savedId in savedDeck.monsterIds)
        {
            if (string.IsNullOrWhiteSpace(savedId))
                continue;

            if (savedId.StartsWith(
                "spell:",
                StringComparison.OrdinalIgnoreCase))
            {
                string spellId = savedId.Substring(6);

                SpellData spell = spellLibrary != null
                    ? spellLibrary.GetSpellById(spellId)
                    : null;

                if (spell == null)
                {
                    Debug.LogWarning(
                        "Battle: Missing spell " + spellId
                    );
                    continue;
                }

                cards.Add(new BattleCard
                {
                    type = BattleCardType.Spell,
                    id = spellId,
                    spell = spell
                });
            }
            else
            {
                string monsterId = savedId.StartsWith(
                    "monster:",
                    StringComparison.OrdinalIgnoreCase)
                    ? savedId.Substring(8)
                    : savedId;

                MonsterData monster = monsters.Find(
                    m => m != null &&
                    string.Equals(
                        m.monsterId,
                        monsterId,
                        StringComparison.OrdinalIgnoreCase)
                );

                if (monster == null)
                {
                    Debug.LogWarning(
                        "Battle: Missing monster " + monsterId
                    );
                    continue;
                }

                cards.Add(new BattleCard
                {
                    type = BattleCardType.Monster,
                    id = monsterId,
                    monster = monster
                });
            }
        }

        Debug.Log(
            "Battle: Loaded " + savedDeck.deckName +
            " with " + cards.Count + " cards."
        );

        return cards;
    }
}
