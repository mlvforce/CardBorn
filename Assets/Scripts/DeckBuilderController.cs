
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckBuilderController : MonoBehaviour
{
    private const int MaxDeckSize = 34;

    [Header("Deck UI")]
    [SerializeField] private Transform deckSlotsParent;
    [SerializeField] private TMP_InputField deckNameInput;
    [SerializeField] private TMP_Text deckCountText;

    [Header("Monster Collection")]
    [SerializeField] private Transform collectionContent;
    [SerializeField] private Button monsterCardPrefab;

    [Header("Optional Collection UI")]
    [SerializeField] private TMP_Text collectionCountText;
    [SerializeField] private TMP_Text selectedMonsterInfoText;

    [Header("Deck Controls")]
    [SerializeField] private Button saveDeckButton;
    [SerializeField] private Button clearDeckButton;
    [SerializeField] private Button randomDeckButton;

    [Header("Optional Feedback")]
    [SerializeField] private TMP_Text feedbackText;

    // NEW: Details popup references
    [Header("Monster Details Popup")]
    [SerializeField] private GameObject monsterDetailsPanel;
    [SerializeField] private TMP_Text monsterDetailsText;
    [SerializeField] private Button addToDeckButton;
    [SerializeField] private Button closeDetailsButton;

    private List<MonsterData> collection =
        new List<MonsterData>();

    private DeckData currentDeck = new DeckData();

    private Button[] deckSlotButtons;

    private MonsterData selectedMonster;

    private void Start()
    {
        SetupDeckSlots();
        SetupButtons();

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(false);

        LoadCollection();
        LoadCurrentDeck();
        RefreshDeckDisplay();
    }

    // ==========================================
    // SETUP
    // ==========================================

    private void SetupDeckSlots()
    {
        if (deckSlotsParent == null)
        {
            Debug.LogError("Deck slots parent missing!");
            return;
        }

        deckSlotButtons =
            deckSlotsParent.GetComponentsInChildren<Button>(true);

        if (deckSlotButtons.Length != MaxDeckSize)
        {
            Debug.LogWarning(
                "Expected 34 deck buttons, found " +
                deckSlotButtons.Length
            );
        }

        for (int i = 0; i < deckSlotButtons.Length; i++)
        {
            int index = i;

            deckSlotButtons[i].onClick.AddListener(
                () => OnDeckSlotClicked(index)
            );
        }
    }

    private void SetupButtons()
    {
        if (saveDeckButton != null)
            saveDeckButton.onClick.AddListener(SaveCurrentDeck);

        if (clearDeckButton != null)
            clearDeckButton.onClick.AddListener(ClearDeck);

        if (randomDeckButton != null)
            randomDeckButton.onClick.AddListener(RandomiseDeck);

        if (addToDeckButton != null)
            addToDeckButton.onClick.AddListener(AddSelectedMonster);

        if (closeDetailsButton != null)
            closeDetailsButton.onClick.AddListener(CloseMonsterDetails);
    }

    // ==========================================
    // LOAD MONSTER COLLECTION
    // ==========================================

    public void LoadCollection()
    {
        collection = MonsterSaveSystem.LoadMonsters();

        if (collection == null)
            collection = new List<MonsterData>();

        RefreshCollectionDisplay();

        Debug.Log(
            "Collection loaded: " + collection.Count +
            " monsters."
        );
    }

    // ==========================================
    // LOAD SAVED DECK
    // ==========================================

    private void LoadCurrentDeck()
    {
        List<DeckData> savedDecks =
            DeckSaveSystem.LoadDecks();

        if (savedDecks.Count > 0 &&
            savedDecks[0] != null)
        {
            currentDeck = savedDecks[0];
        }
        else
        {
            currentDeck = new DeckData();
            currentDeck.deckName = "My First Deck";
        }

        if (currentDeck.monsterIds == null)
        {
            currentDeck.monsterIds =
                new List<string>();
        }

        // Keep only IDs that exist in the collection.
        // This only changes the working deck until saved.
        currentDeck.monsterIds.RemoveAll(
            id => FindMonster(id) == null
        );

        if (currentDeck.monsterIds.Count > MaxDeckSize)
        {
            currentDeck.monsterIds.RemoveRange(
                MaxDeckSize,
                currentDeck.monsterIds.Count - MaxDeckSize
            );
        }

        if (deckNameInput != null)
            deckNameInput.text = currentDeck.deckName;

        RefreshDeckDisplay();
        RefreshCollectionDisplay();
    }

    // ==========================================
    // FIND MONSTER BY ID
    // ==========================================

    private MonsterData FindMonster(string id)
    {
        foreach (MonsterData monster in collection)
        {
            if (monster != null &&
                monster.monsterId == id)
            {
                return monster;
            }
        }

        return null;
    }

    // ==========================================
    // BUILD COLLECTION UI
    // ==========================================

    private void RefreshCollectionDisplay()
    {
        if (collectionContent == null ||
            monsterCardPrefab == null)
        {
            Debug.LogWarning(
                "Collection Content or card prefab missing."
            );
            return;
        }

        for (int i = collectionContent.childCount - 1;
             i >= 0; i--)
        {
            Destroy(
                collectionContent.GetChild(i).gameObject
            );
        }

        int count = 0;

        foreach (MonsterData monster in collection)
        {
            if (monster == null)
                continue;

            count++;

            Button card = Instantiate(
                monsterCardPrefab,
                collectionContent
            );

            card.gameObject.SetActive(true);

            TMP_Text text =
                card.GetComponentInChildren<TMP_Text>(true);

            if (text != null)
            {
                bool inDeck =
                    currentDeck.monsterIds.Contains(
                        monster.monsterId
                    );

                text.text =
                    monster.monsterName + "\n" +
                    "HP " + monster.health +
                    "  ATK " + monster.attack + "\n" +
                    "DEF " + monster.defence +
                    "  INT " + monster.intelligence + "\n" +
                    monster.summonCost + " SP" +
                    (inDeck ? "\nIN DECK" : "");

                text.enableAutoSizing = true;
                text.fontSizeMin = 10;
                text.fontSizeMax = 18;
                text.overflowMode =
                    TextOverflowModes.Truncate;
            }

            MonsterData capturedMonster = monster;

            card.onClick.RemoveAllListeners();

            card.onClick.AddListener(
                () => PreviewMonster(capturedMonster)
            );
        }

        if (collectionCountText != null)
            collectionCountText.text =
                "Your Collection: " + count;
    }

    // ==========================================
    // PREVIEW SELECTED MONSTER
    // ==========================================

    public void PreviewMonster(MonsterData monster)
    {
        if (monster == null)
            return;

        selectedMonster = monster;

        string effect1 =
            string.IsNullOrEmpty(monster.moveOne)
                ? "None" : monster.moveOne;

        string effect2 =
            string.IsNullOrEmpty(monster.moveTwo)
                ? "None" : monster.moveTwo;

        string details =
            monster.monsterName + "\n\n" +
            "HP: " + monster.health + "\n" +
            "DEF: " + monster.defence + "\n" +
            "ATK: " + monster.attack + "\n" +
            "INT: " + monster.intelligence + "\n\n" +
            "Summon Cost: " + monster.summonCost +
            " SP\n\n" +
            "Effect 1: " + effect1 + "\n" +
            "Effect 2: " + effect2;

        if (monsterDetailsText != null)
            monsterDetailsText.text = details;

        if (selectedMonsterInfoText != null)
            selectedMonsterInfoText.text = details;

        bool canAdd =
            currentDeck.monsterIds.Count < MaxDeckSize &&
            !currentDeck.monsterIds.Contains(monster.monsterId);

        if (addToDeckButton != null)
            addToDeckButton.interactable = canAdd;

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(true);

        Debug.Log(
            "Previewing monster: " + monster.monsterName
        );
    }

    // ==========================================
    // ADD SELECTED MONSTER
    // ==========================================

    public void AddSelectedMonster()
    {
        if (selectedMonster == null)
        {
            ShowFeedback("No monster selected.");
            return;
        }

        if (currentDeck.monsterIds.Count >= MaxDeckSize)
        {
            ShowFeedback("Deck is full!");
            return;
        }

        if (currentDeck.monsterIds.Contains(
            selectedMonster.monsterId))
        {
            ShowFeedback("Monster is already in the deck.");
            return;
        }

        if (string.IsNullOrEmpty(selectedMonster.monsterId))
        {
            ShowFeedback("Selected monster has no ID.");
            return;
        }

        currentDeck.monsterIds.Add(
            selectedMonster.monsterId
        );

        ShowFeedback(
            "Added " + selectedMonster.monsterName
        );

        RefreshDeckDisplay();
        RefreshCollectionDisplay();
        CloseMonsterDetails();
    }

    // ==========================================
    // CLOSE PREVIEW
    // ==========================================

    public void CloseMonsterDetails()
    {
        selectedMonster = null;

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(false);
    }

    // ==========================================
    // REMOVE DECK CARD
    // ==========================================

    private void OnDeckSlotClicked(int slot)
    {
        if (slot < 0 ||
            slot >= currentDeck.monsterIds.Count)
            return;

        string id = currentDeck.monsterIds[slot];

        MonsterData monster = FindMonster(id);

        currentDeck.monsterIds.RemoveAt(slot);

        ShowFeedback(
            monster == null
                ? "Removed card."
                : "Removed " + monster.monsterName
        );

        RefreshDeckDisplay();
        RefreshCollectionDisplay();
    }

    // ==========================================
    // DISPLAY DECK
    // ==========================================

    private void RefreshDeckDisplay()
    {
        int count = currentDeck.monsterIds.Count;

        if (deckCountText != null)
        {
            deckCountText.text =
                "Cards In Deck: " + count + "/34";
        }

        if (deckSlotButtons == null)
            return;

        for (int i = 0; i < deckSlotButtons.Length; i++)
        {
            TMP_Text label =
                deckSlotButtons[i]
                .GetComponentInChildren<TMP_Text>(true);

            if (label == null)
                continue;

            if (i < count)
            {
                MonsterData monster =
                    FindMonster(currentDeck.monsterIds[i]);

                label.text = monster == null
                    ? "Missing"
                    : ShortName(monster.monsterName);
            }
            else
            {
                label.text = (i + 1).ToString();
            }

            label.enableAutoSizing = true;
            label.fontSizeMin = 9;
            label.fontSizeMax = 18;
            label.enableWordWrapping = false;
            label.overflowMode =
                TextOverflowModes.Truncate;
        }
    }

    private string ShortName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "?";

        const int maxLength = 8;

        if (name.Length <= maxLength)
            return name;

        return name.Substring(0, maxLength - 1) + "…";
    }

    // ==========================================
    // CLEAR DECK
    // ==========================================

    public void ClearDeck()
    {
        currentDeck.monsterIds.Clear();

        CloseMonsterDetails();
        RefreshDeckDisplay();
        RefreshCollectionDisplay();

        ShowFeedback("Deck cleared.");
    }

    // ==========================================
    // RANDOMISE DECK
    // ==========================================

    public void RandomiseDeck()
    {
        List<MonsterData> available =
            new List<MonsterData>();

        HashSet<string> seen = new HashSet<string>();

        foreach (MonsterData monster in collection)
        {
            if (monster != null &&
                !string.IsNullOrEmpty(monster.monsterId) &&
                seen.Add(monster.monsterId))
            {
                available.Add(monster);
            }
        }

        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            MonsterData temp = available[i];
            available[i] = available[j];
            available[j] = temp;
        }

        currentDeck.monsterIds.Clear();

        int amount = Mathf.Min(
            MaxDeckSize,
            available.Count
        );

        for (int i = 0; i < amount; i++)
        {
            currentDeck.monsterIds.Add(
                available[i].monsterId
            );
        }

        CloseMonsterDetails();
        RefreshDeckDisplay();
        RefreshCollectionDisplay();

        ShowFeedback(
            "Random deck: " + amount + " cards."
        );
    }

    // ==========================================
    // SAVE DECK
    // ==========================================

    public void SaveCurrentDeck()
    {
        if (deckNameInput != null)
        {
            currentDeck.deckName =
                deckNameInput.text.Trim();
        }

        if (string.IsNullOrWhiteSpace(currentDeck.deckName))
        {
            ShowFeedback("Please enter a deck name.");
            return;
        }

        foreach (string id in currentDeck.monsterIds)
        {
            if (FindMonster(id) == null)
            {
                ShowFeedback("Deck has a missing monster.");
                return;
            }
        }

        if (DeckSaveSystem.SaveDeck(currentDeck))
        {
            ShowFeedback(
                "Deck saved: " + currentDeck.deckName
            );
        }
        else
        {
            ShowFeedback("Deck save failed.");
        }
    }

    // ==========================================
    // FEEDBACK
    // ==========================================

    private void ShowFeedback(string message)
    {
        Debug.Log(message);

        if (feedbackText != null)
            feedbackText.text = message;
    }

    // ==========================================
    // RELOAD
    // ==========================================

    public void ReloadCollection()
    {
        LoadCollection();
        LoadCurrentDeck();
        RefreshDeckDisplay();
    }
}
