
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

    [Header("Card Size")]
    [SerializeField]
    private Vector2 cardSize =
        new Vector2(120, 160);

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

        for (int i = 0; i < player.hand.Count; i++)
        {
            int cardIndex = i;
            BattleCard card = player.hand[i];

            GameObject holder = new GameObject(
                "Hand_" + card.Name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement)
            );

            holder.transform.SetParent(
                handContent,
                false
            );

            RectTransform holderRect =
                holder.GetComponent<RectTransform>();

            holderRect.sizeDelta = cardSize;

            LayoutElement layout =
                holder.GetComponent<LayoutElement>();

            layout.preferredWidth = cardSize.x;
            layout.preferredHeight = cardSize.y;

            Image holderImage = holder.GetComponent<Image>();

            
            holderImage.color = new Color(1, 1, 1, 0);

            Button holderButton =
                holder.GetComponent<Button>();

            holderButton.transition =
                Selectable.Transition.None;

            holderButton.onClick.AddListener(
                () => manager.SelectHandCard(cardIndex)
            );

            GameObject prefab =
                card.type == BattleCardType.Monster
                ? monsterCardPrefab
                : spellCardPrefab;

            if (prefab != null)
            {
                GameObject visual =
                    Instantiate(prefab, holder.transform);

                visual.name = "Card_Visual";

                ConfigureVisual(visual, card, cardSize);
            }
            else
            {
                CreateFallbackText(holder, card.Name);
            }

            spawnedCards.Add(holder);
        }
    }

    // SET UP THE REAL CARD PREFAB

    private void ConfigureVisual(
        GameObject visual,
        BattleCard card,
        Vector2 targetSize)
    {
        MonsterCardView monsterView =
            visual.GetComponent<MonsterCardView>();

        SpellCardView spellView =
            visual.GetComponent<SpellCardView>();

        if (card.type == BattleCardType.Monster)
        {
            if (monsterView != null && card.monster != null)
            {
                monsterView.DisplayMonster(
                    card.monster,
                    "",
                    false
                );

                monsterView.SetCompact(true);
            }
            else
            {
                Debug.LogWarning(
                    "BattleHandUI: Monster prefab is " +
                    "missing MonsterCardView."
                );
            }
        }
        else
        {
            if (spellView != null && card.spell != null)
            {
                spellView.DisplaySpell(card.spell);
            }
            else
            {
                Debug.LogWarning(
                    "BattleHandUI: Spell prefab is " +
                    "missing SpellCardView."
                );
            }
        }

        FitCard(visual, targetSize);

      
        foreach (Graphic graphic in
            visual.GetComponentsInChildren<Graphic>(true))
        {
            graphic.raycastTarget = false;
        }

        foreach (Button button in
            visual.GetComponentsInChildren<Button>(true))
        {
            button.interactable = false;
        }
    }

    // FIT THE ORIGINAL CARD WITHOUT DISTORTING IT

    private void FitCard(
        GameObject visual,
        Vector2 targetSize)
    {
        RectTransform rect =
            visual.GetComponent<RectTransform>();

        if (rect == null)
            return;

        float width = rect.rect.width;
        float height = rect.rect.height;

        
        if (width <= 1f) width = 422f;
        if (height <= 1f) height = 478f;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, width);

        rect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical, height);

        rect.anchoredPosition = Vector2.zero;
        rect.localRotation = Quaternion.identity;

        float scale = Mathf.Min(
            targetSize.x / width,
            targetSize.y / height
        );

        rect.localScale = new Vector3(scale, scale, 1f);
    }

    // FALLBACK WHEN A PREFAB IS NOT ASSIGNED

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

        TextMeshProUGUI text =
            textObject.GetComponent<TextMeshProUGUI>();

        text.text = cardName;
        text.color = Color.white;
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
    }

    // REMOVE OLD HAND CARDS

    private void ClearHand()
    {
        foreach (GameObject card in spawnedCards)
        {
            if (card != null)
            {
                card.SetActive(false);
                Destroy(card);
            }
        }

        spawnedCards.Clear();
    }
}
