
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

    [Header("Deck Controls")]
    [SerializeField] private Button saveDeckButton;
    [SerializeField] private Button clearDeckButton;
    [SerializeField] private Button randomDeckButton;

    [Header("Optional Feedback")]
    [SerializeField] private TMP_Text feedbackText;

    [Header("Monster Details Popup")]
    [SerializeField] private GameObject monsterDetailsPanel;
    [SerializeField] private Button closeDetailsButton;
    [SerializeField] private MonsterCardView previewCardView;

    [Header("Card Management Panel")]
    [SerializeField] private GameObject cardManagementPanel;
    [SerializeField] private Button addCopyButton;
    [SerializeField] private Button removeCopyButton;
    [SerializeField] private TMP_Text copiesLabel;
    [SerializeField] private TMP_Text copiesCount;

    [Header("Effect Description Library")]
    [SerializeField]
    private List<CardEffectDescription>
        effectDescriptions = new List<CardEffectDescription>();

    [Header("Deck Slot Appearance")]
    [SerializeField] private bool showCardArtworkInDeckSlots = true;
    [SerializeField] private bool removeDeckSlotTint = true;

    [Header("Collection Layout")]
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
    private Image.Type[] originalSlotTypes;
    private bool[] originalPreserveAspect;
    private Selectable.Transition[] originalTransitions;

    private MonsterData selectedMonster;

    // -1 = opened from collection.
    // 0 to 33 = opened from a deck slot.
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

    // =========================================
    // INITIALISATION
    // =========================================

    private void Start()
    {
        SetupDeckSlots();
        SetupButtons();

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(false);

        LoadCollection();
        LoadCurrentDeck();

        BuildCollectionCards();
        RefreshAll();
    }

    // =========================================
    // BUTTON REGISTRATION
    // =========================================

    private void SetupButtons()
    {
        if (saveDeckButton != null)
            saveDeckButton.onClick.AddListener(SaveCurrentDeck);

        if (clearDeckButton != null)
            clearDeckButton.onClick.AddListener(ClearDeck);

        if (randomDeckButton != null)
            randomDeckButton.onClick.AddListener(RandomiseDeck);

        if (addCopyButton != null)
            addCopyButton.onClick.AddListener(AddSelectedMonster);

        if (removeCopyButton != null)
            removeCopyButton.onClick.AddListener(RemoveSelectedCopy);

        if (closeDetailsButton != null)
            closeDetailsButton.onClick.AddListener(CloseMonsterDetails);

        if (copiesLabel != null)
            copiesLabel.text = "Copies in Deck";

        Debug.Log("DeckBuilder: Buttons registered.");
    }

    // =========================================
    // SET UP DECK SLOTS
    // =========================================

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
                " slots. Expected 34."
            );
        }

        originalSlotSprites = new Sprite[count];
        originalSlotColors = new Color[count];
        originalSlotTypes = new Image.Type[count];
        originalPreserveAspect = new bool[count];
        originalTransitions = new Selectable.Transition[count];

        for (int i = 0; i < count; i++)
        {
            int index = i;
            Button button = deckSlotButtons[i];

            originalTransitions[i] = button.transition;

            if (button.image != null)
            {
                originalSlotSprites[i] = button.image.sprite;
                originalSlotColors[i] = button.image.color;
                originalSlotTypes[i] = button.image.type;
                originalPreserveAspect[i] =
                    button.image.preserveAspect;
            }

            button.onClick.AddListener(
                () => OnDeckSlotClicked(index)
            );
        }
    }

    // =========================================
    // LOAD COLLECTION
    // =========================================

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

    // =========================================
    // LOAD SAVED DECK
    // =========================================

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

        ValidateLoadedDeck();

        if (deckNameInput != null)
            deckNameInput.text = currentDeck.deckName;
    }

    private void ValidateLoadedDeck()
    {
        List<string> validIds = new List<string>();

        Dictionary<string, int> counts =
            new Dictionary<string, int>();

        foreach (string id in currentDeck.monsterIds)
        {
            if (validIds.Count >= MaxDeckSize)
                break;

            if (FindMonster(id) == null)
                continue;

            int count = 0;
            counts.TryGetValue(id, out count);

            if (count >= MaxCopiesPerMonster)
                continue;

            validIds.Add(id);
            counts[id] = count + 1;
        }

        currentDeck.monsterIds = validIds;
    }

    // =========================================
    // MONSTER LOOKUP
    // =========================================

    private MonsterData FindMonster(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
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

    private int CountCopies(string monsterId)
    {
        if (string.IsNullOrWhiteSpace(monsterId))
            return 0;

        int result = 0;

        foreach (string id in currentDeck.monsterIds)
        {
            if (id == monsterId)
                result++;
        }

        return result;
    }

    // =========================================
    // BUILD MONSTER COLLECTION
    // =========================================

    private void BuildCollectionCards()
    {
        if (collectionContent == null ||
            monsterCardPrefab == null)
        {
            Debug.LogError(
                "DeckBuilder: Collection Content or " +
                "Monster Card Prefab is missing!"
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

        Vector2 cellSize =
            grid != null
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
    }

    // =========================================
    // CARD WRAPPER SCALING SYSTEM
    // =========================================

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
            designSize = new Vector2(422f, 478f);

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

        float scale = Mathf.Min(
            width / designSize.x,
            height / designSize.y
        );

        scale *= Mathf.Max(0.01f, cardScaleMultiplier);

        cardRect.localScale =
            new Vector3(scale, scale, 1f);

        MonsterCardView view =
            card.GetComponent<MonsterCardView>();

        if (view == null)
        {
            Debug.LogError(
                "Monster Card Prefab is missing MonsterCardView!"
            );

            card.gameObject.SetActive(false);
            return;
        }

        view.DisplayMonster(
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
        entry.view = view;
        entry.wrapper = wrapper;

        collectionEntries.Add(entry);

        card.gameObject.SetActive(true);
    }

    // =========================================
    // OPTIONAL DIRECT CARD SYSTEM
    // =========================================

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
            Debug.LogError(
                "Monster Card Prefab missing MonsterCardView!"
            );

            card.gameObject.SetActive(false);
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
    }

    // =========================================
    // COLLECTION SCROLL HEIGHT
    // =========================================

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

        float height =
            grid.padding.top +
            grid.padding.bottom +
            rows * grid.cellSize.y +
            Mathf.Max(0, rows - 1) * grid.spacing.y;

        gridRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            height
        );

        if (collectionSectionLayout != null)
        {
            float titleHeight =
                collectionTitle != null
                    ? collectionTitle.rect.height
                    : 50f;

            float sectionHeight =
                collectionTopPadding +
                titleHeight +
                height +
                collectionBottomPadding;

            collectionSectionLayout.preferredHeight =
                Mathf.Max(
                    minimumCollectionHeight,
                    sectionHeight
                );
        }

        Canvas.ForceUpdateCanvases();
    }

    // =========================================
    // REFRESH UI
    // =========================================

    private void RefreshAll()
    {
        RefreshDeckDisplay();
        RefreshCollectionDisplay();
        RefreshCardManagement();
    }

    private void RefreshCollectionDisplay()
    {
        foreach (CollectionEntry entry in collectionEntries)
        {
            if (entry == null ||
                entry.monster == null ||
                entry.view == null)
            {
                continue;
            }

            entry.view.DisplayMonster(
                entry.monster,
                BuildEffectDescription(entry.monster),
                CountCopies(entry.monster.monsterId) > 0
            );
        }
    }

    // =========================================
    // EFFECT DESCRIPTION LIBRARY
    // =========================================

    private string BuildEffectDescription(MonsterData monster)
    {
        if (monster == null)
            return "";

        List<string> descriptions = new List<string>();

        if (!string.IsNullOrWhiteSpace(monster.moveOne))
            descriptions.Add(
                GetEffectDescription(monster.moveOne)
            );

        if (!string.IsNullOrWhiteSpace(monster.moveTwo))
            descriptions.Add(
                GetEffectDescription(monster.moveTwo)
            );

        return string.Join("\n\n", descriptions);
    }

    private string GetEffectDescription(string effectName)
    {
        if (string.IsNullOrWhiteSpace(effectName))
            return "";

        foreach (CardEffectDescription effect in effectDescriptions)
        {
            if (effect == null)
                continue;

            if (string.Equals(
                effect.effectName?.Trim(),
                effectName.Trim(),
                StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(effect.description))
                {
                    return effectName + ":\n" +
                           effect.description.Trim();
                }
            }
        }

        return effectName;
    }

    // =========================================
    // OPEN MONSTER FROM COLLECTION
    // =========================================

    public void PreviewMonster(MonsterData monster)
    {
        if (monster == null)
            return;

        selectedDeckSlot = -1;
        OpenMonsterPreview(monster);
    }

    // =========================================
    // OPEN MONSTER FROM DECK SLOT
    // =========================================

    private void OnDeckSlotClicked(int index)
    {
        if (index < 0 ||
            index >= currentDeck.monsterIds.Count)
        {
            return;
        }

        MonsterData monster =
            FindMonster(currentDeck.monsterIds[index]);

        if (monster == null)
        {
            ShowFeedback("Monster could not be found.");
            return;
        }

        selectedDeckSlot = index;
        OpenMonsterPreview(monster);
    }

    // =========================================
    // LARGE MONSTER PREVIEW
    // =========================================

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

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(true);

        if (cardManagementPanel != null)
            cardManagementPanel.SetActive(true);

        RefreshCardManagement();
    }

    // =========================================
    // CARD MANAGEMENT UI
    // =========================================

    private void RefreshCardManagement()
    {
        if (selectedMonster == null)
            return;

        int copies =
            CountCopies(selectedMonster.monsterId);

        if (copiesLabel != null)
            copiesLabel.text = "Copies in Deck";

        if (copiesCount != null)
        {
            copiesCount.text =
                copies + " / " + MaxCopiesPerMonster;
        }

        bool canAdd =
            copies < MaxCopiesPerMonster &&
            currentDeck.monsterIds.Count < MaxDeckSize;

        bool canRemove = copies > 0;

        if (addCopyButton != null)
            addCopyButton.interactable = canAdd;

        if (removeCopyButton != null)
            removeCopyButton.interactable = canRemove;
    }

    // =========================================
    // ADD ONE COPY
    // =========================================

    public void AddSelectedMonster()
    {
        if (selectedMonster == null)
        {
            ShowFeedback("Select a monster first.");
            return;
        }

        if (string.IsNullOrWhiteSpace(
            selectedMonster.monsterId))
        {
            ShowFeedback("Monster has an invalid ID.");
            return;
        }

        if (currentDeck.monsterIds.Count >= MaxDeckSize)
        {
            ShowFeedback("Deck is full!");
            RefreshCardManagement();
            return;
        }

        int copies =
            CountCopies(selectedMonster.monsterId);

        if (copies >= MaxCopiesPerMonster)
        {
            ShowFeedback("Maximum 3 copies per monster.");
            RefreshCardManagement();
            return;
        }

        currentDeck.monsterIds.Add(
            selectedMonster.monsterId
        );

        ShowFeedback(
            "Added " + selectedMonster.monsterName +
            " (" + (copies + 1) + "/3)"
        );

        // Keep preview open.
        RefreshAll();
    }

    // =========================================
    // REMOVE ONE COPY
    // =========================================

    public void RemoveSelectedCopy()
    {
        if (selectedMonster == null)
        {
            ShowFeedback("Select a monster first.");
            return;
        }

        string id = selectedMonster.monsterId;

        int index = -1;

        if (selectedDeckSlot >= 0 &&
            selectedDeckSlot < currentDeck.monsterIds.Count &&
            currentDeck.monsterIds[selectedDeckSlot] == id)
        {
            index = selectedDeckSlot;
        }
        else
        {
            index = currentDeck.monsterIds.LastIndexOf(id);
        }

        if (index < 0)
        {
            ShowFeedback("No copies to remove.");
            RefreshCardManagement();
            return;
        }

        currentDeck.monsterIds.RemoveAt(index);

        selectedDeckSlot = -1;

        ShowFeedback(
            "Removed one copy of " +
            selectedMonster.monsterName
        );

        // Keep preview open.
        RefreshAll();
    }

    // =========================================
    // CLOSE CARD PREVIEW
    // =========================================

    public void CloseMonsterDetails()
    {
        selectedMonster = null;
        selectedDeckSlot = -1;

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(false);
    }

    // =========================================
    // DECK SLOT APPEARANCE
    // =========================================

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

            bool occupied = i < count;

            MonsterData monster =
                occupied
                    ? FindMonster(currentDeck.monsterIds[i])
                    : null;

            bool useArtwork =
                showCardArtworkInDeckSlots &&
                occupied &&
                cardSprite != null;

            TMP_Text label =
                button.GetComponentInChildren<TMP_Text>(true);

            if (label != null)
            {
                label.text =
                    occupied
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

                if (removeDeckSlotTint)
                {
                    button.transition =
                        Selectable.Transition.None;
                }
            }
            else
            {
                button.transition = originalTransitions[i];
                button.image.sprite = originalSlotSprites[i];
                button.image.color = originalSlotColors[i];
                button.image.type = originalSlotTypes[i];

                button.image.preserveAspect =
                    originalPreserveAspect[i];
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

        return name.Substring(0, maxLength - 3) + "...";
    }

    // =========================================
    // CLEAR DECK
    // =========================================

    public void ClearDeck()
    {
        currentDeck.monsterIds.Clear();

        RefreshAll();

        ShowFeedback("Deck cleared.");
    }

    // =========================================
    // RANDOMISE DECK
    // =========================================

    public void RandomiseDeck()
    {
        List<string> pool = new List<string>();

        HashSet<string> seen =
            new HashSet<string>();

        foreach (MonsterData monster in collection)
        {
            if (monster == null ||
                string.IsNullOrWhiteSpace(monster.monsterId))
            {
                continue;
            }

            if (!seen.Add(monster.monsterId))
                continue;

            for (int i = 0; i < MaxCopiesPerMonster; i++)
            {
                pool.Add(monster.monsterId);
            }
        }

        for (int i = pool.Count - 1; i > 0; i--)
        {
            int randomIndex =
                UnityEngine.Random.Range(0, i + 1);

            string temp = pool[i];
            pool[i] = pool[randomIndex];
            pool[randomIndex] = temp;
        }

        currentDeck.monsterIds.Clear();

        int amount = Mathf.Min(MaxDeckSize, pool.Count);

        for (int i = 0; i < amount; i++)
        {
            currentDeck.monsterIds.Add(pool[i]);
        }

        RefreshAll();

        ShowFeedback(
            "Random deck generated with " +
            amount + " cards."
        );
    }

    // =========================================
    // SAVE DECK
    // =========================================

    public void SaveCurrentDeck()
    {
        if (deckNameInput != null)
            currentDeck.deckName = deckNameInput.text.Trim();

        if (string.IsNullOrWhiteSpace(currentDeck.deckName))
        {
            ShowFeedback("Enter a deck name.");
            return;
        }

        if (currentDeck.monsterIds.Count > MaxDeckSize)
        {
            ShowFeedback("Deck exceeds 34 cards.");
            return;
        }

        Dictionary<string, int> counts =
            new Dictionary<string, int>();

        foreach (string id in currentDeck.monsterIds)
        {
            if (FindMonster(id) == null)
            {
                ShowFeedback(
                    "Deck contains an unavailable monster."
                );
                return;
            }

            int copies = 0;
            counts.TryGetValue(id, out copies);

            copies++;

            if (copies > MaxCopiesPerMonster)
            {
                ShowFeedback(
                    "A monster has more than 3 copies."
                );
                return;
            }

            counts[id] = copies;
        }

        bool saved =
            DeckSaveSystem.SaveDeck(currentDeck);

        ShowFeedback(
            saved
                ? "Deck saved: " + currentDeck.deckName
                : "Deck saving failed. Check Console."
        );
    }

    // =========================================
    // FEEDBACK
    // =========================================

    private void ShowFeedback(string message)
    {
        Debug.Log(message);

        if (feedbackText != null)
            feedbackText.text = message;
    }

    // =========================================
    // RELOAD COLLECTION
    // =========================================

    public void ReloadCollection()
    {
        CloseMonsterDetails();

        LoadCollection();
        LoadCurrentDeck();

        BuildCollectionCards();
        RefreshAll();
    }
}
