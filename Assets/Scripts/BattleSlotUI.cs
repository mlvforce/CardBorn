
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleSlotUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private Button slotButton;
    private Image slotImage;
    private TMP_Text slotLabel;

    private BattleManager manager;
    private BattleUIController battleUI;

    private bool isPlayerSlot;
    private int slotIndex;

    private GameObject monsterPrefab;
    private GameObject currentVisual;

    private BattleMonster displayedMonster;

    private Color originalColor;
    private bool initialized;
    private bool draggingAttack;

    // PREFAB REFERENCE

    public void ConfigurePrefab(GameObject prefab)
    {
        monsterPrefab = prefab;
    }

    // UI CONTROLLER REFERENCE

    public void ConfigureInteractions(BattleUIController ui)
    {
        battleUI = ui;
    }

    // INITIALISE BATTLE SLOT

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
                "BattleSlotUI requires a Button component!",
                this
            );
            return;
        }

        if (!initialized)
        {
            slotLabel =
                GetComponentInChildren<TMP_Text>(true);

            originalColor = slotImage != null
                ? slotImage.color
                : Color.white;

            initialized = true;
        }

        // We handle clicks using IPointerClickHandler.
        slotButton.onClick.RemoveAllListeners();
        slotButton.transition =
            Selectable.Transition.None;
    }

    // REFRESH THE BATTLEFIELD CARD

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
            slotImage.color = originalColor;
    }

    // CREATE MONSTER PREFAB

    private void CreateMonsterVisual(BattleMonster monster)
    {
        if (monsterPrefab == null)
        {
            if (slotLabel != null)
                slotLabel.text = monster.data.monsterName;

            return;
        }

        currentVisual = Instantiate(
            monsterPrefab,
            transform,
            false
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

        FitCardToSlot(currentVisual);

        // All clicks should pass through the visual
        // and reach the battlefield slot underneath.
        foreach (Graphic graphic in
            currentVisual.GetComponentsInChildren<Graphic>(true))
        {
            graphic.raycastTarget = false;
        }

        foreach (Button button in
            currentVisual.GetComponentsInChildren<Button>(true))
        {
            button.transition =
                Selectable.Transition.None;

            button.enabled = false;
        }
    }

    // SCALE CARD WITHOUT DISTORTION

    private void FitCardToSlot(GameObject visual)
    {
        RectTransform slotRect =
            GetComponent<RectTransform>();

        RectTransform cardRect =
            visual.GetComponent<RectTransform>();

        if (slotRect == null || cardRect == null)
            return;

        float width = cardRect.rect.width;
        float height = cardRect.rect.height;

        if (width <= 1f)
            width = 422f;

        if (height <= 1f)
            height = 478f;

        cardRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        cardRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        cardRect.pivot =
            new Vector2(0.5f, 0.5f);

        cardRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            width
        );

        cardRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            height
        );

        cardRect.anchoredPosition = Vector2.zero;
        cardRect.localRotation = Quaternion.identity;

        float scale = Mathf.Min(
            slotRect.rect.width / width,
            slotRect.rect.height / height
        );

        cardRect.localScale =
            new Vector3(scale, scale, 1f);
    }

    // REMOVE CARD WHEN MONSTER LEAVES FIELD

    private void ClearVisual()
    {
        if (currentVisual != null)
        {
            currentVisual.SetActive(false);
            Destroy(currentVisual);
            currentVisual = null;
        }
    }

    // HOVER OVER A MONSTER

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (displayedMonster == null ||
            battleUI == null)
            return;

        // NEW: Pass the actual card slot position,
        // so the tooltip can appear beside it.
        battleUI.ShowMonsterTooltip(
            displayedMonster,
            GetComponent<RectTransform>()
        );
    }

    // STOP HOVERING

    public void OnPointerExit(
        PointerEventData eventData)
    {
        if (battleUI != null)
            battleUI.HideMonsterTooltip();
    }

    // CLICK A FIELD SLOT

    public void OnPointerClick(
        PointerEventData eventData)
    {
        if (eventData.button !=
            PointerEventData.InputButton.Left)
            return;

        if (manager == null)
            return;

        // FIRST: Handle selected spell targeting.
        // This works for occupied friendly/enemy slots.
        if (manager.TryCastSelectedSpellOn(
            isPlayerSlot,
            slotIndex))
        {
            return;
        }

        // SECOND: Inspect an existing monster.
        if (displayedMonster != null)
        {
            if (battleUI != null)
            {
                battleUI.OpenMonsterPreview(
                    displayedMonster
                );
            }

            return;
        }

        // THIRD: Summon to an empty friendly slot.
        if (isPlayerSlot)
        {
            manager.SelectMonsterSlot(
                true,
                slotIndex
            );
        }
    }

    // BEGIN DRAGGING A MONSTER ATTACK

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        draggingAttack = false;

        if (!isPlayerSlot ||
            displayedMonster == null ||
            manager == null ||
            battleUI == null)
            return;

        if (eventData.button !=
            PointerEventData.InputButton.Left)
            return;

        if (manager.GameOver ||
            !displayedMonster.CanAttack())
            return;

        // This also cancels a selected hand spell
        // when the player intentionally attacks.
        if (!manager.PrepareAttacker(slotIndex))
            return;

        draggingAttack = true;

        battleUI.HideMonsterTooltip();

        battleUI.BeginAttackArrow(
            GetComponent<RectTransform>()
        );
    }

    // ARROW FOLLOWS THE MOUSE

    public void OnDrag(PointerEventData eventData)
    {
        // BattleAttackArrow handles cursor movement.
    }

    // RELEASE OVER AN ENEMY OR DIRECT ATTACK TARGET

    public void OnEndDrag(
        PointerEventData eventData)
    {
        if (!draggingAttack)
            return;

        draggingAttack = false;

        if (battleUI != null)
            battleUI.StopAttackArrow();

        GameObject targetObject =
            eventData.pointerCurrentRaycast.gameObject;

        bool completedAttack = false;

        if (targetObject != null && manager != null)
        {
            BattleSlotUI targetSlot =
                targetObject.GetComponentInParent<BattleSlotUI>();

            // MONSTER VERSUS MONSTER ATTACK
            if (targetSlot != null &&
                !targetSlot.isPlayerSlot &&
                targetSlot.displayedMonster != null)
            {
                manager.SelectMonsterSlot(
                    false,
                    targetSlot.slotIndex
                );

                completedAttack = true;
            }

            // DIRECT ATTACK
            else if (battleUI != null &&
                     battleUI.IsDirectAttackTarget(targetObject) &&
                     manager.CanDirectAttack)
            {
                manager.AttackOpponentDirectly();
                completedAttack = true;
            }
        }

        // INVALID TARGET: CANCEL ATTACK
        if (!completedAttack && manager != null)
            manager.CancelAttackSelection();
    }

    private void OnDisable()
    {
        draggingAttack = false;

        if (battleUI != null)
        {
            battleUI.HideMonsterTooltip();
            battleUI.StopAttackArrow();
        }
    }
}
