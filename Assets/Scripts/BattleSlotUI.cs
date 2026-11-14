
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleSlotUI : MonoBehaviour
{
    private Button slotButton;
    private Image slotImage;
    private TMP_Text slotLabel;

    private BattleManager manager;
    private bool isPlayerSlot;
    private int slotIndex;

    private GameObject monsterPrefab;
    private GameObject currentVisual;

    private BattleMonster displayedMonster;

    private Color originalColor;
    private bool initialized;

    // ASSIGN PREFAB

    public void ConfigurePrefab(GameObject prefab)
    {
        monsterPrefab = prefab;
    }

    // INITIALISE SLOT

    public void Initialize(
        BattleManager battleManager,
        bool belongsToPlayer,
        int index)
    {
        manager = battleManager;
        isPlayerSlot = belongsToPlayer;
        slotIndex = index;

        slotButton = GetComponent<Button>();
        slotImage = GetComponent<Image>();

        if (slotButton == null)
        {
            Debug.LogError(
                "BattleSlotUI requires a Button!",
                this
            );
            return;
        }

        if (slotLabel == null)
            slotLabel = GetComponentInChildren<TMP_Text>(true);

        if (!initialized)
        {
            originalColor = slotImage != null
                ? slotImage.color
                : Color.white;

            initialized = true;
        }

        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(OnClicked);
    }

    // DISPLAY CURRENT MONSTER

    public void Refresh(BattleMonster monster)
    {
        if (slotButton == null)
            return;

        if (monster == null)
        {
            ClearVisual();

            displayedMonster = null;

            if (slotLabel != null)
                slotLabel.text = "";

            if (slotImage != null)
                slotImage.color = originalColor;

            return;
        }

        if (displayedMonster != monster ||
            currentVisual == null)
        {
            ClearVisual();

            displayedMonster = monster;
            CreateMonsterVisual(monster);
        }

        if (currentVisual != null)
        {
            MonsterCardView view =
                currentVisual.GetComponent<MonsterCardView>();

            if (view != null)
                view.SetBattleHealth(monster.currentHP);
        }

        if (slotLabel != null)
            slotLabel.text = "";

        if (slotImage != null)
        {
           
            slotImage.color = new Color(1, 1, 1, 0);
        }
    }

    // CREATE MONSTER CARD

    private void CreateMonsterVisual(BattleMonster monster)
    {
        if (monsterPrefab == null)
        {
            Debug.LogWarning(
                "BattleSlotUI: No monster card prefab assigned.",
                this
            );

            if (slotLabel != null)
            {
                slotLabel.text =
                    monster.data.monsterName +
                    "\nHP: " + monster.currentHP;
            }

            return;
        }

        currentVisual = Instantiate(
            monsterPrefab,
            transform
        );

        currentVisual.name =
            "Battle_Card_" + monster.data.monsterName;

        MonsterCardView view =
            currentVisual.GetComponent<MonsterCardView>();

        if (view != null)
        {
            view.DisplayMonster(
                monster.data,
                "",
                false
            );

            view.SetCompact(true);
            view.SetBattleHealth(monster.currentHP);
        }

        FitToSlot(currentVisual);

        foreach (Graphic graphic in
            currentVisual.GetComponentsInChildren<Graphic>(true))
        {
            graphic.raycastTarget = false;
        }

        foreach (Button button in
            currentVisual.GetComponentsInChildren<Button>(true))
        {
            button.interactable = false;
        }
    }

    // FIT CARD TO THE EXISTING SLOT

    private void FitToSlot(GameObject visual)
    {
        RectTransform slotRect =
            GetComponent<RectTransform>();

        RectTransform cardRect =
            visual.GetComponent<RectTransform>();

        if (slotRect == null || cardRect == null)
            return;

        float width = cardRect.rect.width;
        float height = cardRect.rect.height;

        if (width <= 1f) width = 422f;
        if (height <= 1f) height = 478f;

        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);

        cardRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, width);

        cardRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical, height);

        cardRect.anchoredPosition = Vector2.zero;
        cardRect.localRotation = Quaternion.identity;

        float scale = Mathf.Min(
            slotRect.rect.width / width,
            slotRect.rect.height / height
        );

        cardRect.localScale =
            new Vector3(scale, scale, 1f);
    }

    // REMOVE CARD WHEN DEFEATED

    private void ClearVisual()
    {
        if (currentVisual != null)
        {
            currentVisual.SetActive(false);
            Destroy(currentVisual);
            currentVisual = null;
        }
    }

    // CLICK SLOT

    private void OnClicked()
    {
        if (manager != null)
        {
            manager.SelectMonsterSlot(
                isPlayerSlot,
                slotIndex
            );
        }
    }
}
