
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

    [Header("Direct Attack Panel")]
    [SerializeField] private Button directAttackButton;

    [Header("Optional Battle Messages")]
    [SerializeField] private TMP_Text statusText;

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

        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveAllListeners();

            endTurnButton.onClick.AddListener(
                manager.EndTurn
            );
        }

        if (directAttackButton != null)
        {
            directAttackButton.onClick.RemoveAllListeners();

            directAttackButton.onClick.AddListener(
                manager.AttackOpponentDirectly
            );

            directAttackButton.gameObject.SetActive(false);
        }

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
                field.name +
                " has " + slots.Length +
                " buttons. Expected 5.",
                this
            );
        }

        return slots;
    }

    // REFRESH BATTLE UI

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

            return;
        }

        UpdateText(
            playerHPText,
            "HP: " + player.currentHP
        );

        UpdateText(
            playerSPText,
            "SP: " + player.currentSP
        );

        UpdateText(
            playerDeckText,
            "Deck: " + player.deck.Count
        );

        UpdateText(
            playerGraveText,
            "Grave: " + player.graveyard.Count
        );

        UpdateText(
            opponentHPText,
            "HP: " + opponent.currentHP
        );

        UpdateText(
            opponentSPText,
            "SP: " + opponent.currentSP
        );

        UpdateText(
            opponentDeckText,
            "Deck: " + opponent.deck.Count
        );

        UpdateText(
            opponentGraveText,
            "Grave: " + opponent.graveyard.Count
        );

        RefreshMonsterRow(playerSlots, player);
        RefreshMonsterRow(opponentSlots, opponent);

        if (handUI != null)
            handUI.ShowHand(player, manager);

        if (endTurnButton != null)
        {
            endTurnButton.interactable =
                !manager.GameOver;
        }

        UpdateDirectAttackPanel();
    }

    // SHOW RED DIRECT ATTACK TARGET

    private void UpdateDirectAttackPanel()
    {
        if (directAttackButton == null)
            return;

        bool available = manager != null &&
                         manager.CanDirectAttack;

        directAttackButton.gameObject.SetActive(available);
        directAttackButton.interactable = available;
    }

    // REFRESH MONSTER DISPLAYS

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
                monster = player.monsterSlots[i];

            slots[i].Refresh(monster);
        }
    }

    // UPDATE TMP TEXT

    private void UpdateText(
        TMP_Text label,
        string value)
    {
        if (label != null)
            label.text = value;
    }

    // STATUS MESSAGES

    public void ShowMessage(string message)
    {
        Debug.Log(
            "[CardBorn Battle] " + message
        );

        if (statusText != null)
            statusText.text = message;
    }
}
