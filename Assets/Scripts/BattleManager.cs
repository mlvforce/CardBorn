
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleDeckLoader deckLoader;
    [SerializeField] private BattleUIController battleUI;

    [Header("Graveyard Spell UI")]
    [SerializeField] private BattleGraveyardUI graveyardUI;

    [Header("Starting Rules")]
    [SerializeField] private int startingPlayerHP = 100;
    [SerializeField] private int startingSP = 20;
    [SerializeField] private int spPerTurn = 10;
    [SerializeField] private int openingHandSize = 5;

    [Header("Battle Rules")]
    [SerializeField, Min(1)] private int maximumHandSize = 7;
    [SerializeField, Min(1)] private int maximumSP = 100;

    public BattlePlayer Player { get; private set; }
    public BattlePlayer Opponent { get; private set; }
    public bool GameOver { get; private set; }
    public bool IsDiscarding { get; private set; }

    private int selectedHandIndex = -1;
    private int selectedAttackerIndex = -1;

    public int SelectedHandIndex => selectedHandIndex;

    public bool HasPendingSpell
    {
        get
        {
            if (GameOver || IsDiscarding || Player == null)
                return false;

            if (selectedHandIndex < 0 ||
                selectedHandIndex >= Player.hand.Count)
                return false;

            BattleCard card = Player.hand[selectedHandIndex];

            return card != null &&
                   card.type == BattleCardType.Spell &&
                   card.spell != null;
        }
    }

    public bool CanDirectAttack
    {
        get
        {
            if (GameOver || IsDiscarding ||
                Player == null || Opponent == null ||
                Opponent.HasMonsters())
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
                "BattleManager: Missing DeckLoader or BattleUI.",
                this
            );
            return;
        }

        List<BattleCard> playerCards =
            deckLoader.LoadPlayerDeck();

        List<BattleCard> enemyCards =
            deckLoader.LoadAIDeck();

        if (playerCards == null || enemyCards == null ||
            playerCards.Count < openingHandSize ||
            enemyCards.Count < openingHandSize)
        {
            Debug.LogError(
                "Both decks need enough valid cards.",
                this
            );
            return;
        }

        Player = new BattlePlayer(
            "Player",
            playerCards,
            startingPlayerHP,
            Mathf.Clamp(startingSP, 0, maximumSP)
        );

        Opponent = new BattlePlayer(
            "AI Opponent",
            enemyCards,
            startingPlayerHP,
            Mathf.Clamp(startingSP, 0, maximumSP)
        );

        GameOver = false;
        IsDiscarding = false;
        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        Player.DrawCards(openingHandSize);
        Opponent.DrawCards(openingHandSize);

        Player.turnsStarted = 1;

        battleUI.Setup();
        battleUI.Refresh();

        Message("Battle started! Your turn.");
    }

    // SELECT HAND CARD

    public void SelectHandCard(int index)
    {
        if (GameOver || Player == null ||
            index < 0 || index >= Player.hand.Count)
            return;

        if (IsDiscarding)
        {
            DiscardHandCard(index);
            return;
        }

        BattleCard card = Player.hand[index];

        if (card == null)
            return;

        if (graveyardUI != null)
            graveyardUI.Close();

        selectedHandIndex = index;
        selectedAttackerIndex = -1;

        if (card.type == BattleCardType.Spell)
        {
            SelectSpellCard(card);
        }
        else if (card.type == BattleCardType.Monster &&
                 card.monster != null)
        {
            Message(
                "Selected " + card.Name +
                ". Click an empty monster slot to summon. " +
                "Cost: " + card.monster.summonCost + " SP."
            );
        }

        battleUI.Refresh();
    }

    // SELECT SPELL

    private void SelectSpellCard(BattleCard card)
    {
        if (card.spell == null)
        {
            selectedHandIndex = -1;
            return;
        }

        SpellData spell = card.spell;

        if (!CanMeetSpellRequirement(spell))
        {
            Message(
                spell.spellName +
                " requires a living friendly monster with " +
                spell.requiredInt + " INT."
            );
            return;
        }

        // SOUL REBORN

        if (spell.effectType ==
            SpellEffectType.ReturnMonsterToHand)
        {
            int available = 0;

            foreach (BattleCard graveCard in Player.graveyard)
            {
                if (graveCard != null &&
                    graveCard.type == BattleCardType.Monster &&
                    graveCard.monster != null)
                {
                    available++;
                }
            }

            if (available == 0)
            {
                Message(
                    "Soul Reborn: No monsters in your graveyard."
                );
                return;
            }

            if (Player.hand.Count > maximumHandSize)
            {
                Message(
                    "Reduce your hand to " +
                    maximumHandSize +
                    " cards before casting Soul Reborn."
                );
                return;
            }

            if (graveyardUI == null)
            {
                Message(
                    "Assign BattleGraveyardUI to BattleManager."
                );
                return;
            }

            graveyardUI.Open(this);

            Message(
                "Soul Reborn: Choose a monster from your graveyard."
            );
            return;
        }

        // FIRE BALL / HEALING LIGHT

        if (spell.effectType == SpellEffectType.Damage ||
            spell.effectType == SpellEffectType.Heal)
        {
            Message(
                spell.spellName +
                " selected. Choose a monster target."
            );
        }
        else
        {
            Message(
                spell.spellName +
                " is not implemented in combat yet."
            );
        }
    }

    // CHECK INT REQUIREMENT

    private bool CanMeetSpellRequirement(SpellData spell)
    {
        return spell != null &&
               Player != null &&
               Player.HasMonsters() &&
               Player.HighestMonsterINT() >= spell.requiredInt;
    }

    // SOUL REBORN

    public bool TryReturnMonsterFromGrave(int graveIndex)
    {
        if (GameOver || IsDiscarding ||
            Player == null || !HasPendingSpell)
            return false;

        BattleCard spellCard =
            Player.hand[selectedHandIndex];

        SpellData spell = spellCard.spell;

        if (spell.effectType !=
            SpellEffectType.ReturnMonsterToHand)
            return false;

        if (!CanMeetSpellRequirement(spell))
        {
            Message(
                "Soul Reborn: INT requirement not met."
            );
            return false;
        }

        if (Player.hand.Count > maximumHandSize)
        {
            Message(
                "Too many cards in hand to cast Soul Reborn."
            );
            return false;
        }

        if (graveIndex < 0 ||
            graveIndex >= Player.graveyard.Count)
            return false;

        BattleCard recovered =
            Player.graveyard[graveIndex];

        if (recovered == null ||
            recovered.type != BattleCardType.Monster ||
            recovered.monster == null)
            return false;

        // Remove the defeated monster from the grave.
        Player.graveyard.RemoveAt(graveIndex);

        // Consume Soul Reborn.
        Player.hand.RemoveAt(selectedHandIndex);

        // Return the monster to the hand.
        Player.hand.Add(recovered);

        // Soul Reborn enters the graveyard.
        Player.graveyard.Add(spellCard);

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        if (graveyardUI != null)
            graveyardUI.Close();

        battleUI.Refresh();

        Message(
            "Soul Reborn returned " +
            recovered.Name +
            " to your hand. Normal summoning SP applies."
        );

        return true;
    }

    // CAST DAMAGE / HEALING SPELL

    public bool TryCastSelectedSpellOn(
        bool targetIsFriendly,
        int targetSlot)
    {
        if (!HasPendingSpell)
            return false;

        SpellData spell =
            Player.hand[selectedHandIndex].spell;

        // Soul Reborn uses the graveyard instead.
        if (spell.effectType ==
            SpellEffectType.ReturnMonsterToHand)
            return true;

        if (!CanMeetSpellRequirement(spell))
        {
            Message(
                "Cannot cast. INT requirement not met."
            );
            return true;
        }

        BattlePlayer targetOwner =
            targetIsFriendly ? Player : Opponent;

        if (targetOwner == null ||
            targetSlot < 0 ||
            targetSlot >= targetOwner.monsterSlots.Length)
            return true;

        BattleMonster target =
            targetOwner.monsterSlots[targetSlot];

        if (target == null || target.currentHP <= 0)
        {
            Message("Select a living monster target.");
            return true;
        }

        if (!IsValidSpellTarget(spell, targetIsFriendly))
        {
            Message("Invalid spell target.");
            return true;
        }

        switch (spell.effectType)
        {
            case SpellEffectType.Damage:
                {
                    int damage = Mathf.Max(
                        0, spell.effectStrength
                    );

                    // True damage ignores DEF.
                    // BattleMonster.TakeDamage handles shields.
                    int oldHP = target.currentHP;
                    int oldShield = target.shield;

                    target.TakeDamage(damage);

                    Message(
                        spell.spellName + " dealt " +
                        damage + " damage to " +
                        target.data.monsterName + "."
                    );

                    if (target.currentHP <= 0)
                        DefeatMonster(targetOwner, targetSlot);

                    break;
                }

            case SpellEffectType.Heal:
                {
                    int oldHP = target.currentHP;

                    target.Heal(spell.effectStrength);

                    int healed = target.currentHP - oldHP;

                    Message(
                        spell.spellName + " restored " +
                        healed + " HP to " +
                        target.data.monsterName + "."
                    );

                    break;
                }

            default:
                {
                    Message(
                        "This spell effect is not implemented yet."
                    );
                    return true;
                }
        }

        BattleCard usedCard =
            Player.hand[selectedHandIndex];

        Player.hand.RemoveAt(selectedHandIndex);
        Player.graveyard.Add(usedCard);

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        CheckVictory();
        battleUI.Refresh();

        return true;
    }

    private bool IsValidSpellTarget(
        SpellData spell,
        bool targetIsFriendly)
    {
        switch (spell.targetType)
        {
            case SpellTargetType.FriendlyMonster:
                return targetIsFriendly;

            case SpellTargetType.EnemyMonster:
                return !targetIsFriendly;

            case SpellTargetType.AnyMonster:
                return true;

            default:
                return false;
        }
    }

    // SELECT A FIELD SLOT

    public void SelectMonsterSlot(bool isPlayer, int index)
    {
        if (GameOver || IsDiscarding ||
            Player == null || Opponent == null)
            return;

        if (TryCastSelectedSpellOn(isPlayer, index))
            return;

        BattlePlayer owner =
            isPlayer ? Player : Opponent;

        if (index < 0 ||
            index >= owner.monsterSlots.Length)
            return;

        BattleMonster target = owner.monsterSlots[index];

        if (isPlayer)
        {
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

                    Message(
                        Opponent.HasMonsters()
                        ? "Select an enemy monster to attack."
                        : "Direct attack available!"
                    );
                }
                else
                {
                    Message("This monster cannot attack yet.");
                }
            }

            battleUI.Refresh();
            return;
        }

        if (target != null && selectedAttackerIndex >= 0)
            AttackEnemyMonster(index);
    }

    // SUMMON MONSTER

    private void SummonPlayerMonster(int slot)
    {
        if (IsDiscarding || selectedHandIndex < 0 ||
            selectedHandIndex >= Player.hand.Count)
            return;

        BattleCard card = Player.hand[selectedHandIndex];

        if (card.type != BattleCardType.Monster ||
            card.monster == null ||
            Player.monsterSlots[slot] != null)
            return;

        int cost = Mathf.Max(
            0, card.monster.summonCost
        );

        if (Player.currentSP < cost)
        {
            Message("Not enough SP to summon " + card.Name);
            return;
        }

        Player.currentSP -= cost;

        Player.monsterSlots[slot] =
            new BattleMonster(card, Player);

        Player.hand.RemoveAt(selectedHandIndex);

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        Message(
            card.Name +
            " summoned! It can attack next turn."
        );

        battleUI.Refresh();
    }

    // PREPARE ATTACK DRAGGING

    public bool PrepareAttacker(int slotIndex)
    {
        if (GameOver || IsDiscarding ||
            Player == null || Opponent == null)
            return false;

        if (slotIndex < 0 ||
            slotIndex >= Player.monsterSlots.Length)
            return false;

        BattleMonster attacker =
            Player.monsterSlots[slotIndex];

        if (attacker == null || !attacker.CanAttack())
            return false;

        selectedHandIndex = -1;
        selectedAttackerIndex = slotIndex;

        if (graveyardUI != null)
            graveyardUI.Close();

        battleUI.Refresh();
        return true;
    }

    public void CancelAttackSelection()
    {
        selectedAttackerIndex = -1;

        battleUI.Refresh();
    }

    // DEF DAMAGE CALCULATION

    private int CalculateMonsterDamage(
        BattleMonster attacker,
        BattleMonster defender)
    {
        if (attacker == null || defender == null)
            return 0;

        float attack = Mathf.Max(0, attacker.Attack);
        float defence = Mathf.Max(0, defender.Defence);

        float result =
            attack * (100f / (100f + defence));

        return Mathf.Max(1, Mathf.CeilToInt(result));
    }

    // ATTACK ENEMY MONSTER

    private void AttackEnemyMonster(int enemySlot)
    {
        if (GameOver || IsDiscarding ||
            selectedAttackerIndex < 0)
            return;

        if (enemySlot < 0 ||
            enemySlot >= Opponent.monsterSlots.Length)
            return;

        BattleMonster attacker =
            Player.monsterSlots[selectedAttackerIndex];

        BattleMonster defender =
            Opponent.monsterSlots[enemySlot];

        if (attacker == null || defender == null)
            return;

        if (!attacker.CanAttack())
        {
            selectedAttackerIndex = -1;
            battleUI.Refresh();
            return;
        }

        int damage =
            CalculateMonsterDamage(attacker, defender);

        defender.TakeDamage(damage);
        attacker.hasAttacked = true;

        Message(
            attacker.data.monsterName +
            " attacked " +
            defender.data.monsterName +
            " for " + damage + " damage."
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
            Message("Direct attack is unavailable.");
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

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        Message(
            "Direct attack dealt " + damage + " damage!"
        );

        CheckVictory();
        battleUI.Refresh();
    }

    // DEFEATED MONSTERS

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
            monster.data.monsterName + " was defeated!"
        );
    }

    // END TURN / DISCARD RULE

    public void EndTurn()
    {
        if (GameOver || Player == null)
            return;

        if (graveyardUI != null)
            graveyardUI.Close();

        if (IsDiscarding)
        {
            Message(
                "Discard until your hand contains " +
                maximumHandSize + " cards."
            );
            return;
        }

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        if (Player.hand.Count > maximumHandSize)
        {
            IsDiscarding = true;

            battleUI.Refresh();

            Message(
                "HAND LIMIT! Click " +
                (Player.hand.Count - maximumHandSize) +
                " card(s) to discard."
            );

            return;
        }

        CompleteEndTurn();
    }

    private void DiscardHandCard(int index)
    {
        if (!IsDiscarding || Player == null ||
            index < 0 || index >= Player.hand.Count)
            return;

        BattleCard discardedCard =
            Player.hand[index];

        Player.hand.RemoveAt(index);
        Player.graveyard.Add(discardedCard);

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        battleUI.Refresh();

        if (Player.hand.Count > maximumHandSize)
        {
            Message(
                "Discarded " + discardedCard.Name +
                ". Choose " +
                (Player.hand.Count - maximumHandSize) +
                " more card(s) to discard."
            );
            return;
        }

        IsDiscarding = false;

        Message("Hand limit satisfied. Ending turn.");

        CompleteEndTurn();
    }

    private void CompleteEndTurn()
    {
        if (GameOver)
            return;

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        RunAITurn();

        if (!GameOver)
            StartPlayerTurn();
        else
            battleUI.Refresh();
    }

    // START PLAYER TURN

    private void StartPlayerTurn()
    {
        Player.turnsStarted++;

        Player.currentSP = Mathf.Min(
            maximumSP,
            Player.currentSP + spPerTurn
        );

        Player.DrawCard();

        foreach (BattleMonster monster in Player.monsterSlots)
        {
            if (monster != null)
                monster.RefreshForTurn();
        }

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        battleUI.Refresh();

        Message(
            "Your turn! SP: " +
            Player.currentSP + "/" + maximumSP
        );
    }

    // AI TURN

    private void RunAITurn()
    {
        Opponent.turnsStarted++;

        Opponent.currentSP = Mathf.Min(
            maximumSP,
            Opponent.currentSP + spPerTurn
        );

        Opponent.DrawCard();

        foreach (BattleMonster monster in Opponent.monsterSlots)
        {
            if (monster != null)
                monster.RefreshForTurn();
        }

        // SUMMON FIRST AFFORDABLE MONSTER

        int emptySlot =
            Opponent.FirstEmptyMonsterSlot();

        if (emptySlot >= 0)
        {
            for (int i = 0; i < Opponent.hand.Count; i++)
            {
                BattleCard card = Opponent.hand[i];

                if (card == null ||
                    card.type != BattleCardType.Monster ||
                    card.monster == null)
                    continue;

                int cost = Mathf.Max(
                    0, card.monster.summonCost
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

        // AI ATTACK PHASE

        for (int i = 0;
             i < Opponent.monsterSlots.Length;
             i++)
        {
            BattleMonster attacker =
                Opponent.monsterSlots[i];

            if (attacker == null ||
                !attacker.CanAttack())
                continue;

            int targetSlot = -1;

            for (int j = 0;
                 j < Player.monsterSlots.Length;
                 j++)
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

                int damage =
                    CalculateMonsterDamage(attacker, defender);

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
                    "AI dealt " +
                    damage + " direct damage!"
                );
            }

            if (CheckVictory())
                break;
        }

        // AI DISCARD EXCESS CARDS

        while (Opponent.hand.Count > maximumHandSize)
        {
            int lastIndex =
                Opponent.hand.Count - 1;

            BattleCard discarded =
                Opponent.hand[lastIndex];

            Opponent.hand.RemoveAt(lastIndex);
            Opponent.graveyard.Add(discarded);
        }

        battleUI.Refresh();
    }

    // VICTORY CHECKS

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
        if (graveyardUI != null)
            graveyardUI.Close();

        GameOver = true;
        IsDiscarding = false;

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        Message("BATTLE OVER! " + result);
    }

    private void Message(string message)
    {
        if (battleUI != null)
            battleUI.ShowMessage(message);
        else
            Debug.Log(message);
    }
}
