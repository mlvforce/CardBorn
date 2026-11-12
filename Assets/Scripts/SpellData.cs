
using System;
using UnityEngine;

// What happens when the spell is used.
public enum SpellEffectType
{
    Damage,
    Heal,
    Shield,
    ModifyAttack,
    DrawCards,
    ReturnMonsterToHand
}

// What the player can select.
public enum SpellTargetType
{
    None,
    FriendlyMonster,
    EnemyMonster,
    AnyMonster
}

// Spell data stored in the Spell Library.
[Serializable]
public class SpellData
{
    [Header("Spell Identity")]

    [Tooltip("Unique permanent ID, e.g. fireball.")]
    public string spellId;

    public string spellName;

    [TextArea(3, 8)]
    public string description;

    [Header("Spell Requirements")]

    [Min(0)]
    [Tooltip(
        "A living friendly monster on the field must " +
        "have at least this much INT."
    )]
    public int requiredInt = 0;

    [Header("Spell Effect")]

    public SpellEffectType effectType =
        SpellEffectType.Damage;

    public SpellTargetType targetType =
        SpellTargetType.EnemyMonster;

    [Min(0)]
    [Tooltip(
        "Damage, healing, shield strength, " +
        "cards drawn, or other numerical effect."
    )]
    public int effectStrength = 0;

    [Min(1)]
    [Tooltip(
        "How many turns an ongoing effect lasts. " +
        "The battle system will use this later."
    )]
    public int durationTurns = 1;

    [Header("Card Artwork")]

    [Tooltip("Image displayed on the spell card.")]
    public Sprite spellArtwork;

    [Header("Deck Rules")]

    [Range(1, 3)]
    public int maximumCopies = 3;

    // USEFUL HELPER METHODS

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

            default:
                return "Unknown";
        }
    }
}
