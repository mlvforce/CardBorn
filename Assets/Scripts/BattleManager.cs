
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

    private readonly List<BattleTimedEffect> timedEffects =
        new List<BattleTimedEffect>();

    // SELECTED SPELL STATE

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

    // DIRECT ATTACK STATE

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

        timedEffects.Clear();

        Player.DrawCards(openingHandSize);
        Opponent.DrawCards(openingHandSize);

        Player.turnsStarted = 1;

        battleUI.Setup();
        battleUI.Refresh();

        Message("Battle started! Your turn.");
    }

    // SELECT CARD FROM HAND

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

    // SELECT A SPELL

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

        // AREA-OF-EFFECT SPELLS

        if (spell.effectType == SpellEffectType.ModifyAttack ||
            IsArrowRain(spell))
        {
            Message(
                spell.spellName +
                " selected. Click an enemy monster slot " +
                "to affect the whole enemy field."
            );
            return;
        }

        // SINGLE-TARGET SPELLS

        if (spell.effectType == SpellEffectType.Damage ||
            spell.effectType == SpellEffectType.Heal ||
            spell.effectType == SpellEffectType.Shield)
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

    // SPELL VALIDATION

    private bool CanMeetSpellRequirement(SpellData spell)
    {
        return spell != null &&
               Player != null &&
               Player.HasMonsters() &&
               Player.HighestMonsterINT() >= spell.requiredInt;
    }

    // Identify Arrow Rain using its existing ID.
    // This avoids changing SpellEffectType or saved data.

    private bool IsArrowRain(SpellData spell)
    {
        return spell != null &&
               !string.IsNullOrWhiteSpace(spell.spellId) &&
               string.Equals(
                   spell.spellId.Trim(),
                   "arrow rain",
                   System.StringComparison.OrdinalIgnoreCase
               );
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
            Message("Soul Reborn: INT requirement not met.");
            return false;
        }

        if (Player.hand.Count > maximumHandSize)
        {
            Message("Too many cards in hand to cast Soul Reborn.");
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

        Player.graveyard.RemoveAt(graveIndex);
        Player.hand.RemoveAt(selectedHandIndex);

        Player.hand.Add(recovered);
        Player.graveyard.Add(spellCard);

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        if (graveyardUI != null)
            graveyardUI.Close();

        battleUI.Refresh();

        Message(
            "Soul Reborn returned " +
            recovered.Name + " to your hand."
        );

        return true;
    }

    // CAST SPELL ON A FIELD SLOT

    public bool TryCastSelectedSpellOn(
        bool targetIsFriendly,
        int targetSlot)
    {
        if (!HasPendingSpell)
            return false;

        SpellData spell =
            Player.hand[selectedHandIndex].spell;

        if (spell.effectType ==
            SpellEffectType.ReturnMonsterToHand)
            return true;

        if (!CanMeetSpellRequirement(spell))
        {
            Message("Cannot cast. INT requirement not met.");
            return true;
        }

        // NEW: ARROW RAIN

        if (IsArrowRain(spell))
        {
            if (targetIsFriendly)
            {
                Message("Arrow Rain targets the enemy field.");
                return true;
            }

            if (targetSlot < 0 ||
                targetSlot >= Opponent.monsterSlots.Length)
                return true;

            if (!Opponent.HasMonsters())
            {
                Message(
                    "Arrow Rain needs at least one " +
                    "living enemy monster."
                );
                return true;
            }

            // First wave happens immediately.
            int affected = DamageAllMonsters(
                Opponent,
                spell.effectStrength,
                spell.spellName
            );

            // Second wave is scheduled for the
            // target owner's next turn.
            if (!GameOver &&
                spell.durationTurns > 1)
            {
                BattleTimedEffect rain =
                    new BattleTimedEffect(
                        SpellEffectType.Damage,
                        null,
                        Opponent,
                        spell.effectStrength,
                        1
                    );

                timedEffects.Add(rain);
            }

            ConsumeSelectedSpell();

            Message(
                "Arrow Rain hit " +
                affected + " enemy monster(s)!"
            );

            CheckVictory();
            battleUI.Refresh();
            return true;
        }

        // WEAKENING FOG

        if (spell.effectType ==
            SpellEffectType.ModifyAttack)
        {
            if (targetIsFriendly)
            {
                Message(
                    "Weakening Fog must target the enemy field."
                );
                return true;
            }

            if (targetSlot < 0 ||
                targetSlot >= Opponent.monsterSlots.Length)
                return true;

            if (!Opponent.HasMonsters())
            {
                Message(
                    "Weakening Fog needs living enemy monsters."
                );
                return true;
            }

            int affected = ApplyWeakeningFog(
                Opponent,
                spell.effectStrength,
                spell.durationTurns
            );

            if (affected <= 0)
                return true;

            ConsumeSelectedSpell();

            Message(
                "Weakening Fog reduced ATK on " +
                affected + " enemy monster(s)."
            );

            battleUI.Refresh();
            return true;
        }

        // SINGLE-TARGET SPELLS

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
                        0,
                        spell.effectStrength
                    );

                    int oldHP = target.currentHP;
                    int oldShield = target.shield;

                    // True damage bypasses DEF.
                    // Shield still absorbs damage.
                    target.TakeDamage(damage);

                    Message(
                        spell.spellName + " hit " +
                        target.data.monsterName + ". " +
                        "Shield absorbed " +
                        (oldShield - target.shield) +
                        ", HP lost " +
                        (oldHP - target.currentHP) + "."
                    );

                    if (target.currentHP <= 0)
                        DefeatMonster(targetOwner, targetSlot);

                    break;
                }

            case SpellEffectType.Heal:
                {
                    int oldHP = target.currentHP;

                    target.Heal(spell.effectStrength);

                    Message(
                        spell.spellName + " restored " +
                        (target.currentHP - oldHP) +
                        " HP to " +
                        target.data.monsterName + "."
                    );

                    break;
                }

            case SpellEffectType.Shield:
                {
                    ApplyArcaneShield(
                        target,
                        spell.effectStrength,
                        spell.durationTurns
                    );

                    Message(
                        target.data.monsterName +
                        " received " +
                        target.shield + " Shield HP."
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

        ConsumeSelectedSpell();

        CheckVictory();
        battleUI.Refresh();
        return true;
    }

    // CONSUME CAST SPELL

    private void ConsumeSelectedSpell()
    {
        if (Player == null ||
            selectedHandIndex < 0 ||
            selectedHandIndex >= Player.hand.Count)
            return;

        BattleCard usedCard =
            Player.hand[selectedHandIndex];

        Player.hand.RemoveAt(selectedHandIndex);
        Player.graveyard.Add(usedCard);

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;
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

    // ===================================
    // NEW: ARROW RAIN DAMAGE WAVES
    // ===================================

    private int DamageAllMonsters(
        BattlePlayer targetOwner,
        int strength,
        string sourceName)
    {
        if (targetOwner == null)
            return 0;

        int affected = 0;
        int damage = Mathf.Max(0, strength);

        for (int i = 0;
             i < targetOwner.monsterSlots.Length;
             i++)
        {
            BattleMonster monster =
                targetOwner.monsterSlots[i];

            if (monster == null ||
                monster.currentHP <= 0)
                continue;

            int oldHP = monster.currentHP;
            int oldShield = monster.shield;

            monster.TakeDamage(damage);
            affected++;

            Message(
                sourceName + " hit " +
                monster.data.monsterName +
                ". Shield absorbed " +
                (oldShield - monster.shield) +
                ", HP lost " +
                (oldHP - monster.currentHP) + "."
            );

            if (monster.currentHP <= 0)
                DefeatMonster(targetOwner, i);
        }

        return affected;
    }

    // Process scheduled Arrow Rain waves
    // once at the affected player's next turn.

    private void ProcessArrowRainFor(
        BattlePlayer affectedPlayer)
    {
        for (int i = timedEffects.Count - 1;
             i >= 0;
             i--)
        {
            BattleTimedEffect effect =
                timedEffects[i];

            // Only the scheduled area-damage waves.
            if (effect.effectType !=
                    SpellEffectType.Damage ||
                effect.targetMonster != null ||
                effect.affectedPlayer != affectedPlayer)
                continue;

            int affected = DamageAllMonsters(
                affectedPlayer,
                effect.strength,
                "Arrow Rain (second wave)"
            );

            effect.EndOwnerTurn();

            Message(
                "Arrow Rain's second wave hit " +
                affected + " monster(s)!"
            );

            if (effect.IsExpired)
                timedEffects.RemoveAt(i);
        }
    }

    // ===================================
    // ARCANE SHIELD
    // ===================================

    private void ApplyArcaneShield(
        BattleMonster target,
        int strength,
        int duration)
    {
        if (target == null)
            return;

        for (int i = timedEffects.Count - 1;
             i >= 0;
             i--)
        {
            BattleTimedEffect effect = timedEffects[i];

            if (effect.effectType ==
                    SpellEffectType.Shield &&
                effect.targetMonster == target)
            {
                RemoveTimedEffect(effect);
                timedEffects.RemoveAt(i);
            }
        }

        int amount = Mathf.Max(0, strength);
        target.shield = amount;

        timedEffects.Add(
            new BattleTimedEffect(
                SpellEffectType.Shield,
                target,
                target.owner,
                amount,
                duration
            )
        );
    }

    // ===================================
    // WEAKENING FOG
    // ===================================

    private int ApplyWeakeningFog(
        BattlePlayer targetOwner,
        int strength,
        int duration)
    {
        if (targetOwner == null)
            return 0;

        int affected = 0;
        int amount = Mathf.Max(0, strength);
        int turns = Mathf.Max(1, duration);

        foreach (BattleMonster monster in
                 targetOwner.monsterSlots)
        {
            if (monster == null ||
                monster.currentHP <= 0)
                continue;

            // Refresh instead of stacking.
            for (int i = timedEffects.Count - 1;
                 i >= 0;
                 i--)
            {
                BattleTimedEffect existing =
                    timedEffects[i];

                if (existing.effectType ==
                        SpellEffectType.ModifyAttack &&
                    existing.targetMonster == monster)
                {
                    RemoveTimedEffect(existing);
                    timedEffects.RemoveAt(i);
                }
            }

            monster.attackModifier -= amount;

            timedEffects.Add(
                new BattleTimedEffect(
                    SpellEffectType.ModifyAttack,
                    monster,
                    targetOwner,
                    amount,
                    turns
                )
            );

            affected++;
        }

        return affected;
    }

    // ===================================
    // TEMPORARY EFFECT EXPIRATION
    // ===================================

    private void EndTimedEffectsFor(
        BattlePlayer owner)
    {
        for (int i = timedEffects.Count - 1;
             i >= 0;
             i--)
        {
            BattleTimedEffect effect =
                timedEffects[i];

            if (effect.affectedPlayer != owner)
                continue;

            // Arrow Rain is processed separately
            // at the beginning of the next turn.
            if (effect.effectType ==
                    SpellEffectType.Damage &&
                effect.targetMonster == null)
                continue;

            if (effect.targetMonster == null ||
                effect.targetMonster.currentHP <= 0)
            {
                timedEffects.RemoveAt(i);
                continue;
            }

            effect.EndOwnerTurn();

            if (effect.IsExpired)
            {
                RemoveTimedEffect(effect);
                timedEffects.RemoveAt(i);
            }
        }
    }

    private void RemoveTimedEffect(
        BattleTimedEffect effect)
    {
        if (effect == null ||
            effect.targetMonster == null)
            return;

        switch (effect.effectType)
        {
            case SpellEffectType.Shield:
                {
                    effect.targetMonster.shield = 0;
                    break;
                }

            case SpellEffectType.ModifyAttack:
                {
                    effect.targetMonster.attackModifier +=
                        effect.strength;
                    break;
                }
        }
    }

    private void ClearEffectsFor(
        BattleMonster monster)
    {
        for (int i = timedEffects.Count - 1;
             i >= 0;
             i--)
        {
            if (timedEffects[i].targetMonster == monster)
                timedEffects.RemoveAt(i);
        }
    }

    // ===================================
    // MONSTER SLOT SELECTION
    // ===================================

    public void SelectMonsterSlot(
        bool isPlayer,
        int index)
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

        BattleMonster target =
            owner.monsterSlots[index];

        if (isPlayer)
        {
            if (selectedHandIndex >= 0 &&
                target == null)
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
                    Message(
                        "This monster cannot attack yet."
                    );
                }
            }

            battleUI.Refresh();
            return;
        }

        if (target != null &&
            selectedAttackerIndex >= 0)
        {
            AttackEnemyMonster(index);
        }
    }

    // SUMMON MONSTER

    private void SummonPlayerMonster(int slot)
    {
        if (IsDiscarding || selectedHandIndex < 0 ||
            selectedHandIndex >= Player.hand.Count)
            return;

        BattleCard card =
            Player.hand[selectedHandIndex];

        if (card.type != BattleCardType.Monster ||
            card.monster == null ||
            Player.monsterSlots[slot] != null)
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

    // RED ATTACK ARROW SUPPORT

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

        if (attacker == null ||
            !attacker.CanAttack())
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

    // DEF FORMULA

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

        return Mathf.Max(
            1,
            Mathf.CeilToInt(result)
        );
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
            "Direct attack dealt " +
            damage + " damage!"
        );

        CheckVictory();
        battleUI.Refresh();
    }

    // DEFEATED MONSTER

    private void DefeatMonster(
        BattlePlayer owner,
        int slot)
    {
        BattleMonster monster =
            owner.monsterSlots[slot];

        if (monster == null)
            return;

        ClearEffectsFor(monster);

        owner.graveyard.Add(monster.originalCard);
        owner.monsterSlots[slot] = null;

        Message(
            monster.data.monsterName +
            " was defeated!"
        );
    }

    // ===================================
    // END PLAYER TURN
    // ===================================

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

        CompleteEndTurn();
    }

    private void CompleteEndTurn()
    {
        if (GameOver)
            return;

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        EndTimedEffectsFor(Player);

        RunAITurn();

        if (!GameOver)
            StartPlayerTurn();
        else
            battleUI.Refresh();
    }

    // ===================================
    // START PLAYER TURN
    // ===================================

    private void StartPlayerTurn()
    {
        Player.turnsStarted++;

        Player.currentSP = Mathf.Min(
            maximumSP,
            Player.currentSP + spPerTurn
        );

        Player.DrawCard();

        foreach (BattleMonster monster in
                 Player.monsterSlots)
        {
            if (monster != null)
                monster.RefreshForTurn();
        }

        // Supports future AI-cast area effects.
        ProcessArrowRainFor(Player);

        if (CheckVictory())
        {
            battleUI.Refresh();
            return;
        }

        selectedHandIndex = -1;
        selectedAttackerIndex = -1;

        battleUI.Refresh();

        Message(
            "Your turn! SP: " +
            Player.currentSP + "/" + maximumSP
        );
    }

    // ===================================
    // AI TURN
    // ===================================

    private void RunAITurn()
    {
        Opponent.turnsStarted++;

        Opponent.currentSP = Mathf.Min(
            maximumSP,
            Opponent.currentSP + spPerTurn
        );

        Opponent.DrawCard();

        foreach (BattleMonster monster in
                 Opponent.monsterSlots)
        {
            if (monster != null)
                monster.RefreshForTurn();
        }

        // AI SUMMONS FIRST AFFORDABLE MONSTER

        int emptySlot =
            Opponent.FirstEmptyMonsterSlot();

        if (emptySlot >= 0)
        {
            for (int i = 0;
                 i < Opponent.hand.Count;
                 i++)
            {
                BattleCard card =
                    Opponent.hand[i];

                if (card == null ||
                    card.type != BattleCardType.Monster ||
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

                Message(
                    "AI summoned " + card.Name
                );
                break;
            }
        }

        // NEW: SECOND ARROW RAIN WAVE.
        // Occurs after summoning, before attacking.
        // Newly summoned AI monsters can be hit.

        ProcessArrowRainFor(Opponent);

        if (CheckVictory())
        {
            battleUI.Refresh();
            return;
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
                    CalculateMonsterDamage(
                        attacker,
                        defender
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
                    "AI dealt " +
                    damage + " direct damage!"
                );
            }

            if (CheckVictory())
                break;
        }

        EndTimedEffectsFor(Opponent);

        // AI HAND LIMIT

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

    // ===================================
    // WIN / LOSS
    // ===================================

    private bool CheckVictory()
    {
        if (Player == null || Opponent == null)
            return false;

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

        Message(
            "BATTLE OVER! " + result
        );
    }

    private void Message(string message)
    {
        if (battleUI != null)
            battleUI.ShowMessage(message);
        else
            Debug.Log(message);
    }
}
