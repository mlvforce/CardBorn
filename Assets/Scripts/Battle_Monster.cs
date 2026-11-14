
using UnityEngine;

public class BattleMonster
{
    public MonsterData data;
    public BattleCard originalCard;
    public BattlePlayer owner;

    public int currentHP;
    public int shield;
    public int attackModifier;

    public int summonedOnTurn;
    public bool hasAttacked;

    public BattleMonster(
        BattleCard card,
        BattlePlayer ownerPlayer)
    {
        originalCard = card;
        data = card.monster;
        owner = ownerPlayer;

        currentHP = Mathf.Max(1, data.health);
        summonedOnTurn = owner.turnsStarted;
        hasAttacked = false;
    }

    public int MaxHP => Mathf.Max(1, data.health);

    public int Attack =>
        Mathf.Max(0, data.attack + attackModifier);

    public int Defence => Mathf.Max(0, data.defence);

    public bool CanAttack()
    {
        return currentHP > 0 &&
               !hasAttacked &&
               owner.turnsStarted > summonedOnTurn;
    }

    public void TakeDamage(int amount)
    {
        amount = Mathf.Max(0, amount);

        int absorbed = Mathf.Min(shield, amount);
        shield -= absorbed;
        amount -= absorbed;

        currentHP = Mathf.Max(0, currentHP - amount);
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(
            MaxHP,
            currentHP + Mathf.Max(0, amount)
        );
    }

    public void RefreshForTurn()
    {
        hasAttacked = false;
    }
}
