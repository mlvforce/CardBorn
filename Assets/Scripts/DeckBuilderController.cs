
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class CardEffectDescription
{
    public string effectName;

    [TextArea(3, 8)]
    public string description;
}

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

    [Header("Monster Details Popup")]
    [SerializeField] private GameObject monsterDetailsPanel;
    [SerializeField] private TMP_Text monsterDetailsText;
    [SerializeField] private Button addToDeckButton;
    [SerializeField] private Button closeDetailsButton;

    [Header("Optional Large Card Preview")]
    [SerializeField] private MonsterCardView previewCardView;

    [Header("Effect Description Library")]
    [SerializeField]
    private List<CardEffectDescription>
        effectDescriptions = new List<CardEffectDescription>();

    [Header("Deck Slot Appearance")]
    [SerializeField] private bool showCardArtworkInDeckSlots = false;

    [Header("Optional Collection Layout")]
    [SerializeField] private LayoutElement collectionSectionLayout;
    [SerializeField] private RectTransform collectionTitle;
    [SerializeField] private float collectionTopPadding = 80f;
    [SerializeField] private float collectionBottomPadding = 40f;
    [SerializeField] private float minimumCollectionHeight = 700f;

    private List<MonsterData> collection =
        new List<MonsterData>();

    private DeckData currentDeck = new DeckData();

    private Button[] deckSlotButtons;
    private Sprite[] originalSlotSprites;
    private Color[] originalSlotColors;
    private Image.Type[] originalSlotImageTypes;

    private MonsterData selectedMonster;

    private class CollectionEntry
    {
        public MonsterData monster;
        public MonsterCardView view;
    }

    private readonly List<CollectionEntry> collectionEntries =
        new List<CollectionEntry>();

    private readonly List<GameObject> generatedObjects =
        new List<GameObject>();

    // ==========================================
    // INITIALISATION
    // ==========================================

    private void Start()
    {
        SetupDeckSlots();
        SetupControlButtons();

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(false);

        LoadCollection();
        LoadCurrentDeck();
        BuildCollectionCards();
        RefreshAll();
    }

    // ==========================================
    // SET UP THE 34 DECK SLOTS
    // ==========================================

    private void SetupDeckSlots()
    {
        if (deckSlotsParent == null)
        {
            Debug.LogError(
                "DeckBuilder: Deck Slots Parent is missing!"
            );
            return;
        }

        deckSlotButtons =
            deckSlotsParent.GetComponentsInChildren<Button>(true);

        int count = deckSlotButtons.Length;

        if (count != MaxDeckSize)
        {
            Debug.LogWarning(
                "DeckBuilder: Found " + count +
                " deck slot buttons, expected 34."
            );
        }

        originalSlotSprites = new Sprite[count];
        originalSlotColors = new Color[count];
        originalSlotImageTypes = new Image.Type[count];

        for (int i = 0; i < count; i++)
        {
            int slotIndex = i;
            Button button = deckSlotButtons[i];

            if (button.image != null)
            {
                originalSlotSprites[i] = button.image.sprite;
                originalSlotColors[i] = button.image.color;
                originalSlotImageTypes[i] = button.image.type;
            }

            button.onClick.AddListener(
                () => OnDeckSlotClicked(slotIndex)
            );
        }
    }

    // ==========================================
    // CONNECT UI BUTTONS AUTOMATICALLY
    // ==========================================

    private void SetupControlButtons()
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
    // LOAD SAVED MONSTERS
    // ==========================================

    public void LoadCollection()
    {
        collection = MonsterSaveSystem.LoadMonsters();

        if (collection == null)
            collection = new List<MonsterData>();

        Debug.Log(
            "DeckBuilder: Loaded " +
            collection.Count + " monsters."
        );
    }

    // ==========================================
    // LOAD SAVED DECK
    // ==========================================

    private void LoadCurrentDeck()
    {
        List<DeckData> savedDecks =
            DeckSaveSystem.LoadDecks();

        if (savedDecks != null &&
            savedDecks.Count > 0 &&
            savedDecks[0] != null)
        {
            DeckData saved = savedDecks[0];

            currentDeck = new DeckData();
            currentDeck.deckId = saved.deckId;
            currentDeck.deckName = saved.deckName;

            currentDeck.monsterIds = saved.monsterIds != null
                ? new List<string>(saved.monsterIds)
                : new List<string>();

            Debug.Log(
                "DeckBuilder: Loaded deck " +
                currentDeck.deckName
            );
        }
        else
        {
            currentDeck = new DeckData();
            currentDeck.deckName = "My First Deck";
            currentDeck.monsterIds = new List<string>();
        }

        // For the current beta, one copy of each
        // saved monster is allowed per deck.
        HashSet<string> seen = new HashSet<string>();

        currentDeck.monsterIds.RemoveAll(
            id => FindMonster(id) == null ||
                  !seen.Add(id)
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
    }

    // ==========================================
    // FIND MONSTER
    // ==========================================

    private MonsterData FindMonster(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

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
    // GENERATE COLLECTION CARDS
    // ==========================================

    private void BuildCollectionCards()
    {
        if (collectionContent == null ||
            monsterCardPrefab == null)
        {
            Debug.LogError(
                "DeckBuilder: Assign Collection Content " +
                "and Monster Card Prefab in the Inspector."
            );
            return;
        }

        // Destroy only cards generated by this script.
        // Do not delete any manually created UI elements.
        foreach (GameObject generated in generatedObjects)
        {
            if (generated != null)
                Destroy(generated);
        }

        generatedObjects.Clear();
        collectionEntries.Clear();

        foreach (MonsterData monster in collection)
        {
            if (monster == null)
                continue;

            Button card = Instantiate(
                monsterCardPrefab,
                collectionContent,
                false
            );

            generatedObjects.Add(card.gameObject);

            MonsterCardView cardView =
                card.GetComponent<MonsterCardView>();

            if (cardView == null)
            {
                Debug.LogError(
                    "DeckBuilder: Monster Card Prefab " +
                    "is missing MonsterCardView on its root!"
                );

                card.gameObject.SetActive(false);
                continue;
            }

            MonsterData capturedMonster = monster;

            CollectionEntry entry = new CollectionEntry();
            entry.monster = monster;
            entry.view = cardView;

            collectionEntries.Add(entry);

            card.onClick.RemoveAllListeners();

            card.onClick.AddListener(
                () => PreviewMonster(capturedMonster)
            );

            card.gameObject.SetActive(true);

            cardView.DisplayMonster(
                monster,
                BuildEffectDescription(monster),
                currentDeck.monsterIds != null &&
                currentDeck.monsterIds.Contains(monster.monsterId)
            );
        }

        UpdateCollectionHeight();

        Debug.Log(
            "DeckBuilder: Generated " +
            collectionEntries.Count + " collection cards."
        );
    }

    // ==========================================
    // OPTIONAL COLLECTION HEIGHT ADJUSTMENT
    // ==========================================

    private void UpdateCollectionHeight()
    {
        if (collectionContent == null)
            return;

        GridLayoutGroup grid =
            collectionContent.GetComponent<GridLayoutGroup>();

        RectTransform gridRect =
            collectionContent as RectTransform;

        if (grid == null || gridRect == null)
            return;

        int cardCount = collectionEntries.Count;

        int columns = 1;

        if (grid.constraint ==
            GridLayoutGroup.Constraint.FixedColumnCount)
        {
            columns = Mathf.Max(1, grid.constraintCount);
        }
        else
        {
            float availableWidth = gridRect.rect.width -
                grid.padding.left - grid.padding.right;

            columns = Mathf.Max(
                1,
                Mathf.FloorToInt(
                    (availableWidth + grid.spacing.x) /
                    Mathf.Max(
                        1f,
                        grid.cellSize.x + grid.spacing.x
                    )
                )
            );
        }

        int rows = Mathf.CeilToInt(
            cardCount / (float)columns
        );

        float gridHeight =
            grid.padding.top +
            grid.padding.bottom +
            rows * grid.cellSize.y +
            Mathf.Max(0, rows - 1) * grid.spacing.y;

        // This grid is not controlled by a ContentSizeFitter.
        gridRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            gridHeight
        );

        if (collectionSectionLayout != null)
        {
            float titleHeight = collectionTitle != null
                ? collectionTitle.rect.height
                : 50f;

            float requiredHeight =
                collectionTopPadding +
                titleHeight +
                gridHeight +
                collectionBottomPadding;

            collectionSectionLayout.preferredHeight =
                Mathf.Max(
                    minimumCollectionHeight,
                    requiredHeight
                );
        }

        Canvas.ForceUpdateCanvases();
    }

    // ==========================================
    // REFRESH ALL DISPLAYED DATA
    // ==========================================

    private void RefreshAll()
    {
        RefreshDeckDisplay();
        RefreshCollectionDisplay();
    }

    private void RefreshCollectionDisplay()
    {
        foreach (CollectionEntry entry in collectionEntries)
        {
            if (entry == null ||
                entry.view == null ||
                entry.monster == null)
            {
                continue;
            }

            bool isInDeck =
                currentDeck.monsterIds.Contains(
                    entry.monster.monsterId
                );

            entry.view.DisplayMonster(
                entry.monster,
                BuildEffectDescription(entry.monster),
                isInDeck
            );
        }

        if (collectionCountText != null)
        {
            collectionCountText.text =
                "Your Collection: " +
                collectionEntries.Count;
        }
    }

    // ==========================================
    // EFFECT DESCRIPTIONS
    // ==========================================

    private string BuildEffectDescription(MonsterData monster)
    {
        if (monster == null)
            return "";

        string result = "";

        if (!string.IsNullOrWhiteSpace(monster.moveOne))
        {
            result = GetEffectDescription(monster.moveOne);
        }

        if (!string.IsNullOrWhiteSpace(monster.moveTwo))
        {
            if (!string.IsNullOrEmpty(result))
                result += "\n\n";

            result += GetEffectDescription(monster.moveTwo);
        }

        return result;
    }

    private string GetEffectDescription(string effectName)
    {
        foreach (CardEffectDescription effect in effectDescriptions)
        {
            if (effect == null)
                continue;

            if (string.Equals(
                effect.effectName != null
                    ? effect.effectName.Trim()
                    : "",
                effectName.Trim(),
                StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(
                    effect.description))
                {
                    return effectName + ":\n" +
                           effect.description;
                }
            }
        }

        // If the description isn't configured,
        // display the saved effect name.
        return effectName;
    }

    // ==========================================
    // MONSTER PREVIEW
    // ==========================================

    public void PreviewMonster(MonsterData monster)
    {
        if (monster == null)
            return;

        selectedMonster = monster;

        bool inDeck =
            currentDeck.monsterIds.Contains(
                monster.monsterId
            );

        if (previewCardView != null)
        {
            previewCardView.DisplayMonster(
                monster,
                BuildEffectDescription(monster),
                inDeck
            );
        }

        string effect1 =
            string.IsNullOrWhiteSpace(monster.moveOne)
                ? "None" : monster.moveOne;

        string effect2 =
            string.IsNullOrWhiteSpace(monster.moveTwo)
                ? "None" : monster.moveTwo;

        string details =
            monster.monsterName + "\n\n" +
            "HP: " + monster.health + "\n" +
            "ATK: " + monster.attack + "\n" +
            "DEF: " + monster.defence + "\n" +
            "INT: " + monster.intelligence + "\n" +
            "Summon Cost: " + monster.summonCost +
            " SP\n\n" +
            "Effect 1: " + effect1 + "\n" +
            "Effect 2: " + effect2;

        if (monsterDetailsText != null)
            monsterDetailsText.text = details;

        if (selectedMonsterInfoText != null)
            selectedMonsterInfoText.text = details;

        if (addToDeckButton != null)
        {
            addToDeckButton.interactable =
                !inDeck &&
                currentDeck.monsterIds.Count < MaxDeckSize;
        }

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(true);
    }

    public void CloseMonsterDetails()
    {
        selectedMonster = null;

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(false);
    }

    // ==========================================
    // ADD SELECTED MONSTER
    // ==========================================

    public void AddSelectedMonster()
    {
        if (selectedMonster == null)
        {
            ShowFeedback("Select a monster first.");
            return;
        }

        if (string.IsNullOrEmpty(selectedMonster.monsterId))
        {
            ShowFeedback("Monster has no valid ID.");
            return;
        }

        if (currentDeck.monsterIds.Count >= MaxDeckSize)
        {
            ShowFeedback("Your deck is full!");
            return;
        }

        if (currentDeck.monsterIds.Contains(
            selectedMonster.monsterId))
        {
            ShowFeedback(
                "That monster is already in your deck."
            );
            return;
        }

        currentDeck.monsterIds.Add(
            selectedMonster.monsterId
        );

        ShowFeedback(
            "Added " + selectedMonster.monsterName
        );

        RefreshAll();
        CloseMonsterDetails();
    }

    // ==========================================
    // REMOVE MONSTER FROM A DECK SLOT
    // ==========================================

    private void OnDeckSlotClicked(int slotIndex)
    {
        if (slotIndex < 0 ||
            slotIndex >= currentDeck.monsterIds.Count)
        {
            return;
        }

        MonsterData monster = FindMonster(
            currentDeck.monsterIds[slotIndex]
        );

        currentDeck.monsterIds.RemoveAt(slotIndex);

        ShowFeedback(
            monster == null
                ? "Removed a monster."
                : "Removed " + monster.monsterName
        );

        RefreshAll();
    }

    // ==========================================
    // UPDATE DECK SLOT DISPLAY
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

        Sprite cardSprite = null;

        if (monsterCardPrefab != null &&
            monsterCardPrefab.image != null)
        {
            cardSprite = monsterCardPrefab.image.sprite;
        }

        for (int i = 0; i < deckSlotButtons.Length; i++)
        {
            Button button = deckSlotButtons[i];

            TMP_Text label =
                button.GetComponentInChildren<TMP_Text>(true);

            bool occupied = i < count;

            MonsterData monster = occupied
                ? FindMonster(currentDeck.monsterIds[i])
                : null;

            bool useArtwork =
                showCardArtworkInDeckSlots &&
                occupied &&
                cardSprite != null;

            if (label != null)
            {
                label.text = occupied
                    ? (monster == null
                        ? "Missing"
                        : ShortName(monster.monsterName))
                    : (i + 1).ToString();

                label.enableAutoSizing = true;
                label.fontSizeMin = 9;
                label.fontSizeMax = 18;
                label.textWrappingMode =
                    TextWrappingModes.NoWrap;
                label.overflowMode =
                    TextOverflowModes.Truncate;
                label.alignment =
                    TextAlignmentOptions.Center;

                label.enabled = !useArtwork;
            }

            if (button.image == null)
                continue;

            if (useArtwork)
            {
                button.image.sprite = cardSprite;
                button.image.type = Image.Type.Simple;
                button.image.preserveAspect = true;
                button.image.color = Color.white;
            }
            else
            {
                button.image.sprite = originalSlotSprites[i];
                button.image.color = originalSlotColors[i];
                button.image.type = originalSlotImageTypes[i];
                button.image.preserveAspect = false;
            }
        }
    }

    private string ShortName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "?";

        const int maxLength = 8;

        if (name.Length <= maxLength)
            return name;

        return name.Substring(0, maxLength - 1) + "...";
    }

    // ==========================================
    // CLEAR DECK
    // ==========================================

    public void ClearDeck()
    {
        currentDeck.monsterIds.Clear();

        CloseMonsterDetails();
        RefreshAll();

        ShowFeedback("Deck cleared.");
    }

    // ==========================================
    // RANDOMISE DECK
    // ==========================================

    public void RandomiseDeck()
    {
        List<MonsterData> available =
            new List<MonsterData>();

        HashSet<string> seen =
            new HashSet<string>();

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
            int randomIndex =
                UnityEngine.Random.Range(0, i + 1);

            MonsterData temp = available[i];

            available[i] = available[randomIndex];
            available[randomIndex] = temp;
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
        RefreshAll();

        ShowFeedback(
            "Random deck created with " +
            amount + " monsters."
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
                ShowFeedback(
                    "Deck contains an unavailable monster."
                );
                return;
            }
        }

        bool saved =
            DeckSaveSystem.SaveDeck(currentDeck);

        ShowFeedback(
            saved
                ? "Deck saved: " + currentDeck.deckName
                : "Deck saving failed. Check Console."
        );
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
    // RELOAD MONSTERS AND DECK
    // ==========================================

    public void ReloadCollection()
    {
        CloseMonsterDetails();

        LoadCollection();
        LoadCurrentDeck();
        BuildCollectionCards();
        RefreshAll();
    }
}
