
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
    private const int MaxCopiesPerMonster = 3;

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

    [Header("NEW - Remove From Deck")]
    [SerializeField] private Button removeFromDeckButton;

    [Header("Optional Large Card Preview")]
    [SerializeField] private MonsterCardView previewCardView;

    [Header("Effect Description Library")]
    [SerializeField]
    private List<CardEffectDescription>
        effectDescriptions = new List<CardEffectDescription>();

    [Header("Deck Slot Appearance")]
    [SerializeField] private bool showCardArtworkInDeckSlots = true;
    [SerializeField] private bool removeDeckSlotTint = true;

    [Header("Optional Collection Layout")]
    [SerializeField] private LayoutElement collectionSectionLayout;
    [SerializeField] private RectTransform collectionTitle;
    [SerializeField] private float collectionTopPadding = 80f;
    [SerializeField] private float collectionBottomPadding = 40f;
    [SerializeField] private float minimumCollectionHeight = 700f;

    [Header("Collection Card Scaling")]
    [SerializeField] private bool useCardWrappers = true;
    [SerializeField] private float cardScaleMultiplier = 1f;
    [SerializeField] private float cardPadding = 0f;

    private List<MonsterData> collection =
        new List<MonsterData>();

    private DeckData currentDeck = new DeckData();

    private Button[] deckSlotButtons;
    private Sprite[] originalSlotSprites;
    private Color[] originalSlotColors;
    private Image.Type[] originalSlotImageTypes;
    private bool[] originalPreserveAspect;
    private Selectable.Transition[] originalTransitions;

    private MonsterData selectedMonster;

    // -1 means the card was opened from the collection.
    // 0 to 33 means it was opened from a deck slot.
    private int selectedDeckSlot = -1;

    private class CollectionEntry
    {
        public MonsterData monster;
        public MonsterCardView view;
        public RectTransform wrapper;
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
    // DECK SLOT BUTTON SETUP
    // ==========================================

    private void SetupDeckSlots()
    {
        if (deckSlotsParent == null)
        {
            Debug.LogError(
                "DeckBuilder: Deck Slots Parent missing!"
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
                " slot buttons. Expected 34."
            );
        }

        originalSlotSprites = new Sprite[count];
        originalSlotColors = new Color[count];
        originalSlotImageTypes = new Image.Type[count];
        originalPreserveAspect = new bool[count];
        originalTransitions = new Selectable.Transition[count];

        for (int i = 0; i < count; i++)
        {
            int slotIndex = i;
            Button button = deckSlotButtons[i];

            originalTransitions[i] = button.transition;

            if (button.image != null)
            {
                originalSlotSprites[i] = button.image.sprite;
                originalSlotColors[i] = button.image.color;
                originalSlotImageTypes[i] = button.image.type;
                originalPreserveAspect[i] =
                    button.image.preserveAspect;
            }

            button.onClick.AddListener(
                () => OnDeckSlotClicked(slotIndex)
            );
        }
    }

    // ==========================================
    // BUTTON REGISTRATION
    // ==========================================

    private void SetupControlButtons()
    {
        if (saveDeckButton != null)
            saveDeckButton.onClick.AddListener(
                SaveCurrentDeck
            );

        if (clearDeckButton != null)
            clearDeckButton.onClick.AddListener(
                ClearDeck
            );

        if (randomDeckButton != null)
            randomDeckButton.onClick.AddListener(
                RandomiseDeck
            );

        if (addToDeckButton != null)
            addToDeckButton.onClick.AddListener(
                AddSelectedMonster
            );

        if (removeFromDeckButton != null)
            removeFromDeckButton.onClick.AddListener(
                RemoveSelectedDeckCopy
            );

        if (closeDetailsButton != null)
            closeDetailsButton.onClick.AddListener(
                CloseMonsterDetails
            );
    }

    // ==========================================
    // LOAD MONSTER COLLECTION
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
    // LOAD DECK
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

            currentDeck.monsterIds =
                saved.monsterIds != null
                ? new List<string>(saved.monsterIds)
                : new List<string>();
        }
        else
        {
            currentDeck = new DeckData();
            currentDeck.deckName = "My First Deck";
            currentDeck.monsterIds = new List<string>();
        }

        if (currentDeck.monsterIds == null)
            currentDeck.monsterIds = new List<string>();

        // Remove invalid IDs, excess copies and
        // entries beyond the deck size.
        List<string> validatedIds = new List<string>();

        Dictionary<string, int> copyCounts =
            new Dictionary<string, int>();

        foreach (string id in currentDeck.monsterIds)
        {
            if (validatedIds.Count >= MaxDeckSize)
                break;

            if (FindMonster(id) == null)
                continue;

            int copies = 0;
            copyCounts.TryGetValue(id, out copies);

            if (copies >= MaxCopiesPerMonster)
                continue;

            validatedIds.Add(id);
            copyCounts[id] = copies + 1;
        }

        currentDeck.monsterIds = validatedIds;

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
    // NEW - COUNT COPIES IN CURRENT DECK
    // ==========================================

    private int CountCopies(string monsterId)
    {
        if (string.IsNullOrEmpty(monsterId))
            return 0;

        int copies = 0;

        foreach (string id in currentDeck.monsterIds)
        {
            if (id == monsterId)
                copies++;
        }

        return copies;
    }

    // ==========================================
    // BUILD COLLECTION
    // ==========================================

    private void BuildCollectionCards()
    {
        if (collectionContent == null ||
            monsterCardPrefab == null)
        {
            Debug.LogError(
                "DeckBuilder: Assign Collection Content " +
                "and Monster Card Prefab."
            );
            return;
        }

        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Destroy(obj);
            }
        }

        generatedObjects.Clear();
        collectionEntries.Clear();

        GridLayoutGroup grid =
            collectionContent.GetComponent<GridLayoutGroup>();

        Vector2 cellSize = grid != null
            ? grid.cellSize
            : new Vector2(120f, 160f);

        foreach (MonsterData monster in collection)
        {
            if (monster == null)
                continue;

            if (useCardWrappers)
                CreateWrappedCard(monster, cellSize);
            else
                CreateDirectCard(monster);
        }

        UpdateCollectionHeight();

        Debug.Log(
            "DeckBuilder: Generated " +
            collectionEntries.Count + " collection cards."
        );
    }

    // ==========================================
    // COLLECTION WRAPPER SYSTEM
    // ==========================================

    private void CreateWrappedCard(
        MonsterData monster,
        Vector2 cellSize)
    {
        GameObject wrapperObject =
            new GameObject(
                "Card_Wrapper_" + monster.monsterName,
                typeof(RectTransform)
            );

        RectTransform wrapper =
            wrapperObject.GetComponent<RectTransform>();

        wrapper.SetParent(collectionContent, false);

        wrapper.anchorMin = new Vector2(0.5f, 0.5f);
        wrapper.anchorMax = new Vector2(0.5f, 0.5f);
        wrapper.pivot = new Vector2(0.5f, 0.5f);
        wrapper.sizeDelta = cellSize;
        wrapper.localScale = Vector3.one;
        wrapper.localRotation = Quaternion.identity;

        generatedObjects.Add(wrapperObject);

        Button card = Instantiate(
            monsterCardPrefab,
            wrapper,
            false
        );

        card.gameObject.name =
            "Monster_Card_" + monster.monsterName;

        RectTransform cardRect =
            card.GetComponent<RectTransform>();

        RectTransform prefabRect =
            monsterCardPrefab.GetComponent<RectTransform>();

        Vector2 designSize = prefabRect.rect.size;

        if (designSize.x <= 0f || designSize.y <= 0f)
        {
            designSize = new Vector2(422f, 478f);
        }

        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = designSize;
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.localRotation = Quaternion.identity;

        float width =
            Mathf.Max(1f, cellSize.x - cardPadding * 2f);

        float height =
            Mathf.Max(1f, cellSize.y - cardPadding * 2f);

        float fitScale = Mathf.Min(
            width / designSize.x,
            height / designSize.y
        );

        float scale =
            fitScale * Mathf.Max(0.01f, cardScaleMultiplier);

        cardRect.localScale =
            new Vector3(scale, scale, 1f);

        MonsterCardView cardView =
            card.GetComponent<MonsterCardView>();

        if (cardView == null)
        {
            Debug.LogError(
                "Monster Card Prefab is missing MonsterCardView!"
            );

            card.gameObject.SetActive(false);
            return;
        }

        cardView.DisplayMonster(
            monster,
            BuildEffectDescription(monster),
            CountCopies(monster.monsterId) > 0
        );

        MonsterData capturedMonster = monster;

        card.onClick.RemoveAllListeners();

        card.onClick.AddListener(
            () => PreviewMonster(capturedMonster)
        );

        CollectionEntry entry = new CollectionEntry();
        entry.monster = monster;
        entry.view = cardView;
        entry.wrapper = wrapper;

        collectionEntries.Add(entry);

        card.gameObject.SetActive(true);
    }

    // ==========================================
    // OPTIONAL DIRECT CARD SYSTEM
    // ==========================================

    private void CreateDirectCard(MonsterData monster)
    {
        Button card = Instantiate(
            monsterCardPrefab,
            collectionContent,
            false
        );

        generatedObjects.Add(card.gameObject);

        MonsterCardView view =
            card.GetComponent<MonsterCardView>();

        if (view == null)
        {
            card.gameObject.SetActive(false);
            Debug.LogError(
                "Monster Card Prefab missing MonsterCardView!"
            );
            return;
        }

        MonsterData capturedMonster = monster;

        card.onClick.RemoveAllListeners();

        card.onClick.AddListener(
            () => PreviewMonster(capturedMonster)
        );

        view.DisplayMonster(
            monster,
            BuildEffectDescription(monster),
            CountCopies(monster.monsterId) > 0
        );

        CollectionEntry entry = new CollectionEntry();
        entry.monster = monster;
        entry.view = view;

        collectionEntries.Add(entry);

        card.gameObject.SetActive(true);
    }

    // ==========================================
    // COLLECTION HEIGHT
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

        int columns = 1;

        if (grid.constraint ==
            GridLayoutGroup.Constraint.FixedColumnCount)
        {
            columns = Mathf.Max(1, grid.constraintCount);
        }
        else
        {
            float width =
                gridRect.rect.width -
                grid.padding.left -
                grid.padding.right;

            columns = Mathf.Max(
                1,
                Mathf.FloorToInt(
                    (width + grid.spacing.x) /
                    Mathf.Max(
                        1f,
                        grid.cellSize.x + grid.spacing.x
                    )
                )
            );
        }

        int rows = Mathf.CeilToInt(
            collectionEntries.Count / (float)columns
        );

        float gridHeight =
            grid.padding.top +
            grid.padding.bottom +
            rows * grid.cellSize.y +
            Mathf.Max(0, rows - 1) * grid.spacing.y;

        gridRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            gridHeight
        );

        if (collectionSectionLayout != null)
        {
            float titleHeight = collectionTitle != null
                ? collectionTitle.rect.height
                : 50f;

            float height =
                collectionTopPadding +
                titleHeight +
                gridHeight +
                collectionBottomPadding;

            collectionSectionLayout.preferredHeight =
                Mathf.Max(minimumCollectionHeight, height);
        }

        Canvas.ForceUpdateCanvases();
    }

    // ==========================================
    // REFRESH DISPLAY
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

            entry.view.DisplayMonster(
                entry.monster,
                BuildEffectDescription(entry.monster),
                CountCopies(entry.monster.monsterId) > 0
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
        if (string.IsNullOrWhiteSpace(effectName))
            return "";

        foreach (CardEffectDescription effect in effectDescriptions)
        {
            if (effect == null)
                continue;

            string configuredName =
                effect.effectName != null
                ? effect.effectName.Trim()
                : "";

            if (string.Equals(
                configuredName,
                effectName.Trim(),
                StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(
                    effect.description
                ))
                {
                    return effectName + ":\n" +
                           effect.description.Trim();
                }
            }
        }

        return effectName;
    }

    // ==========================================
    // OPEN CARD FROM COLLECTION
    // ==========================================

    public void PreviewMonster(MonsterData monster)
    {
        if (monster == null)
            return;

        selectedDeckSlot = -1;

        OpenMonsterPreview(monster);
    }

    // ==========================================
    // NEW - OPEN CARD FROM DECK SLOT
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

        if (monster == null)
        {
            ShowFeedback("Monster could not be found.");
            return;
        }

        selectedDeckSlot = slotIndex;

        OpenMonsterPreview(monster);
    }

    // ==========================================
    // SHARED LARGE CARD PREVIEW
    // ==========================================

    private void OpenMonsterPreview(MonsterData monster)
    {
        selectedMonster = monster;

        int copies = CountCopies(monster.monsterId);

        if (previewCardView != null)
        {
            previewCardView.DisplayMonster(
                monster,
                BuildEffectDescription(monster),
                copies > 0
            );
        }

        string effect1 =
            string.IsNullOrWhiteSpace(monster.moveOne)
            ? "None"
            : monster.moveOne;

        string effect2 =
            string.IsNullOrWhiteSpace(monster.moveTwo)
            ? "None"
            : monster.moveTwo;

        string details =
            monster.monsterName + "\n\n" +
            "HP: " + monster.health + "\n" +
            "ATK: " + monster.attack + "\n" +
            "DEF: " + monster.defence + "\n" +
            "INT: " + monster.intelligence + "\n" +
            "Summon Cost: " +
            monster.summonCost + " SP\n\n" +
            "Effect 1: " + effect1 + "\n" +
            "Effect 2: " + effect2 + "\n\n" +
            "Copies In Deck: " +
            copies + "/" + MaxCopiesPerMonster;

        if (monsterDetailsText != null)
            monsterDetailsText.text = details;

        if (selectedMonsterInfoText != null)
            selectedMonsterInfoText.text = details;

        if (addToDeckButton != null)
        {
            addToDeckButton.interactable =
                copies < MaxCopiesPerMonster &&
                currentDeck.monsterIds.Count < MaxDeckSize;
        }

        // Show Remove only when a specific
        // deck slot was clicked.
        if (removeFromDeckButton != null)
        {
            bool openedFromDeck =
                selectedDeckSlot >= 0;

            removeFromDeckButton.gameObject.SetActive(
                openedFromDeck
            );
        }

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(true);
    }

    // ==========================================
    // CLOSE PREVIEW
    // ==========================================

    public void CloseMonsterDetails()
    {
        selectedMonster = null;
        selectedDeckSlot = -1;

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(false);
    }

    // ==========================================
    // ADD MONSTER - MAXIMUM THREE COPIES
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

        int copies =
            CountCopies(selectedMonster.monsterId);

        if (copies >= MaxCopiesPerMonster)
        {
            ShowFeedback(
                "You can only have 3 copies of a monster."
            );
            return;
        }

        currentDeck.monsterIds.Add(
            selectedMonster.monsterId
        );

        ShowFeedback(
            "Added " + selectedMonster.monsterName +
            " (" + (copies + 1) + "/3)"
        );

        RefreshAll();
        CloseMonsterDetails();
    }

    // ==========================================
    // NEW - REMOVE ONE SPECIFIC DECK COPY
    // ==========================================

    public void RemoveSelectedDeckCopy()
    {
        if (selectedDeckSlot < 0 ||
            selectedDeckSlot >= currentDeck.monsterIds.Count)
        {
            ShowFeedback(
                "Open a monster from a deck slot to remove it."
            );
            return;
        }

        MonsterData monster = FindMonster(
            currentDeck.monsterIds[selectedDeckSlot]
        );

        currentDeck.monsterIds.RemoveAt(selectedDeckSlot);

        ShowFeedback(
            monster == null
            ? "Removed one monster copy."
            : "Removed one copy of " +
              monster.monsterName
        );

        RefreshAll();
        CloseMonsterDetails();
    }

    // ==========================================
    // UPDATE DECK SLOT IMAGES
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

            if (button == null)
                continue;

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

                // Display artwork without the
                // original purple slot colour.
                button.image.color = Color.white;

                // Colour Tint can tint the image during
                // Hover, Pressed or Selected states.
                // Turn it off for occupied card slots.
                if (removeDeckSlotTint)
                {
                    button.transition =
                        Selectable.Transition.None;
                }
            }
            else
            {
                button.transition =
                    originalTransitions[i];

                button.image.sprite =
                    originalSlotSprites[i];

                button.image.type =
                    originalSlotImageTypes[i];

                button.image.preserveAspect =
                    originalPreserveAspect[i];

                button.image.color =
                    originalSlotColors[i];
            }
        }
    }

    private string ShortName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "?";

        const int maxLength = 8;

        if (name.Length <= maxLength)
            return name;

        return name.Substring(
            0,
            maxLength - 3
        ) + "...";
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
    // RANDOM DECK - UP TO THREE COPIES EACH
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

        // Make a pool with three entries per monster.
        List<string> pool = new List<string>();

        foreach (MonsterData monster in available)
        {
            for (int i = 0; i < MaxCopiesPerMonster; i++)
            {
                pool.Add(monster.monsterId);
            }
        }

        // Fisher-Yates shuffle.
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int randomIndex =
                UnityEngine.Random.Range(0, i + 1);

            string temp = pool[i];
            pool[i] = pool[randomIndex];
            pool[randomIndex] = temp;
        }

        currentDeck.monsterIds.Clear();

        int amount = Mathf.Min(
            MaxDeckSize,
            pool.Count
        );

        for (int i = 0; i < amount; i++)
        {
            currentDeck.monsterIds.Add(pool[i]);
        }

        CloseMonsterDetails();
        RefreshAll();

        ShowFeedback(
            "Random deck created with " +
            amount + " cards."
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

        if (string.IsNullOrWhiteSpace(
            currentDeck.deckName))
        {
            ShowFeedback("Please enter a deck name.");
            return;
        }

        // Validate IDs and copy limits before saving.
        Dictionary<string, int> copies =
            new Dictionary<string, int>();

        if (currentDeck.monsterIds.Count > MaxDeckSize)
        {
            ShowFeedback("Deck exceeds 34 cards.");
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

            int count = 0;
            copies.TryGetValue(id, out count);
            count++;

            if (count > MaxCopiesPerMonster)
            {
                ShowFeedback(
                    "Deck contains more than 3 copies."
                );
                return;
            }

            copies[id] = count;
        }

        bool saved =
            DeckSaveSystem.SaveDeck(currentDeck);

        ShowFeedback(
            saved
            ? "Deck saved: " + currentDeck.deckName
            : "Deck save failed. Check Console."
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
    // RELOAD COLLECTION
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
