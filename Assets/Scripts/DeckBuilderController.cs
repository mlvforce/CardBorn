
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private const int MaxMonsterCopies = 3;

    private const string MonsterPrefix = "monster:";
    private const string SpellPrefix = "spell:";

    // INSPECTOR REFERENCES

    [Header("Deck UI")]
    [SerializeField] private Transform deckSlotsParent;
    [SerializeField] private TMP_InputField deckNameInput;
    [SerializeField] private TMP_Text deckCountText;

    [Header("Monster Collection")]
    [SerializeField] private Transform collectionContent;
    [SerializeField] private Button monsterCardPrefab;

    [Header("Spell Collection")]
    [SerializeField] private SpellLibrary spellLibrary;
    [SerializeField] private Button spellCardPrefab;

    [Header("Deck Controls")]
    [SerializeField] private Button saveDeckButton;
    [SerializeField] private Button clearDeckButton;
    [SerializeField] private Button randomDeckButton;

    [Header("Optional Feedback")]
    [SerializeField] private TMP_Text feedbackText;

    [Header("Card Details Popup")]
    [SerializeField] private GameObject monsterDetailsPanel;
    [SerializeField] private Button closeDetailsButton;

    [Header("Large Card Previews")]
    [SerializeField] private MonsterCardView previewCardView;
    [SerializeField] private SpellCardView previewSpellCardView;

    [Header("Card Management Panel")]
    [SerializeField] private GameObject cardManagementPanel;
    [SerializeField] private Button addCopyButton;
    [SerializeField] private Button removeCopyButton;
    [SerializeField] private TMP_Text copiesLabel;
    [SerializeField] private TMP_Text copiesCount;

    [Header("Monster Effect Description Library")]
    [SerializeField]
    private List<CardEffectDescription> effectDescriptions =
        new List<CardEffectDescription>();

    [Header("Deck Slot Appearance")]
    [SerializeField]
    private bool showCardArtworkInDeckSlots = true;

    [SerializeField]
    private bool removeDeckSlotTint = true;

    [Header("Miniature Card Settings")]
    [SerializeField]
    private float miniCardScaleMultiplier = 1f;

    [SerializeField]
    private float miniCardPadding = 0f;

    [Header("Collection Layout")]
    [SerializeField]
    private LayoutElement collectionSectionLayout;

    [SerializeField]
    private RectTransform collectionTitle;

    [SerializeField]
    private float collectionTopPadding = 80f;

    [SerializeField]
    private float collectionBottomPadding = 40f;

    [SerializeField]
    private float minimumCollectionHeight = 700f;

    [Header("Collection Card Scaling")]
    [SerializeField]
    private bool useCardWrappers = true;

    [SerializeField]
    private float cardScaleMultiplier = 1f;

    [SerializeField]
    private float cardPadding = 0f;

    // RUNTIME DATA

    private List<MonsterData> monsters =
        new List<MonsterData>();

    private List<SpellData> spells =
        new List<SpellData>();

    private DeckData currentDeck = new DeckData();

    private MonsterData selectedMonster;
    private SpellData selectedSpell;

    
    private int selectedDeckSlot = -1;

    private Button[] deckSlotButtons;

    private Sprite[] originalSlotSprites;
    private Color[] originalSlotColors;
    private Image.Type[] originalSlotTypes;
    private bool[] originalPreserveAspect;
    private Selectable.Transition[] originalTransitions;

    private GameObject[] miniCardObjects;
    private string[] miniCardKeys;

    private DeckSlotSwitcher deckSlotSwitcher;

    // COLLECTION DATA

    private class CollectionEntry
    {
        public MonsterData monster;
        public SpellData spell;

        public MonsterCardView monsterView;
        public SpellCardView spellView;
    }

    private readonly List<CollectionEntry>
        collectionEntries = new List<CollectionEntry>();

    private readonly List<GameObject>
        generatedObjects = new List<GameObject>();

    // STARTUP

    private void Start()
    {
        deckSlotSwitcher =
            GetComponent<DeckSlotSwitcher>();

        SetupDeckSlots();
        SetupButtons();

        LoadCollection();
        LoadSpells();

        
        LoadCurrentDeck();

        BuildCollectionCards();

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(false);

        RefreshAll();

        Debug.Log(
            "Deck Builder initialised successfully."
        );
    }

    // BUTTON SETUP

    private void SetupButtons()
    {
        if (saveDeckButton != null)
        {
            saveDeckButton.onClick.AddListener(
                SaveCurrentDeck
            );
        }

        if (clearDeckButton != null)
        {
            clearDeckButton.onClick.AddListener(
                ClearDeck
            );
        }

        if (randomDeckButton != null)
        {
            randomDeckButton.onClick.AddListener(
                RandomiseDeck
            );
        }

        if (addCopyButton != null)
        {
            addCopyButton.onClick.AddListener(
                AddSelectedCard
            );
        }

        if (removeCopyButton != null)
        {
            removeCopyButton.onClick.AddListener(
                RemoveSelectedCopy
            );
        }

        if (closeDetailsButton != null)
        {
            closeDetailsButton.onClick.AddListener(
                CloseMonsterDetails
            );
        }

        if (copiesLabel != null)
            copiesLabel.text = "Copies in Deck";
    }

    // SET UP DECK SLOTS

    private void SetupDeckSlots()
    {
        if (deckSlotsParent == null)
        {
            Debug.LogError(
                "Deck Slots Parent is missing!"
            );

            return;
        }

        deckSlotButtons =
            deckSlotsParent.GetComponentsInChildren<Button>(
                true
            );

        int count = deckSlotButtons.Length;

        if (count != MaxDeckSize)
        {
            Debug.LogWarning(
                "Expected 34 deck slots but found " +
                count
            );
        }

        originalSlotSprites = new Sprite[count];
        originalSlotColors = new Color[count];
        originalSlotTypes = new Image.Type[count];
        originalPreserveAspect = new bool[count];
        originalTransitions =
            new Selectable.Transition[count];

        miniCardObjects = new GameObject[count];
        miniCardKeys = new string[count];

        for (int i = 0; i < count; i++)
        {
            Button button = deckSlotButtons[i];

            if (button == null)
                continue;

            int index = i;

            originalTransitions[i] = button.transition;

            if (button.image != null)
            {
                originalSlotSprites[i] =
                    button.image.sprite;

                originalSlotColors[i] =
                    button.image.color;

                originalSlotTypes[i] =
                    button.image.type;

                originalPreserveAspect[i] =
                    button.image.preserveAspect;
            }

            button.onClick.AddListener(
                () => OnDeckSlotClicked(index)
            );

            Transform oldArtwork =
                button.transform.Find("Card_Artwork");

            if (oldArtwork != null)
            {
                oldArtwork.gameObject.SetActive(false);
            }
        }
    }

    // LOAD MONSTERS

    public void LoadCollection()
    {
        monsters = MonsterSaveSystem.LoadMonsters();

        if (monsters == null)
        {
            monsters = new List<MonsterData>();
        }

        Debug.Log(
            "Loaded " + monsters.Count + " monsters."
        );
    }

    // LOAD SPELLS

    private void LoadSpells()
    {
        spells = new List<SpellData>();

        if (spellLibrary == null)
        {
            Debug.LogWarning(
                "Spell Library is not assigned!"
            );

            return;
        }

        IReadOnlyList<SpellData> available =
            spellLibrary.GetAllSpells();

        if (available == null)
            return;

        HashSet<string> usedIds =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

        foreach (SpellData spell in available)
        {
            if (spell == null ||
                string.IsNullOrWhiteSpace(spell.spellId))
            {
                continue;
            }

            if (!usedIds.Add(spell.spellId.Trim()))
            {
                Debug.LogWarning(
                    "Duplicate spell ID: " +
                    spell.spellId
                );

                continue;
            }

            spells.Add(spell);
        }

        Debug.Log(
            "Loaded " + spells.Count + " spells."
        );
    }

    // CARD IDENTIFIERS

    private string MonsterKey(string id)
    {
        return MonsterPrefix + id;
    }

    private string SpellKey(string id)
    {
        return SpellPrefix + id;
    }

    private bool IsSpellEntry(string entry)
    {
        return !string.IsNullOrWhiteSpace(entry) &&
            entry.StartsWith(
                SpellPrefix,
                StringComparison.OrdinalIgnoreCase
            );
    }

    private string GetMonsterId(string entry)
    {
        if (string.IsNullOrWhiteSpace(entry))
            return "";

        if (entry.StartsWith(
            MonsterPrefix,
            StringComparison.OrdinalIgnoreCase))
        {
            return entry.Substring(
                MonsterPrefix.Length
            );
        }

        return entry;
    }

    private string GetSpellId(string entry)
    {
        if (!IsSpellEntry(entry))
            return "";

        return entry.Substring(SpellPrefix.Length);
    }

    // FIND MONSTER

    private MonsterData FindMonster(string id)
    {
        foreach (MonsterData monster in monsters)
        {
            if (monster != null &&
                monster.monsterId == id)
            {
                return monster;
            }
        }

        return null;
    }

    // FIND SPELL

    private SpellData FindSpell(string id)
    {
        if (spellLibrary == null)
            return null;

        return spellLibrary.GetSpellById(id);
    }

    // COUNT COPIES

    private int CountMonsterCopies(string id)
    {
        int count = 0;

        foreach (string entry in currentDeck.monsterIds)
        {
            if (!IsSpellEntry(entry) &&
                GetMonsterId(entry) == id)
            {
                count++;
            }
        }

        return count;
    }

    private int CountSpellCopies(string id)
    {
        int count = 0;

        foreach (string entry in currentDeck.monsterIds)
        {
            if (IsSpellEntry(entry) &&
                string.Equals(
                    GetSpellId(entry),
                    id,
                    StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        return count;
    }

    private int GetMaximumCopies(SpellData spell)
    {
        if (spell == null)
            return 0;

        return Mathf.Clamp(
            spell.maximumCopies,
            1,
            3
        );
    }

    // LOAD ORIGINAL SAVED DECK

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

            currentDeck.deckName =
                "My First Deck";

            currentDeck.monsterIds =
                new List<string>();
        }

        if (currentDeck.monsterIds == null)
        {
            currentDeck.monsterIds =
                new List<string>();
        }

        ValidateLoadedDeck();

        if (deckNameInput != null)
        {
            deckNameInput.SetTextWithoutNotify(
                currentDeck.deckName
            );
        }
    }

    // VALIDATE LOADED DECK

    private void ValidateLoadedDeck()
    {
        if (currentDeck == null)
            currentDeck = new DeckData();

        if (currentDeck.monsterIds == null)
        {
            currentDeck.monsterIds =
                new List<string>();
        }

        List<string> valid =
            new List<string>();

        Dictionary<string, int> counts =
            new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase
            );

        foreach (string entry in currentDeck.monsterIds)
        {
            if (valid.Count >= MaxDeckSize)
                break;

            if (string.IsNullOrWhiteSpace(entry))
                continue;

            string key;
            int maximum;

            if (IsSpellEntry(entry))
            {
                SpellData spell =
                    FindSpell(GetSpellId(entry));

                if (spell == null)
                    continue;

                key = SpellKey(spell.spellId);

                maximum = GetMaximumCopies(spell);
            }
            else
            {
                MonsterData monster =
                    FindMonster(GetMonsterId(entry));

                if (monster == null)
                    continue;

                key = MonsterKey(monster.monsterId);

                maximum = MaxMonsterCopies;
            }

            int amount = 0;

            counts.TryGetValue(
                key,
                out amount
            );

            if (amount >= maximum)
                continue;

            valid.Add(key);

            counts[key] = amount + 1;
        }

        currentDeck.monsterIds = valid;
    }

    
    // BUILD COLLECTION

    private void BuildCollectionCards()
    {
        if (collectionContent == null)
        {
            Debug.LogError(
                "Collection Content is missing!"
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

        if (monsterCardPrefab != null)
        {
            foreach (MonsterData monster in monsters)
            {
                if (monster != null)
                {
                    CreateMonsterCard(
                        monster,
                        cellSize
                    );
                }
            }
        }

        if (spellCardPrefab != null)
        {
            foreach (SpellData spell in spells)
            {
                if (spell != null)
                {
                    CreateSpellCard(
                        spell,
                        cellSize
                    );
                }
            }
        }

        UpdateCollectionHeight();
    }

    // CREATE CARD WRAPPER

    private RectTransform CreateCardParent(
        string objectName,
        Vector2 cellSize)
    {
        if (!useCardWrappers)
        {
            return collectionContent
                as RectTransform;
        }

        GameObject wrapperObject =
            new GameObject(
                objectName,
                typeof(RectTransform)
            );

        RectTransform wrapper =
            wrapperObject.GetComponent<RectTransform>();

        wrapper.SetParent(
            collectionContent,
            false
        );

        wrapper.anchorMin =
            new Vector2(0.5f, 0.5f);

        wrapper.anchorMax =
            new Vector2(0.5f, 0.5f);

        wrapper.pivot =
            new Vector2(0.5f, 0.5f);

        wrapper.sizeDelta = cellSize;

        wrapper.localScale = Vector3.one;
        wrapper.localRotation = Quaternion.identity;

        generatedObjects.Add(wrapperObject);

        return wrapper;
    }

    // SCALE COMPLETE CARD

    private void FitCardToArea(
        RectTransform cardRect,
        RectTransform prefabRect,
        Vector2 availableSize,
        float padding,
        float multiplier)
    {
        if (cardRect == null || prefabRect == null)
            return;

        Vector2 designSize =
            prefabRect.rect.size;

        if (designSize.x <= 0f ||
            designSize.y <= 0f)
        {
            designSize =
                new Vector2(422f, 478f);
        }

        cardRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        cardRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        cardRect.pivot =
            new Vector2(0.5f, 0.5f);

        cardRect.sizeDelta = designSize;
        cardRect.anchoredPosition = Vector2.zero;

        cardRect.localRotation =
            Quaternion.identity;

        float width = Mathf.Max(
            1f,
            availableSize.x - padding * 2f
        );

        float height = Mathf.Max(
            1f,
            availableSize.y - padding * 2f
        );

        float scale = Mathf.Min(
            width / designSize.x,
            height / designSize.y
        );

        scale *= Mathf.Max(
            0.01f,
            multiplier
        );

        cardRect.localScale =
            new Vector3(scale, scale, 1f);
    }

    // SCALE COLLECTION CARD

    private void ScaleCollectionCard(
        Button card,
        Button prefab,
        Vector2 cellSize)
    {
        if (!useCardWrappers)
            return;

        FitCardToArea(
            card.GetComponent<RectTransform>(),
            prefab.GetComponent<RectTransform>(),
            cellSize,
            cardPadding,
            cardScaleMultiplier
        );
    }

    // CREATE MONSTER COLLECTION CARD

    private void CreateMonsterCard(
        MonsterData monster,
        Vector2 cellSize)
    {
        RectTransform parent =
            CreateCardParent(
                "Monster_Wrapper_" +
                monster.monsterName,
                cellSize
            );

        Button card = Instantiate(
            monsterCardPrefab,
            parent,
            false
        );

        card.gameObject.name =
            "Monster_Card_" +
            monster.monsterName;

        if (!useCardWrappers)
            generatedObjects.Add(card.gameObject);

        ScaleCollectionCard(
            card,
            monsterCardPrefab,
            cellSize
        );

        MonsterCardView view =
            card.GetComponent<MonsterCardView>();

        if (view == null)
        {
            Debug.LogError(
                "Monster prefab needs MonsterCardView!"
            );

            card.gameObject.SetActive(false);
            return;
        }

        view.DisplayMonster(
            monster,
            BuildEffectDescription(monster),
            CountMonsterCopies(
                monster.monsterId
            ) > 0
        );

        MonsterData captured = monster;

        card.onClick.RemoveAllListeners();

        card.onClick.AddListener(
            () => PreviewMonster(captured)
        );

        collectionEntries.Add(
            new CollectionEntry
            {
                monster = monster,
                monsterView = view
            }
        );
    }

    // CREATE SPELL COLLECTION CARD

    private void CreateSpellCard(
        SpellData spell,
        Vector2 cellSize)
    {
        RectTransform parent =
            CreateCardParent(
                "Spell_Wrapper_" +
                spell.spellName,
                cellSize
            );

        Button card = Instantiate(
            spellCardPrefab,
            parent,
            false
        );

        card.gameObject.name =
            "Spell_Card_" +
            spell.spellName;

        if (!useCardWrappers)
            generatedObjects.Add(card.gameObject);

        ScaleCollectionCard(
            card,
            spellCardPrefab,
            cellSize
        );

        SpellCardView view =
            card.GetComponent<SpellCardView>();

        if (view == null)
        {
            Debug.LogError(
                "Spell prefab needs SpellCardView!"
            );

            card.gameObject.SetActive(false);
            return;
        }

        view.DisplaySpell(spell);

        SpellData captured = spell;

        card.onClick.RemoveAllListeners();

        card.onClick.AddListener(
            () => PreviewSpell(captured)
        );

        collectionEntries.Add(
            new CollectionEntry
            {
                spell = spell,
                spellView = view
            }
        );
    }

    // COLLECTION HEIGHT

    private void UpdateCollectionHeight()
    {
        if (collectionContent == null)
            return;

        GridLayoutGroup grid =
            collectionContent.GetComponent<GridLayoutGroup>();

        RectTransform rect =
            collectionContent as RectTransform;

        if (grid == null || rect == null)
            return;

        int columns;

        if (grid.constraint ==
            GridLayoutGroup.Constraint.FixedColumnCount)
        {
            columns = Mathf.Max(
                1,
                grid.constraintCount
            );
        }
        else
        {
            float width =
                rect.rect.width -
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
            collectionEntries.Count /
            (float)columns
        );

        float height =
            grid.padding.top +
            grid.padding.bottom +
            rows * grid.cellSize.y +
            Mathf.Max(0, rows - 1) *
            grid.spacing.y;

        rect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            height
        );

        if (collectionSectionLayout != null)
        {
            float titleHeight =
                collectionTitle != null
                    ? collectionTitle.rect.height
                    : 50f;

            float totalHeight =
                collectionTopPadding +
                titleHeight +
                height +
                collectionBottomPadding;

            collectionSectionLayout.preferredHeight =
                Mathf.Max(
                    minimumCollectionHeight,
                    totalHeight
                );
        }

        Canvas.ForceUpdateCanvases();
    }

    // MONSTER EFFECT DESCRIPTIONS

    private string BuildEffectDescription(
        MonsterData monster)
    {
        if (monster == null)
            return "";

        List<string> descriptions =
            new List<string>();

        if (!string.IsNullOrWhiteSpace(
            monster.moveOne))
        {
            descriptions.Add(
                GetEffectDescription(
                    monster.moveOne
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(
            monster.moveTwo))
        {
            descriptions.Add(
                GetEffectDescription(
                    monster.moveTwo
                )
            );
        }

        return string.Join(
            "\n\n",
            descriptions
        );
    }

    private string GetEffectDescription(
        string effectName)
    {
        if (string.IsNullOrWhiteSpace(effectName))
            return "";

        foreach (CardEffectDescription effect
                 in effectDescriptions)
        {
            if (effect == null)
                continue;

            if (string.Equals(
                effect.effectName?.Trim(),
                effectName.Trim(),
                StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(
                    effect.description))
                {
                    return effectName + ":\n" +
                           effect.description.Trim();
                }
            }
        }

        return effectName;
    }

    // MONSTER PREVIEW

    public void PreviewMonster(
        MonsterData monster)
    {
        if (monster == null)
            return;

        selectedDeckSlot = -1;

        OpenMonsterPreview(monster);
    }

    private void OpenMonsterPreview(
        MonsterData monster)
    {
        selectedMonster = monster;
        selectedSpell = null;

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(true);

        if (previewSpellCardView != null)
        {
            previewSpellCardView.gameObject
                .SetActive(false);
        }

        if (previewCardView != null)
        {
            previewCardView.gameObject
                .SetActive(true);

            previewCardView.DisplayMonster(
                monster,
                BuildEffectDescription(monster),
                CountMonsterCopies(
                    monster.monsterId
                ) > 0
            );
        }

        if (cardManagementPanel != null)
            cardManagementPanel.SetActive(true);

        RefreshCardManagement();
    }

    // SPELL PREVIEW

    public void PreviewSpell(
        SpellData spell)
    {
        if (spell == null)
            return;

        selectedDeckSlot = -1;

        OpenSpellPreview(spell);
    }

    private void OpenSpellPreview(
        SpellData spell)
    {
        selectedSpell = spell;
        selectedMonster = null;

        if (monsterDetailsPanel != null)
            monsterDetailsPanel.SetActive(true);

        if (previewCardView != null)
        {
            previewCardView.gameObject
                .SetActive(false);
        }

        if (previewSpellCardView != null)
        {
            previewSpellCardView.gameObject
                .SetActive(true);

            previewSpellCardView.DisplaySpell(
                spell
            );
        }

        if (cardManagementPanel != null)
            cardManagementPanel.SetActive(true);

        RefreshCardManagement();
    }

    // CLICK DECK SLOT

    private void OnDeckSlotClicked(int index)
    {
        if (index < 0 ||
            index >= currentDeck.monsterIds.Count)
        {
            return;
        }

        string entry =
            currentDeck.monsterIds[index];

        if (IsSpellEntry(entry))
        {
            SpellData spell =
                FindSpell(
                    GetSpellId(entry)
                );

            if (spell == null)
                return;

            selectedDeckSlot = index;

            OpenSpellPreview(spell);
        }
        else
        {
            MonsterData monster =
                FindMonster(
                    GetMonsterId(entry)
                );

            if (monster == null)
                return;

            selectedDeckSlot = index;

            OpenMonsterPreview(monster);
        }
    }

    // REFRESH CARD MANAGEMENT

    private void RefreshCardManagement()
    {
        int copies = 0;
        int maximum = 0;

        if (selectedMonster != null)
        {
            copies = CountMonsterCopies(
                selectedMonster.monsterId
            );

            maximum = MaxMonsterCopies;
        }
        else if (selectedSpell != null)
        {
            copies = CountSpellCopies(
                selectedSpell.spellId
            );

            maximum = GetMaximumCopies(
                selectedSpell
            );
        }

        if (copiesLabel != null)
        {
            copiesLabel.text =
                "Copies in Deck";
        }

        if (copiesCount != null)
        {
            copiesCount.text =
                copies + " / " + maximum;
        }

        bool hasSelection =
            selectedMonster != null ||
            selectedSpell != null;

        if (addCopyButton != null)
        {
            addCopyButton.interactable =
                hasSelection &&
                copies < maximum &&
                currentDeck.monsterIds.Count <
                MaxDeckSize;
        }

        if (removeCopyButton != null)
        {
            removeCopyButton.interactable =
                hasSelection &&
                copies > 0;
        }
    }

    // ADD SELECTED CARD

    public void AddSelectedCard()
    {
        if (selectedMonster == null &&
            selectedSpell == null)
        {
            ShowFeedback(
                "Select a card first."
            );

            return;
        }

        if (currentDeck.monsterIds.Count >=
            MaxDeckSize)
        {
            ShowFeedback(
                "Your deck is full!"
            );

            return;
        }

        string key;
        string name;

        int copies;
        int maximum;

        if (selectedMonster != null)
        {
            if (string.IsNullOrWhiteSpace(
                selectedMonster.monsterId))
            {
                return;
            }

            key = MonsterKey(
                selectedMonster.monsterId
            );

            name = selectedMonster.monsterName;

            copies = CountMonsterCopies(
                selectedMonster.monsterId
            );

            maximum = MaxMonsterCopies;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(
                selectedSpell.spellId))
            {
                return;
            }

            key = SpellKey(
                selectedSpell.spellId
            );

            name = selectedSpell.spellName;

            copies = CountSpellCopies(
                selectedSpell.spellId
            );

            maximum = GetMaximumCopies(
                selectedSpell
            );
        }

        if (copies >= maximum)
        {
            ShowFeedback(
                "Maximum copies already reached."
            );

            return;
        }

        currentDeck.monsterIds.Add(key);

        ShowFeedback(
            "Added " + name +
            " (" + (copies + 1) +
            "/" + maximum + ")"
        );

        RefreshAll();
    }

    // REMOVE SELECTED COPY

    public void RemoveSelectedCopy()
    {
        if (selectedMonster == null &&
            selectedSpell == null)
        {
            return;
        }

        int removeIndex = -1;

        string key =
            selectedMonster != null
                ? MonsterKey(
                    selectedMonster.monsterId
                  )
                : SpellKey(
                    selectedSpell.spellId
                  );

        if (selectedDeckSlot >= 0 &&
            selectedDeckSlot <
            currentDeck.monsterIds.Count)
        {
            if (EntryMatchesKey(
                currentDeck.monsterIds[
                    selectedDeckSlot
                ],
                key))
            {
                removeIndex =
                    selectedDeckSlot;
            }
        }

        if (removeIndex < 0)
        {
            for (int i =
                currentDeck.monsterIds.Count - 1;
                i >= 0;
                i--)
            {
                if (EntryMatchesKey(
                    currentDeck.monsterIds[i],
                    key))
                {
                    removeIndex = i;
                    break;
                }
            }
        }

        if (removeIndex < 0)
        {
            ShowFeedback(
                "No copies to remove."
            );

            return;
        }

        currentDeck.monsterIds.RemoveAt(
            removeIndex
        );

        selectedDeckSlot = -1;

        ShowFeedback(
            "Removed one copy."
        );

        RefreshAll();
    }

    private bool EntryMatchesKey(
        string entry,
        string key)
    {
        if (IsSpellEntry(entry))
        {
            return string.Equals(
                entry,
                key,
                StringComparison.OrdinalIgnoreCase
            );
        }

        return string.Equals(
            MonsterKey(
                GetMonsterId(entry)
            ),
            key,
            StringComparison.Ordinal
        );
    }

    // CLOSE PREVIEW

    public void CloseMonsterDetails()
    {
        selectedMonster = null;
        selectedSpell = null;
        selectedDeckSlot = -1;

        if (monsterDetailsPanel != null)
        {
            monsterDetailsPanel.SetActive(false);
        }
    }

    // REFRESH EVERYTHING

    private void RefreshAll()
    {
        RefreshDeckDisplay();
        RefreshCollectionDisplay();
        RefreshCardManagement();
    }

    private void RefreshCollectionDisplay()
    {
        foreach (CollectionEntry entry
                 in collectionEntries)
        {
            if (entry == null)
                continue;

            if (entry.monster != null &&
                entry.monsterView != null)
            {
                entry.monsterView.DisplayMonster(
                    entry.monster,
                    BuildEffectDescription(
                        entry.monster
                    ),
                    CountMonsterCopies(
                        entry.monster.monsterId
                    ) > 0
                );
            }

            if (entry.spell != null &&
                entry.spellView != null)
            {
                entry.spellView.DisplaySpell(
                    entry.spell
                );
            }
        }
    }

    // FULL MINIATURE CARDS IN DECK SLOTS

    private void RefreshDeckDisplay()
    {
        int count =
            currentDeck.monsterIds.Count;

        if (deckCountText != null)
        {
            deckCountText.text =
                "Cards In Deck: " +
                count + "/34";
        }

        if (deckSlotButtons == null)
            return;

        for (int i = 0;
             i < deckSlotButtons.Length;
             i++)
        {
            Button slot =
                deckSlotButtons[i];

            if (slot == null)
                continue;

            bool occupied = i < count;

            string entry = occupied
                ? currentDeck.monsterIds[i]
                : "";

            RestoreSlotAppearance(i);

            Transform oldArtwork =
                slot.transform.Find(
                    "Card_Artwork"
                );

            if (oldArtwork != null)
            {
                oldArtwork.gameObject
                    .SetActive(false);
            }

            TMP_Text label =
                GetSlotLabel(slot);

            if (label != null)
            {
                label.text =
                    (i + 1).ToString();

                label.enabled =
                    !occupied ||
                    !showCardArtworkInDeckSlots;
            }

            if (!occupied ||
                !showCardArtworkInDeckSlots)
            {
                RemoveMiniCard(i);
                continue;
            }

            if (miniCardObjects[i] != null &&
                miniCardKeys[i] == entry)
            {
                FitExistingMiniCard(
                    i,
                    entry
                );

                continue;
            }

            RemoveMiniCard(i);

            CreateMiniCard(
                i,
                entry
            );
        }
    }

    // RESTORE ORIGINAL SLOT APPEARANCE

    private void RestoreSlotAppearance(
        int index)
    {
        if (deckSlotButtons == null ||
            index < 0 ||
            index >= deckSlotButtons.Length)
        {
            return;
        }

        Button button =
            deckSlotButtons[index];

        if (button == null)
            return;

        button.transition =
            originalTransitions[index];

        if (button.image != null)
        {
            button.image.sprite =
                originalSlotSprites[index];

            button.image.color =
                originalSlotColors[index];

            button.image.type =
                originalSlotTypes[index];

            button.image.preserveAspect =
                originalPreserveAspect[index];
        }
    }

    // CREATE MINIATURE CARD

    private void CreateMiniCard(
        int slotIndex,
        string entry)
    {
        Button slot =
            deckSlotButtons[slotIndex];

        if (slot == null)
            return;

        Button prefab = null;

        MonsterData monster = null;
        SpellData spell = null;

        if (IsSpellEntry(entry))
        {
            spell = FindSpell(
                GetSpellId(entry)
            );

            prefab = spellCardPrefab;
        }
        else
        {
            monster = FindMonster(
                GetMonsterId(entry)
            );

            prefab = monsterCardPrefab;
        }

        if (prefab == null)
        {
            Debug.LogWarning(
                "Missing prefab for slot " +
                (slotIndex + 1)
            );

            return;
        }

        if (monster == null &&
            spell == null)
        {
            Debug.LogWarning(
                "Card data missing for slot " +
                (slotIndex + 1)
            );

            return;
        }

        GameObject wrapperObject =
            new GameObject(
                "Mini_Card",
                typeof(RectTransform)
            );

        RectTransform wrapper =
            wrapperObject.GetComponent<RectTransform>();

        wrapper.SetParent(
            slot.transform,
            false
        );

        wrapper.anchorMin = Vector2.zero;
        wrapper.anchorMax = Vector2.one;

        wrapper.offsetMin = Vector2.zero;
        wrapper.offsetMax = Vector2.zero;

        wrapper.localScale = Vector3.one;

        wrapper.localRotation =
            Quaternion.identity;

        Button miniature = Instantiate(
            prefab,
            wrapper,
            false
        );

        miniature.gameObject.name =
            IsSpellEntry(entry)
                ? "Mini_Spell"
                : "Mini_Monster";

        FitCardToArea(
            miniature.GetComponent<RectTransform>(),
            prefab.GetComponent<RectTransform>(),
            slot.GetComponent<RectTransform>()
                .rect.size,
            miniCardPadding,
            miniCardScaleMultiplier
        );

        if (monster != null)
        {
            MonsterCardView view =
                miniature.GetComponent<MonsterCardView>();

            if (view != null)
            {
                view.DisplayMonster(
                    monster,
                    BuildEffectDescription(
                        monster
                    ),
                    true
                );
            }
        }
        else if (spell != null)
        {
            SpellCardView view =
                miniature.GetComponent<SpellCardView>();

            if (view != null)
            {
                view.DisplaySpell(spell);
            }
        }

        DisableMiniCardInteractions(
            miniature.gameObject
        );

        miniCardObjects[slotIndex] =
            wrapperObject;

        miniCardKeys[slotIndex] =
            entry;
    }

    // RESIZE EXISTING MINIATURE

    private void FitExistingMiniCard(
        int slotIndex,
        string entry)
    {
        GameObject wrapperObject =
            miniCardObjects[slotIndex];

        if (wrapperObject == null ||
            wrapperObject.transform.childCount == 0)
        {
            return;
        }

        Button prefab =
            IsSpellEntry(entry)
                ? spellCardPrefab
                : monsterCardPrefab;

        if (prefab == null)
            return;

        RectTransform cardRect =
            wrapperObject.transform
                .GetChild(0)
                .GetComponent<RectTransform>();

        RectTransform slotRect =
            deckSlotButtons[slotIndex]
                .GetComponent<RectTransform>();

        if (cardRect == null ||
            slotRect == null)
        {
            return;
        }

        FitCardToArea(
            cardRect,
            prefab.GetComponent<RectTransform>(),
            slotRect.rect.size,
            miniCardPadding,
            miniCardScaleMultiplier
        );
    }

    // DISABLE MINI CARD INTERACTIONS

    private void DisableMiniCardInteractions(
        GameObject cardObject)
    {
        Button[] buttons =
            cardObject.GetComponentsInChildren<Button>(
                true
            );

        foreach (Button button in buttons)
        {
            button.transition =
                Selectable.Transition.None;

            button.enabled = false;
        }

        Graphic[] graphics =
            cardObject.GetComponentsInChildren<Graphic>(
                true
            );

        foreach (Graphic graphic in graphics)
        {
            graphic.raycastTarget = false;
        }

        CanvasGroup[] groups =
            cardObject.GetComponentsInChildren<CanvasGroup>(
                true
            );

        foreach (CanvasGroup group in groups)
        {
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        Image rootImage =
            cardObject.GetComponent<Image>();

        if (rootImage != null)
        {
            rootImage.color = Color.white;
        }
    }

    // REMOVE MINI CARD

    private void RemoveMiniCard(
        int slotIndex)
    {
        if (miniCardObjects == null ||
            slotIndex < 0 ||
            slotIndex >= miniCardObjects.Length)
        {
            return;
        }

        GameObject oldCard =
            miniCardObjects[slotIndex];

        if (oldCard != null)
        {
            oldCard.SetActive(false);
            Destroy(oldCard);
        }

        miniCardObjects[slotIndex] = null;
        miniCardKeys[slotIndex] = null;
    }

    // GET SLOT NUMBER LABEL

    private TMP_Text GetSlotLabel(
        Button button)
    {
        if (button == null)
            return null;

        for (int i = 0;
             i < button.transform.childCount;
             i++)
        {
            Transform child =
                button.transform.GetChild(i);

            if (child.name == "Mini_Card" ||
                child.name == "Card_Artwork")
            {
                continue;
            }

            TMP_Text label =
                child.GetComponent<TMP_Text>();

            if (label != null)
                return label;
        }

        return null;
    }

    // CLEAR CURRENT DECK

    public void ClearDeck()
    {
        currentDeck.monsterIds.Clear();

        CloseMonsterDetails();
        RefreshAll();

        ShowFeedback(
            "Current deck cleared."
        );
    }

    // RANDOM DECK

    public void RandomiseDeck()
    {
        List<string> pool =
            new List<string>();

        HashSet<string> monsterIds =
            new HashSet<string>();

        foreach (MonsterData monster in monsters)
        {
            if (monster == null ||
                string.IsNullOrWhiteSpace(
                    monster.monsterId))
            {
                continue;
            }

            if (!monsterIds.Add(
                monster.monsterId))
            {
                continue;
            }

            for (int i = 0;
                 i < MaxMonsterCopies;
                 i++)
            {
                pool.Add(
                    MonsterKey(
                        monster.monsterId
                    )
                );
            }
        }

        HashSet<string> spellIds =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

        foreach (SpellData spell in spells)
        {
            if (spell == null ||
                string.IsNullOrWhiteSpace(
                    spell.spellId))
            {
                continue;
            }

            if (!spellIds.Add(
                spell.spellId))
            {
                continue;
            }

            int maximum =
                GetMaximumCopies(spell);

            for (int i = 0;
                 i < maximum;
                 i++)
            {
                pool.Add(
                    SpellKey(
                        spell.spellId
                    )
                );
            }
        }

        for (int i = pool.Count - 1;
             i > 0;
             i--)
        {
            int j =
                UnityEngine.Random.Range(
                    0,
                    i + 1
                );

            string temp = pool[i];

            pool[i] = pool[j];
            pool[j] = temp;
        }

        currentDeck.monsterIds.Clear();

        int amount = Mathf.Min(
            MaxDeckSize,
            pool.Count
        );

        for (int i = 0;
             i < amount;
             i++)
        {
            currentDeck.monsterIds.Add(
                pool[i]
            );
        }

        CloseMonsterDetails();
        RefreshAll();

        ShowFeedback(
            "Random deck created with " +
            amount + " cards."
        );
    }

    // SAVE CURRENT DECK

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
            ShowFeedback(
                "Enter a deck name."
            );

            return;
        }

        if (currentDeck.monsterIds.Count >
            MaxDeckSize)
        {
            ShowFeedback(
                "Deck exceeds 34 cards."
            );

            return;
        }

        Dictionary<string, int> counts =
            new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase
            );

        foreach (string entry
                 in currentDeck.monsterIds)
        {
            string key;
            int maximum;

            if (IsSpellEntry(entry))
            {
                SpellData spell =
                    FindSpell(
                        GetSpellId(entry)
                    );

                if (spell == null)
                {
                    ShowFeedback(
                        "Deck contains an unavailable spell."
                    );

                    return;
                }

                key = SpellKey(
                    spell.spellId
                );

                maximum =
                    GetMaximumCopies(spell);
            }
            else
            {
                MonsterData monster =
                    FindMonster(
                        GetMonsterId(entry)
                    );

                if (monster == null)
                {
                    ShowFeedback(
                        "Deck contains an unavailable monster."
                    );

                    return;
                }

                key = MonsterKey(
                    monster.monsterId
                );

                maximum =
                    MaxMonsterCopies;
            }

            int amount = 0;

            counts.TryGetValue(
                key,
                out amount
            );

            amount++;

            if (amount > maximum)
            {
                ShowFeedback(
                    "Too many copies of a card."
                );

                return;
            }

            counts[key] = amount;
        }

        // NEW: FOUR-DECK SAVING SUPPORT

        if (deckSlotSwitcher == null)
        {
            deckSlotSwitcher =
                GetComponent<DeckSlotSwitcher>();
        }

        bool saved;

        if (deckSlotSwitcher != null)
        {
           
            saved =
                deckSlotSwitcher.SaveActiveDeck();
        }
        else
        {
           
            saved =
                DeckSaveSystem.SaveDeck(
                    currentDeck
                );
        }

        ShowFeedback(
            saved
                ? "Deck saved: " +
                  currentDeck.deckName
                : "Deck saving failed. Check Console."
        );
    }

    // NEW: EXPORT CURRENT DECK SNAPSHOT

    public DeckData ExportDeckSnapshot()
    {
        DeckData copy =
            new DeckData();

        copy.deckId =
            currentDeck.deckId;

        copy.deckName =
            deckNameInput != null
                ? deckNameInput.text.Trim()
                : currentDeck.deckName;

        copy.monsterIds =
            currentDeck.monsterIds != null
                ? new List<string>(
                    currentDeck.monsterIds
                  )
                : new List<string>();

        return copy;
    }

    // NEW: IMPORT ANOTHER DECK SLOT

    public void ImportDeckSnapshot(
        DeckData deck)
    {
        if (deck == null)
            return;

        CloseMonsterDetails();

        currentDeck =
            new DeckData();

        currentDeck.deckId =
            deck.deckId;

        currentDeck.deckName =
            deck.deckName;

        currentDeck.monsterIds =
            deck.monsterIds != null
                ? new List<string>(
                    deck.monsterIds
                  )
                : new List<string>();

        
        ValidateLoadedDeck();

        if (deckNameInput != null)
        {
            deckNameInput.SetTextWithoutNotify(
                currentDeck.deckName
            );
        }

        
        RefreshAll();

        Debug.Log(
            "Loaded deck: " +
            currentDeck.deckName
        );
    }

    // OPTIONAL FEEDBACK

    private void ShowFeedback(
        string message)
    {
        Debug.Log(message);

        if (feedbackText != null)
        {
            feedbackText.text =
                message;
        }
    }

   
    // RELOAD COLLECTION

    public void ReloadCollection()
    {
        CloseMonsterDetails();

        LoadCollection();
        LoadSpells();

       
        if (deckSlotSwitcher == null)
        {
            LoadCurrentDeck();
        }
        else
        {
            ValidateLoadedDeck();
        }

        BuildCollectionCards();

        if (miniCardObjects != null)
        {
            for (int i = 0;
                 i < miniCardObjects.Length;
                 i++)
            {
                RemoveMiniCard(i);
            }
        }

        RefreshAll();
    }
}
