
using UnityEngine;

public class BattleTimedEffect
{
    public SpellEffectType effectType;

    public BattleMonster targetMonster;
    public BattlePlayer affectedPlayer;

    public int strength;
    public int remainingTurns;

    public BattleTimedEffect(
        SpellEffectType type,
        BattleMonster target,
        BattlePlayer owner,
        int effectStrength,
        int duration)
    {
        effectType = type;
        targetMonster = target;
        affectedPlayer = owner;

        strength = Mathf.Max(0, effectStrength);
        remainingTurns = Mathf.Max(1, duration);
    }

    public bool IsExpired
    {
        get { return remainingTurns <= 0; }
    }

    public void EndOwnerTurn()
    {
        if (remainingTurns > 0)
            remainingTurns--;
    }
}
