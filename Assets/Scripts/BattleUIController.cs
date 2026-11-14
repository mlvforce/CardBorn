
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIController : MonoBehaviour
{
    [Header("Battle Manager")]
    [SerializeField] private BattleManager manager;

    [Header("Player UI")]
    [SerializeField] private TMP_Text playerHPText;
    [SerializeField] private TMP_Text playerSPText;
    [SerializeField] private TMP_Text playerDeckText;
    [SerializeField] private TMP_Text playerGraveText;

    [Header("Opponent UI")]
    [SerializeField] private TMP_Text opponentHPText;
    [SerializeField] private TMP_Text opponentSPText;
    [SerializeField] private TMP_Text opponentDeckText;
    [SerializeField] private TMP_Text opponentGraveText;

    [Header("Battlefield Monster Rows")]
    [SerializeField] private RectTransform playerMonsterField;
    [SerializeField] private RectTransform opponentMonsterField;

    [Header("Battlefield Card Prefab")]
    [SerializeField] private GameObject monsterCardPrefab;

    [Header("Player Hand")]
    [SerializeField] private BattleHandUI handUI;

    [Header("Battle Controls")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button directAttackButton;

    [Header("Card Interactions")]
    [SerializeField] private BattleCardTooltip cardTooltip;
    [SerializeField] private BattleCardPreview cardPreview;
    [SerializeField] private BattleAttackArrow attackArrow;

    [Header("Optional Battle Messages")]
    [SerializeField] private TMP_Text statusText;

    [Header("Hand Limit Warning")]
    [SerializeField] private GameObject handLimitWarningPanel;
    [SerializeField] private TMP_Text handLimitWarningText;

    private BattleSlotUI[] playerSlots;
    private BattleSlotUI[] opponentSlots;

    // INITIAL SETUP

    public void Setup()
    {
        if (manager == null)
        {
            Debug.LogError(
                "BattleUIController: Assign Battle Manager!",
                this
            );
            return;
        }

        playerSlots = SetupMonsterRow(
            playerMonsterField,
            true
        );

        opponentSlots = SetupMonsterRow(
            opponentMonsterField,
            false
        );

        // END TURN BUTTON

        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveAllListeners();

            endTurnButton.onClick.AddListener(
                manager.EndTurn
            );
        }

        // DIRECT ATTACK BUTTON

        if (directAttackButton != null)
        {
            directAttackButton.onClick.RemoveAllListeners();

            directAttackButton.onClick.AddListener(
                manager.AttackOpponentDirectly
            );

            directAttackButton.gameObject.SetActive(false);
        }

        // HIDE OPTIONAL UI

        if (cardTooltip != null)
            cardTooltip.Hide();

        if (cardPreview != null)
            cardPreview.Hide();

        if (attackArrow != null)
            attackArrow.StopArrow();

        if (handLimitWarningPanel != null)
            handLimitWarningPanel.SetActive(false);

        Debug.Log(
            "CardBorn Battle UI initialised!",
            this
        );
    }

    // REGISTER MONSTER SLOTS

    private BattleSlotUI[] SetupMonsterRow(
        RectTransform field,
        bool belongsToPlayer)
    {
        if (field == null)
        {
            Debug.LogWarning(
                "BattleUIController: Monster field missing!",
                this
            );

            return new BattleSlotUI[0];
        }

        Button[] buttons =
            field.GetComponentsInChildren<Button>(true);

        BattleSlotUI[] slots =
            new BattleSlotUI[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            BattleSlotUI slot =
                buttons[i].GetComponent<BattleSlotUI>();

            if (slot == null)
            {
                slot = buttons[i].gameObject
                    .AddComponent<BattleSlotUI>();
            }

            slot.ConfigurePrefab(monsterCardPrefab);
            slot.ConfigureInteractions(this);

            slot.Initialize(
                manager,
                belongsToPlayer,
                i
            );

            slots[i] = slot;
        }

        if (slots.Length != 5)
        {
            Debug.LogWarning(
                field.name + " contains " +
                slots.Length +
                " monster buttons. Expected 5.",
                this
            );
        }

        return slots;
    }

    // REFRESH ALL BATTLE UI

    public void Refresh()
    {
        if (manager == null)
            return;

        BattlePlayer player = manager.Player;
        BattlePlayer opponent = manager.Opponent;

        if (player == null || opponent == null)
        {
            if (directAttackButton != null)
                directAttackButton.gameObject.SetActive(false);

            if (handLimitWarningPanel != null)
                handLimitWarningPanel.SetActive(false);

            return;
        }

        // PLAYER STATS

        SetText(
            playerHPText,
            "HP: " + player.currentHP
        );

        SetText(
            playerSPText,
            "SP: " + player.currentSP
        );

        SetText(
            playerDeckText,
            "Deck: " + player.deck.Count
        );

        SetText(
            playerGraveText,
            "Grave: " + player.graveyard.Count
        );

        // OPPONENT STATS

        SetText(
            opponentHPText,
            "HP: " + opponent.currentHP
        );

        SetText(
            opponentSPText,
            "SP: " + opponent.currentSP
        );

        SetText(
            opponentDeckText,
            "Deck: " + opponent.deck.Count
        );

        SetText(
            opponentGraveText,
            "Grave: " + opponent.graveyard.Count
        );

        // MONSTER CARD SLOTS

        RefreshMonsterRow(playerSlots, player);
        RefreshMonsterRow(opponentSlots, opponent);

        // PLAYER HAND

        if (handUI != null)
        {
            handUI.ShowHand(
                player,
                manager
            );
        }

        // END TURN BUTTON

        if (endTurnButton != null)
        {
            endTurnButton.interactable =
                !manager.GameOver &&
                !manager.IsDiscarding;
        }

        // DIRECT ATTACK PANEL

        UpdateDirectAttackPanel();

        // HAND LIMIT WARNING

        UpdateHandLimitWarning();
    }

    // HAND LIMIT WARNING PANEL

    private void UpdateHandLimitWarning()
    {
        bool showWarning =
            manager != null &&
            manager.IsDiscarding &&
            !manager.GameOver;

        if (handLimitWarningPanel != null)
        {
            handLimitWarningPanel.SetActive(
                showWarning
            );
        }

        if (!showWarning)
            return;

        if (handLimitWarningText != null)
        {
            handLimitWarningText.text =
                "HAND LIMIT EXCEEDED!\n" +
                "Click a card in your hand to discard.";
        }
    }

    // DIRECT ATTACK PANEL

    private void UpdateDirectAttackPanel()
    {
        if (directAttackButton == null)
            return;

        bool available =
            manager != null &&
            manager.CanDirectAttack &&
            !manager.IsDiscarding;

        directAttackButton.gameObject.SetActive(
            available
        );

        directAttackButton.interactable = available;
    }

    // REFRESH MONSTER SLOTS

    private void RefreshMonsterRow(
        BattleSlotUI[] slots,
        BattlePlayer player)
    {
        if (slots == null || player == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;

            BattleMonster monster = null;

            if (i < player.monsterSlots.Length)
            {
                monster = player.monsterSlots[i];
            }

            slots[i].Refresh(monster);
        }
    }

    // MONSTER HOVER TOOLTIP

    public void ShowMonsterTooltip(
        BattleMonster monster,
        RectTransform cardRect)
    {
        if (cardTooltip != null)
        {
            cardTooltip.ShowMonster(
                monster,
                cardRect
            );
        }
    }

    public void HideMonsterTooltip()
    {
        if (cardTooltip != null)
            cardTooltip.Hide();
    }

    // LARGE MONSTER PREVIEW

    public void OpenMonsterPreview(
        BattleMonster monster)
    {
        HideMonsterTooltip();

        if (cardPreview != null)
        {
            cardPreview.ShowMonster(monster);
        }
    }

    // RED ATTACK ARROW

    public void BeginAttackArrow(
        RectTransform source)
    {
        if (attackArrow != null)
        {
            attackArrow.BeginArrow(source);
        }
    }

    public void StopAttackArrow()
    {
        if (attackArrow != null)
            attackArrow.StopArrow();
    }

    // CHECK DIRECT ATTACK TARGET

    public bool IsDirectAttackTarget(
        GameObject target)
    {
        if (directAttackButton == null ||
            target == null)
            return false;

        Transform buttonTransform =
            directAttackButton.transform;

        return target.transform == buttonTransform ||
               target.transform.IsChildOf(buttonTransform);
    }

    // SAFE TEXT UPDATE

    private void SetText(
        TMP_Text textComponent,
        string value)
    {
        if (textComponent != null)
            textComponent.text = value;
    }

    // GENERAL BATTLE MESSAGES

    public void ShowMessage(string message)
    {
        Debug.Log(
            "[CardBorn Battle] " + message
        );

        if (statusText != null)
            statusText.text = message;
    }
}
