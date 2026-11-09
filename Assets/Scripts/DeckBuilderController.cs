
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckBuilderController : MonoBehaviour
{
    private const int MaxDeckSize = 34;

    // ==========================================
    // DECK UI
    // ==========================================

    [Header("Deck UI")]
    [SerializeField] private Transform deckSlotsParent;
    [SerializeField] private TMP_InputField deckNameInput;
    [SerializeField] private TMP_Text deckCountText;

    // ==========================================
    // COLLECTION UI
    // ==========================================

    [Header("Monster Collection")]
    [SerializeField] private Transform collectionContent;
    [SerializeField] private Button monsterCardPrefab;

    [Header("Optional Collection UI")]
    [SerializeField] private TMP_Text collectionCountText;
    [SerializeField] private TMP_Text selectedMonsterInfoText;

    // ==========================================
    // CONTROL BUTTONS
    // ==========================================

    [Header("Deck Controls")]
    [SerializeField] private Button saveDeckButton;
    [SerializeField] private Button clearDeckButton;
    [SerializeField] private Button randomDeckButton;

    [Header("Optional Feedback")]
    [SerializeField] private TMP_Text feedbackText;

    // ==========================================
    // CURRENT DATA
    // ==========================================

    private List<MonsterData> collection =
        new List<MonsterData>();

    private DeckData currentDeck = new DeckData();

    private Button[] deckSlotButtons;

    // ==========================================
    // START
    // ==========================================

    private void Start()
    {
        SetupDeckSlotButtons();
        SetupControlButtons();

        LoadCollection();
        LoadCurrentDeck();

        RefreshDeckDisplay();
    }

    // ==========================================
    // SET UP THE EXISTING 34 SLOTS
    // ==========================================

    private void SetupDeckSlotButtons()
    {
        if (deckSlotsParent == null)
        {
            Debug.LogError(
                "DeckBuilder: Deck Slots Parent is missing!"
            );
            return;
        }

        // Finds all Button components under the
        // existing Deck_Building_Buttons object.
        deckSlotButtons =
            deckSlotsParent.GetComponentsInChildren<Button>(true);

        if (deckSlotButtons.Length != MaxDeckSize)
        {
            Debug.LogWarning(
                "DeckBuilder: Found " +
                deckSlotButtons.Length +
                " slot buttons. Expected 34. " +
                "Make sure this parent contains only deck slots."
            );
        }

        for (int i = 0; i < deckSlotButtons.Length; i++)
        {
            int slotIndex = i;

            Button button = deckSlotButtons[i];

            button.onClick.AddListener(
                () => OnDeckSlotClicked(slotIndex)
            );
        }
    }

    // ==========================================
    // SET UP DECK CONTROL BUTTONS
    // ==========================================

    private void SetupControlButtons()
    {
        if (saveDeckButton != null)
        {
            saveDeckButton.onClick.AddListener(SaveCurrentDeck);
        }

        if (clearDeckButton != null)
        {
            clearDeckButton.onClick.AddListener(ClearDeck);
        }

        if (randomDeckButton != null)
        {
            randomDeckButton.onClick.AddListener(RandomiseDeck);
        }
    }

    // ==========================================
    // LOAD SAVED MONSTERS
    // ==========================================

    public void LoadCollection()
    {
        collection = MonsterSaveSystem.LoadMonsters();

        if (collection == null)
            collection = new List<MonsterData>();

        Debug.Log(
            "Loaded " + collection.Count +
            " monsters into collection."
        );

        RefreshCollectionDisplay();
    }

    // ==========================================
    // LOAD CURRENT DECK
    // ==========================================

    private void LoadCurrentDeck()
    {
        List<DeckData> savedDecks =
            DeckSaveSystem.LoadDecks();

        // For the beta, open the first saved deck.
        if (savedDecks.Count > 0 &&
            savedDecks[0] != null)
        {
            currentDeck = savedDecks[0];

            if (currentDeck.monsterIds == null)
            {
                currentDeck.monsterIds =
                    new List<string>();
            }

            // Remove references to monsters
            // that no longer exist.
            currentDeck.monsterIds.RemoveAll(
                id => FindMonster(id) == null
            );

            // Cap any old/invalid deck size.
            if (currentDeck.monsterIds.Count > MaxDeckSize)
            {
                currentDeck.monsterIds.RemoveRange(
                    MaxDeckSize,
                    currentDeck.monsterIds.Count - MaxDeckSize
                );
            }

            if (deckNameInput != null)
                deckNameInput.text = currentDeck.deckName;

            Debug.Log(
                "Loaded deck: " + currentDeck.deckName
            );
        }
        else
        {
            currentDeck = new DeckData();

            currentDeck.deckName = "My First Deck";

            if (deckNameInput != null)
                deckNameInput.text = currentDeck.deckName;
        }

        RefreshCollectionDisplay();
    }

    // ==========================================
    // FIND A SAVED MONSTER
    // ==========================================

    private MonsterData FindMonster(string monsterId)
    {
        foreach (MonsterData monster in collection)
        {
            if (monster != null &&
                monster.monsterId == monsterId)
            {
                return monster;
            }
        }

        return null;
    }

    // ==========================================
    // REFRESH MONSTER COLLECTION
    // ==========================================

    private void RefreshCollectionDisplay()
    {
        if (collectionContent == null ||
            monsterCardPrefab == null)
        {
            Debug.LogWarning(
                "Collection Content or Monster Card Prefab is missing."
            );
            return;
        }

        // Destroy previously generated cards.
        // The prefab is stored as an asset, not
        // inside this content object.
        for (int i = collectionContent.childCount - 1;
             i >= 0; i--)
        {
            Destroy(collectionContent.GetChild(i).gameObject);
        }

        int validCount = 0;

        foreach (MonsterData monster in collection)
        {
            if (monster == null)
                continue;

            validCount++;

            Button card = Instantiate(
                monsterCardPrefab,
                collectionContent
            );

            card.gameObject.SetActive(true);

            TMP_Text cardText =
                card.GetComponentInChildren<TMP_Text>(true);

            bool isInDeck =
                currentDeck.monsterIds != null &&
                currentDeck.monsterIds.Contains(monster.monsterId);

            if (cardText != null)
            {
                cardText.text =
                    monster.monsterName + "\n" +
                    "HP " + monster.health +
                    " | ATK " + monster.attack + "\n" +
                    "DEF " + monster.defence +
                    " | INT " + monster.intelligence + "\n" +
                    monster.summonCost + " SP" +
                    (isInDeck ? "\nIN DECK" : "");
            }

            // Capture this monster for its button.
            MonsterData capturedMonster = monster;

            card.onClick.RemoveAllListeners();

            card.onClick.AddListener(
                () => OnCollectionMonsterClicked(capturedMonster)
            );
        }

        if (collectionCountText != null)
        {
            collectionCountText.text =
                "Your Collection: " + validCount;
        }
    }

    // ==========================================
    // CLICK A MONSTER IN COLLECTION
    // ==========================================

    private void OnCollectionMonsterClicked(MonsterData monster)
    {
        if (monster == null)
            return;

        ShowMonsterDetails(monster);
        AddMonsterToDeck(monster);
    }

    // ==========================================
    // ADD MONSTER TO DECK
    // ==========================================

    public void AddMonsterToDeck(MonsterData monster)
    {
        if (monster == null ||
            string.IsNullOrEmpty(monster.monsterId))
        {
            ShowFeedback("Invalid monster.");
            return;
        }

        if (currentDeck.monsterIds.Count >= MaxDeckSize)
        {
            ShowFeedback(
                "Your deck is full! Maximum 34 cards."
            );
            return;
        }

        // For the beta, each saved monster
        // can only occupy one deck slot.
        if (currentDeck.monsterIds.Contains(monster.monsterId))
        {
            ShowFeedback(
                monster.monsterName +
                " is already in the deck."
            );
            return;
        }

        currentDeck.monsterIds.Add(monster.monsterId);

        ShowFeedback(
            "Added " + monster.monsterName
        );

        RefreshDeckDisplay();
        RefreshCollectionDisplay();
    }

    // ==========================================
    // CLICK AN OCCUPIED DECK SLOT
    // ==========================================

    private void OnDeckSlotClicked(int slotIndex)
    {
        if (slotIndex < 0 ||
            slotIndex >= currentDeck.monsterIds.Count)
        {
            return;
        }

        string monsterId =
            currentDeck.monsterIds[slotIndex];

        MonsterData monster = FindMonster(monsterId);

        currentDeck.monsterIds.RemoveAt(slotIndex);

        ShowFeedback(
            monster != null
                ? "Removed " + monster.monsterName
                : "Removed monster from deck."
        );

        RefreshDeckDisplay();
        RefreshCollectionDisplay();
    }

    // ==========================================
    // UPDATE THE 34 DECK SLOTS
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
            Button button = deckSlotButtons[i];

            TMP_Text label =
                button.GetComponentInChildren<TMP_Text>(true);

            if (label == null)
                continue;

            if (i < count)
            {
                MonsterData monster =
                    FindMonster(currentDeck.monsterIds[i]);

                label.text = monster != null
                    ? monster.monsterName
                    : "Missing";
            }
            else
            {
                // Show the original slot number.
                label.text = (i + 1).ToString();
            }
        }
    }

    // ==========================================
    // CLEAR CURRENT DECK
    // ==========================================

    public void ClearDeck()
    {
        currentDeck.monsterIds.Clear();

        RefreshDeckDisplay();
        RefreshCollectionDisplay();

        ShowFeedback("Deck cleared.");

        // Does NOT delete monsters.json.
        // Save the deck to persist this change.
    }

    // ==========================================
    // RANDOM DECK
    // ==========================================

    public void RandomiseDeck()
    {
        List<MonsterData> available =
            new List<MonsterData>();

        foreach (MonsterData monster in collection)
        {
            if (monster != null &&
                !string.IsNullOrEmpty(monster.monsterId))
            {
                available.Add(monster);
            }
        }

        if (available.Count == 0)
        {
            ShowFeedback(
                "Create some monsters first!"
            );
            return;
        }

        // Fisher-Yates shuffle.
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

        RefreshDeckDisplay();
        RefreshCollectionDisplay();

        ShowFeedback(
            "Random deck created with " +
            amount + " monster(s)."
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

        // Make sure every card still exists.
        foreach (string id in currentDeck.monsterIds)
        {
            if (FindMonster(id) == null)
            {
                ShowFeedback(
                    "Deck contains a missing monster."
                );
                return;
            }
        }

        bool saved =
            DeckSaveSystem.SaveDeck(currentDeck);

        if (saved)
        {
            ShowFeedback(
                "Deck saved: " + currentDeck.deckName
            );
        }
        else
        {
            ShowFeedback(
                "Deck saving failed. Check Console."
            );
        }
    }

    // ==========================================
    // SHOW MONSTER DETAILS
    // ==========================================

    private void ShowMonsterDetails(MonsterData monster)
    {
        if (selectedMonsterInfoText == null)
            return;

        string firstEffect =
            string.IsNullOrEmpty(monster.moveOne)
                ? "None"
                : monster.moveOne;

        string secondEffect =
            string.IsNullOrEmpty(monster.moveTwo)
                ? "None"
                : monster.moveTwo;

        selectedMonsterInfoText.text =
            monster.monsterName + "\n" +
            "HP: " + monster.health + "\n" +
            "DEF: " + monster.defence + "\n" +
            "ATK: " + monster.attack + "\n" +
            "INT: " + monster.intelligence + "\n" +
            "Cost: " + monster.summonCost + " SP\n" +
            "Effect 1: " + firstEffect + "\n" +
            "Effect 2: " + secondEffect;
    }

    // ==========================================
    // FEEDBACK
    // ==========================================

    private void ShowFeedback(string message)
    {
        Debug.Log(message);

        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    // ==========================================
    // OPTIONAL REFRESH METHOD
    // ==========================================

    public void ReloadCollection()
    {
        LoadCollection();
        LoadCurrentDeck();
        RefreshDeckDisplay();
    }
}
