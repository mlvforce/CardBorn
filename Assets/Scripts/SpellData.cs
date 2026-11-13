
using System;
using UnityEngine;

// WHAT HAPPENS WHEN A SPELL IS USED

public enum SpellEffectType
{
    Damage,
    Heal,
    Shield,
    ModifyAttack,
    DrawCards,
    ReturnMonsterToHand,

    // New effects.
    DamageOverTime
}

// WHAT THE SPELL CAN TARGET

public enum SpellTargetType
{
    None,
    FriendlyMonster,
    EnemyMonster,
    AnyMonster,

    AllMonsters,
    AllEnemyMonsters,
    AllFriendlyMonsters,

    FriendlyGraveyardMonster
}

// SPELL INFORMATION

[Serializable]
public class SpellData
{
    [Header("Spell Identity")]

    [Tooltip("Unique permanent ID, e.g. fireball.")]
    public string spellId;

    public string spellName;

    [TextArea(3, 8)]
    public string description;

    // SPELL REQUIREMENTS

    [Header("Spell Requirements")]

    [Min(0)]
    [Tooltip(
        "A living friendly monster on the battlefield " +
        "must have at least this much INT to cast the spell."
    )]
    public int requiredInt = 0;

    // SPELL EFFECT

    [Header("Spell Effect")]

    public SpellEffectType effectType =
        SpellEffectType.Damage;

    public SpellTargetType targetType =
        SpellTargetType.EnemyMonster;

    [Min(0)]
    [Tooltip(
        "Damage, healing, shield strength, " +
        "cards drawn or other numerical effect."
    )]
    public int effectStrength = 0;

    [Min(0)]
    [Tooltip(
        "For instant effects, use 0. " +
        "For effects lasting multiple turns, " +
        "enter the number of turns."
    )]
    public int durationTurns = 1;

    // CARD ARTWORK

    [Header("Card Artwork")]

    [Tooltip("Image displayed on the spell card.")]
    public Sprite spellArtwork;

    // DECK RULES

    [Header("Deck Rules")]

    [Range(1, 3)]
    public int maximumCopies = 3;

    
    // HELPER METHODS

    public bool HasValidId()
    {
        return !string.IsNullOrWhiteSpace(spellId);
    }

    public bool MeetsIntRequirement(int casterInt)
    {
        return casterInt >= requiredInt;
    }

    public string GetRequirementText()
    {
        return "Required INT: " + requiredInt;
    }

    public bool IsMultiTarget()
    {
        return targetType == SpellTargetType.AllMonsters ||
               targetType == SpellTargetType.AllEnemyMonsters ||
               targetType == SpellTargetType.AllFriendlyMonsters;
    }

    public bool TargetsGraveyard()
    {
        return targetType ==
            SpellTargetType.FriendlyGraveyardMonster;
    }

    public bool IsOngoingEffect()
    {
        return effectType == SpellEffectType.DamageOverTime ||
               effectType == SpellEffectType.ModifyAttack ||
               effectType == SpellEffectType.Shield;
    }

    // EFFECT TYPE DESCRIPTION

    public string GetEffectTypeText()
    {
        switch (effectType)
        {
            case SpellEffectType.Damage:
                return "Damage";

            case SpellEffectType.Heal:
                return "Healing";

            case SpellEffectType.Shield:
                return "Shield";

            case SpellEffectType.ModifyAttack:
                return "Attack Modifier";

            case SpellEffectType.DrawCards:
                return "Card Draw";

            case SpellEffectType.ReturnMonsterToHand:
                return "Return Monster";

            case SpellEffectType.DamageOverTime:
                return "Damage Over Time";

            default:
                return "Unknown";
        }
    }

    // TARGET TYPE DESCRIPTION

    public string GetTargetTypeText()
    {
        switch (targetType)
        {
            case SpellTargetType.None:
                return "No Target";

            case SpellTargetType.FriendlyMonster:
                return "Friendly Monster";

            case SpellTargetType.EnemyMonster:
                return "Enemy Monster";

            case SpellTargetType.AnyMonster:
                return "Any Monster";

            case SpellTargetType.AllMonsters:
                return "All Monsters";

            case SpellTargetType.AllEnemyMonsters:
                return "All Enemy Monsters";

            case SpellTargetType.AllFriendlyMonsters:
                return "All Friendly Monsters";

            case SpellTargetType.FriendlyGraveyardMonster:
                return "Friendly Graveyard Monster";

            default:
                return "Unknown Target";
        }
    }
}
