
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleHandUI : MonoBehaviour
{
    [Header("Hand Container")]
    [SerializeField] private RectTransform handContent;

    [Header("Card Prefabs")]
    [SerializeField] private GameObject monsterCardPrefab;
    [SerializeField] private GameObject spellCardPrefab;

    [Header("Full Card Preview")]
    [SerializeField] private BattleCardPreview cardPreview;

    [Header("Normal Card Size")]
    [SerializeField]
    private Vector2 cardSize =
        new Vector2(120f, 160f);

    [Header("Automatic Hand Scaling")]
    [SerializeField] private float cardSpacing = 5f;

    [SerializeField] private float handSidePadding = 5f;

    [Tooltip("Only used if the hand has no valid width.")]
    [SerializeField] private float fallbackHandWidth = 650f;

    [Header("Card Background Colours")]
    [SerializeField]
    private Color monsterBackground =
        new Color(0.22f, 0.13f, 0.07f, 1f);

    [SerializeField]
    private Color spellBackground =
        new Color(0.04f, 0.16f, 0.08f, 1f);

    [Header("Selected Card Appearance")]
    [SerializeField]
    private Color selectedOutlineColor =
        new Color(1f, 0.82f, 0.16f, 1f);

    [SerializeField]
    private Vector2 outlineThickness =
        new Vector2(3f, 3f);

    [SerializeField]
    private Color selectedBackground =
        new Color(0.55f, 0.38f, 0.08f, 1f);

    [Header("Custom Info Button")]
    [SerializeField] private GameObject infoButtonPrefab;

    [SerializeField]
    private Vector2 infoButtonSize =
        new Vector2(75f, 30f);

    [SerializeField] private float infoButtonOffset = 5f;

    private readonly List<GameObject> spawnedCards =
        new List<GameObject>();

    // DISPLAY PLAYER HAND

    public void ShowHand(
        BattlePlayer player,
        BattleManager manager)
    {
        ClearHand();

        if (player == null ||
            manager == null ||
            handContent == null)
            return;

        Canvas.ForceUpdateCanvases();

        int count = player.hand.Count;

        if (count == 0)
            return;

        Vector2 fittedSize = CalculateCardSize(count);

        int selectedIndex = manager.SelectedHandIndex;

        for (int i = 0; i < count; i++)
        {
            BattleCard card = player.hand[i];

            if (card == null)
                continue;

            CreateHandCard(
                card,
                i,
                manager,
                i == selectedIndex,
                fittedSize
            );
        }
    }

    // CALCULATE SIZE BASED ON AVAILABLE SPACE

    private Vector2 CalculateCardSize(int cardCount)
    {
        if (cardCount <= 0)
            return cardSize;

        float availableWidth = handContent.rect.width;

        if (availableWidth <= 1f)
        {
            availableWidth = fallbackHandWidth;

            Debug.LogWarning(
                "BattleHandUI: Hand Content width is " +
                "not valid. Using fallback width.",
                this
            );
        }

        float spacing = Mathf.Max(0f, cardSpacing);
        float padding = Mathf.Max(0f, handSidePadding);

        // Account for any Horizontal Layout Group.
        HorizontalLayoutGroup group =
            handContent.GetComponent<HorizontalLayoutGroup>();

        if (group != null)
        {
            spacing = group.spacing;

            padding += (
                group.padding.left +
                group.padding.right
            ) * 0.5f;
        }

        float usableWidth =
            availableWidth -
            padding * 2f -
            spacing * Mathf.Max(0, cardCount - 1);

        float fittedWidth =
            usableWidth / cardCount;

        // Never enlarge cards beyond normal size.
        fittedWidth = Mathf.Min(
            cardSize.x,
            Mathf.Max(1f, fittedWidth)
        );

        // Maintain the original card aspect ratio.
        float aspectRatio =
            cardSize.y / Mathf.Max(1f, cardSize.x);

        float fittedHeight =
            fittedWidth * aspectRatio;

        // Also fit the available hand height.
        float availableHeight = handContent.rect.height;

        if (availableHeight > 1f &&
            fittedHeight > availableHeight)
        {
            fittedHeight = availableHeight;
            fittedWidth = fittedHeight / aspectRatio;
        }

        return new Vector2(
            fittedWidth,
            fittedHeight
        );
    }

    // CREATE ONE HAND CARD

    private void CreateHandCard(
        BattleCard card,
        int cardIndex,
        BattleManager manager,
        bool isSelected,
        Vector2 fittedSize)
    {
        GameObject holder = new GameObject(
            "Hand_" + card.Name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(LayoutElement),
            typeof(Outline)
        );

        holder.transform.SetParent(
            handContent,
            false
        );

        RectTransform holderRect =
            holder.GetComponent<RectTransform>();

        holderRect.sizeDelta = fittedSize;

        LayoutElement layout =
            holder.GetComponent<LayoutElement>();

        layout.minWidth = 0f;
        layout.minHeight = 0f;

        layout.preferredWidth = fittedSize.x;
        layout.preferredHeight = fittedSize.y;

        layout.flexibleWidth = 0f;
        layout.flexibleHeight = 0f;

        // OPAQUE BACKGROUND

        Image backing =
            holder.GetComponent<Image>();

        backing.sprite = null;
        backing.type = Image.Type.Simple;

        backing.color = isSelected
            ? selectedBackground
            : card.type == BattleCardType.Monster
                ? monsterBackground
                : spellBackground;

        backing.raycastTarget = true;

        // SELECTED OUTLINE

        Outline outline =
            holder.GetComponent<Outline>();

        outline.effectColor = selectedOutlineColor;
        outline.effectDistance = outlineThickness;
        outline.useGraphicAlpha = false;
        outline.enabled = isSelected;

        // CARD SELECTION BUTTON

        Button holderButton =
            holder.GetComponent<Button>();

        holderButton.transition =
            Selectable.Transition.None;

        holderButton.onClick.AddListener(() =>
        {
            manager.SelectHandCard(cardIndex);
        });

        // ACTUAL CARD PREFAB

        GameObject prefab =
            card.type == BattleCardType.Monster
                ? monsterCardPrefab
                : spellCardPrefab;

        if (prefab != null)
        {
            GameObject visual = Instantiate(
                prefab,
                holder.transform,
                false
            );

            visual.name = "Card_Visual";

            ConfigureVisual(visual, card);
            FitCard(visual, fittedSize);
        }
        else
        {
            CreateFallbackText(holder, card.Name);
        }

        // ONLY SELECTED CARD HAS INFO BUTTON

        if (isSelected)
        {
            CreateInfoButton(
                holder,
                card,
                fittedSize
            );
        }

        spawnedCards.Add(holder);
    }

    // CREATE INFO PREFAB ABOVE SELECTED CARD

    private void CreateInfoButton(
        GameObject holder,
        BattleCard card,
        Vector2 fittedSize)
    {
        if (infoButtonPrefab == null)
            return;

        GameObject infoObject = Instantiate(
            infoButtonPrefab,
            holder.transform,
            false
        );

        infoObject.name = "Info_Button";
        infoObject.SetActive(true);

        RectTransform rect =
            infoObject.GetComponent<RectTransform>();

        Button button =
            infoObject.GetComponent<Button>();

        if (rect == null || button == null)
        {
            Debug.LogError(
                "BattleHandUI: Info prefab needs " +
                "a RectTransform and Button.",
                this
            );

            Destroy(infoObject);
            return;
        }

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        // Keep Info small enough for the selected card.
        float buttonWidth = Mathf.Min(
            infoButtonSize.x,
            fittedSize.x
        );

        rect.sizeDelta = new Vector2(
            buttonWidth,
            infoButtonSize.y
        );

        rect.anchoredPosition = new Vector2(
            0f,
            fittedSize.y * 0.5f +
            infoButtonSize.y * 0.5f +
            infoButtonOffset
        );

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            if (cardPreview != null)
                cardPreview.ShowHandCard(card);
        });

        infoObject.transform.SetAsLastSibling();
    }

    // POPULATE MONSTER OR SPELL

    private void ConfigureVisual(
        GameObject visual,
        BattleCard card)
    {
        if (card.type == BattleCardType.Monster)
        {
            MonsterCardView view =
                visual.GetComponent<MonsterCardView>();

            if (view != null && card.monster != null)
            {
                view.DisplayMonster(
                    card.monster,
                    "",
                    false
                );

                view.SetCompact(true);
            }
        }
        else
        {
            SpellCardView view =
                visual.GetComponent<SpellCardView>();

            if (view != null && card.spell != null)
                view.DisplaySpell(card.spell);
        }

        // Let holder handle the card click.
        foreach (Button childButton in
            visual.GetComponentsInChildren<Button>(true))
        {
            childButton.transition =
                Selectable.Transition.None;

            childButton.enabled = false;
        }

        foreach (Graphic graphic in
            visual.GetComponentsInChildren<Graphic>(true))
        {
            graphic.raycastTarget = false;
        }
    }

    // SCALE CARD PREFAB UNIFORMLY

    private void FitCard(
        GameObject visual,
        Vector2 fittedSize)
    {
        RectTransform rect =
            visual.GetComponent<RectTransform>();

        if (rect == null)
            return;

        float width = rect.rect.width;
        float height = rect.rect.height;

        if (width <= 1f)
            width = 422f;

        if (height <= 1f)
            height = 478f;

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            width
        );

        rect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            height
        );

        rect.anchoredPosition = Vector2.zero;
        rect.localRotation = Quaternion.identity;

        float scale = Mathf.Min(
            fittedSize.x / width,
            fittedSize.y / height
        );

        rect.localScale =
            new Vector3(scale, scale, 1f);
    }

    // FALLBACK FOR MISSING PREFAB

    private void CreateFallbackText(
        GameObject holder,
        string cardName)
    {
        GameObject textObject = new GameObject(
            "Fallback_Name",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        textObject.transform.SetParent(
            holder.transform,
            false
        );

        RectTransform rect =
            textObject.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI label =
            textObject.GetComponent<TextMeshProUGUI>();

        label.text = cardName;
        label.fontSize = 16f;
        label.color = Color.white;
        label.alignment =
            TextAlignmentOptions.Center;

        label.raycastTarget = false;
    }

    // REMOVE PREVIOUS CARDS

    private void ClearHand()
    {
        foreach (GameObject card in spawnedCards)
        {
            if (card == null)
                continue;

            card.SetActive(false);
            Destroy(card);
        }

        spawnedCards.Clear();
    }
}
