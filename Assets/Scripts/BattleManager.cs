
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleDeckLoader deckLoader;
    [SerializeField] private BattleUIController battleUI;

    [Header("Starting Rules")]
    [SerializeField] private int startingPlayerHP = 100;
    [SerializeField] private int startingSP = 20;
    [SerializeField] private int spPerTurn = 10;
    [SerializeField] private int openingHandSize = 5;

    public BattlePlayer Player { get; private set; }
    public BattlePlayer Opponent { get; private set; }

    public bool GameOver { get; private set; }

    private int selectedHandIndex = -1;
    private int selectedAttackerIndex = -1;

    // DIRECT ATTACK AVAILABILITY

    public bool CanDirectAttack
    {
        get
        {
            if (GameOver ||
                Player == null ||
                Opponent == null)
                return false;

            if (Opponent.HasMonsters())
                return false;

            if (selectedAttackerIndex < 0 ||
                selectedAttackerIndex >= Player.monsterSlots.Length)
                return false;

            BattleMonster attacker =
                Player.monsterSlots[selectedAttackerIndex];

            return attacker != null && attacker.CanAttack();
        }
    }

    private void Start()
    {
        StartBattle();
    }

    // START BATTLE

    public void StartBattle()
    {
        if (deckLoader == null || battleUI == null)
        {
            Debug.LogError(
                "BattleManager: Assign Deck Loader and Battle UI!",
                this
            );
            return;
        }

        List<BattleCard> playerCards =
            deckLoader.LoadPlayerDeck();

        List<BattleCard> enemyCards =
            deckLoader.LoadAIDeck();

        if (playerCards.Count < openingHandSize ||
            enemyCards.Count < openingHandSize)
        {
            Debug.LogError(
                "Battle: Both decks need at least " +
                openingHandSize + " valid cards.",
                this
            );
            return;
        }

        Player = new BattlePlayer(
            "Player",
            playerCards,
            startingPlayerHP,
            startingSP
        );

        Opponent = new BattlePlayer(
            "AI Opponent",
            enemyCards,
            startingPlayerHP,
            startingSP
        );

        GameOver = false;
        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        Player.DrawCards(openingHandSize);
        Opponent.DrawCards(openingHandSize);

        Player.turnsStarted = 1;

        battleUI.Setup();
        battleUI.Refresh();

        Message("Your turn! Select a card to begin.");
    }

    // SELECT A CARD FROM HAND

    public void SelectHandCard(int index)
    {
        if (GameOver || Player == null)
            return;

        if (index < 0 || index >= Player.hand.Count)
            return;

        BattleCard card = Player.hand[index];

        selectedAttackerIndex = -1;

        if (card.type == BattleCardType.Spell)
        {
            selectedHandIndex = -1;

            Message(
                card.Name +
                ": Spell casting will be added after " +
                "basic monster combat."
            );

            battleUI.Refresh();
            return;
        }

        if (card.monster == null)
            return;

        selectedHandIndex = index;

        Message(
            "Selected " + card.Name +
            ". Click an empty friendly monster slot " +
            "to summon it. Cost: " +
            card.monster.summonCost + " SP."
        );

        battleUI.Refresh();
    }

    // SELECT A FIELD SLOT

    public void SelectMonsterSlot(
        bool isPlayer,
        int index)
    {
        if (GameOver || Player == null || Opponent == null)
            return;

        BattlePlayer targetPlayer =
            isPlayer ? Player : Opponent;

        if (index < 0 ||
            index >= targetPlayer.monsterSlots.Length)
            return;

        BattleMonster target =
            targetPlayer.monsterSlots[index];

        if (isPlayer)
        {
            // Summon selected hand monster.
            if (selectedHandIndex >= 0 && target == null)
            {
                SummonPlayerMonster(index);
                return;
            }

            selectedHandIndex = -1;
            selectedAttackerIndex = -1;

            if (target != null)
            {
                if (target.CanAttack())
                {
                    selectedAttackerIndex = index;

                    if (Opponent.HasMonsters())
                    {
                        Message(
                            "Selected " +
                            target.data.monsterName +
                            ". Click an enemy monster to attack!"
                        );
                    }
                    else
                    {
                        Message(
                            "Selected " +
                            target.data.monsterName +
                            ". The opponent has no monsters. " +
                            "Click the red DIRECT ATTACK panel!"
                        );
                    }
                }
                else
                {
                    Message(
                        target.data.monsterName +
                        " cannot attack yet or has already attacked."
                    );
                }
            }

            // Important: updates the red target immediately.
            battleUI.Refresh();
            return;
        }

        // Enemy slot selection.
        if (target != null && selectedAttackerIndex >= 0)
        {
            AttackEnemyMonster(index);
        }
        else if (target != null)
        {
            Message(
                "Enemy monster: " + target.data.monsterName +
                " | HP: " + target.currentHP +
                "/" + target.MaxHP
            );
        }
    }

    // SUMMON MONSTER

    private void SummonPlayerMonster(int slot)
    {
        if (selectedHandIndex < 0 ||
            selectedHandIndex >= Player.hand.Count)
            return;

        BattleCard card = Player.hand[selectedHandIndex];

        if (card.type != BattleCardType.Monster ||
            card.monster == null)
            return;

        int cost = Mathf.Max(
            0,
            card.monster.summonCost
        );

        if (Player.currentSP < cost)
        {
            Message(
                "Not enough SP to summon " + card.Name
            );
            return;
        }

        if (Player.monsterSlots[slot] != null)
            return;

        Player.currentSP -= cost;

        Player.monsterSlots[slot] =
            new BattleMonster(card, Player);

        Player.hand.RemoveAt(selectedHandIndex);

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        Message(
            card.Name +
            " summoned! It must wait until your next turn to attack."
        );

        battleUI.Refresh();
    }

    // ATTACK ENEMY MONSTER

    private void AttackEnemyMonster(int enemySlot)
    {
        if (selectedAttackerIndex < 0)
            return;

        BattleMonster attacker =
            Player.monsterSlots[selectedAttackerIndex];

        BattleMonster defender =
            Opponent.monsterSlots[enemySlot];

        if (attacker == null || defender == null)
            return;

        if (!attacker.CanAttack())
        {
            Message("This monster cannot attack yet.");
            selectedAttackerIndex = -1;
            battleUI.Refresh();
            return;
        }

        int damage = Mathf.Max(
            1,
            attacker.Attack - defender.Defence
        );

        defender.TakeDamage(damage);
        attacker.hasAttacked = true;

        Message(
            attacker.data.monsterName +
            " attacked " +
            defender.data.monsterName +
            " for " + damage + " damage!"
        );

        if (defender.currentHP <= 0)
            DefeatMonster(Opponent, enemySlot);

        selectedAttackerIndex = -1;

        CheckVictory();
        battleUI.Refresh();
    }

    // DIRECT ATTACK

    public void AttackOpponentDirectly()
    {
        if (!CanDirectAttack)
        {
            Message(
                "Direct attack is not currently available."
            );
            battleUI.Refresh();
            return;
        }

        BattleMonster attacker =
            Player.monsterSlots[selectedAttackerIndex];

        int damage = attacker.Attack;

        Opponent.currentHP = Mathf.Max(
            0,
            Opponent.currentHP - damage
        );

        attacker.hasAttacked = true;

        selectedAttackerIndex = -1;
        selectedHandIndex = -1;

        Message(
            attacker.data.monsterName +
            " directly attacked the opponent for " +
            damage + " damage!"
        );

        CheckVictory();
        battleUI.Refresh();
    }

    // MOVE DEFEATED MONSTER TO GRAVEYARD

    private void DefeatMonster(
        BattlePlayer owner,
        int slot)
    {
        BattleMonster monster = owner.monsterSlots[slot];

        if (monster == null)
            return;

        owner.graveyard.Add(monster.originalCard);
        owner.monsterSlots[slot] = null;

        Message(
            monster.data.monsterName +
            " was defeated!"
        );
    }

    // END PLAYER TURN

    public void EndTurn()
    {
        if (GameOver || Player == null)
            return;

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        battleUI.Refresh();

        RunAITurn();

        if (GameOver)
            return;

        StartPlayerTurn();
    }

    // BEGIN PLAYER TURN

    private void StartPlayerTurn()
    {
        Player.turnsStarted++;
        Player.currentSP += spPerTurn;
        Player.DrawCard();

        foreach (BattleMonster monster in Player.monsterSlots)
        {
            if (monster != null)
                monster.RefreshForTurn();
        }

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        battleUI.Refresh();

        Message("Your turn!");
    }

    // BASIC AI OPPONENT

    private void RunAITurn()
    {
        Opponent.turnsStarted++;
        Opponent.currentSP += spPerTurn;
        Opponent.DrawCard();

        foreach (BattleMonster monster in Opponent.monsterSlots)
        {
            if (monster != null)
                monster.RefreshForTurn();
        }

        // AI summons the first affordable monster.
        int emptySlot = Opponent.FirstEmptyMonsterSlot();

        if (emptySlot >= 0)
        {
            for (int i = 0; i < Opponent.hand.Count; i++)
            {
                BattleCard card = Opponent.hand[i];

                if (card.type != BattleCardType.Monster ||
                    card.monster == null)
                    continue;

                int cost = Mathf.Max(
                    0,
                    card.monster.summonCost
                );

                if (cost > Opponent.currentSP)
                    continue;

                Opponent.currentSP -= cost;

                Opponent.monsterSlots[emptySlot] =
                    new BattleMonster(card, Opponent);

                Opponent.hand.RemoveAt(i);

                Message("AI summoned " + card.Name);
                break;
            }
        }

        // AI attacks with eligible monsters.
        for (int i = 0; i < Opponent.monsterSlots.Length; i++)
        {
            BattleMonster attacker =
                Opponent.monsterSlots[i];

            if (attacker == null || !attacker.CanAttack())
                continue;

            int targetSlot = -1;

            for (int j = 0; j < Player.monsterSlots.Length; j++)
            {
                if (Player.monsterSlots[j] != null)
                {
                    targetSlot = j;
                    break;
                }
            }

            if (targetSlot >= 0)
            {
                BattleMonster defender =
                    Player.monsterSlots[targetSlot];

                int damage = Mathf.Max(
                    1,
                    attacker.Attack - defender.Defence
                );

                defender.TakeDamage(damage);
                attacker.hasAttacked = true;

                Message(
                    "AI attacked " +
                    defender.data.monsterName +
                    " for " + damage + " damage."
                );

                if (defender.currentHP <= 0)
                    DefeatMonster(Player, targetSlot);
            }
            else
            {
                int damage = attacker.Attack;

                Player.currentHP = Mathf.Max(
                    0,
                    Player.currentHP - damage
                );

                attacker.hasAttacked = true;

                Message(
                    "AI dealt " + damage +
                    " direct damage!"
                );
            }

            if (CheckVictory())
                break;
        }

        battleUI.Refresh();
    }

    // CHECK WINNER

    private bool CheckVictory()
    {
        if (Player.currentHP <= 0)
        {
            FinishBattle("AI wins!");
            return true;
        }

        if (Opponent.currentHP <= 0)
        {
            FinishBattle("Player wins!");
            return true;
        }

        return false;
    }

    private void FinishBattle(string result)
    {
        GameOver = true;
        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        Message("BATTLE OVER! " + result);
    }

    // STATUS MESSAGES

    private void Message(string text)
    {
        if (battleUI != null)
            battleUI.ShowMessage(text);
        else
            Debug.Log(text);
    }
}
