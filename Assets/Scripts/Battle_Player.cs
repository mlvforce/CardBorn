
using System;
using System.Collections.Generic;
using UnityEngine;

public enum BattleCardType
{
    Monster,
    Spell
}

[Serializable]
public class BattleCard
{
    public BattleCardType type;
    public string id;
    public MonsterData monster;
    public SpellData spell;

    public string Name
    {
        get
        {
            if (type == BattleCardType.Monster)
                return monster != null ? monster.monsterName : id;

            return spell != null ? spell.spellName : id;
        }
    }
}

public class BattlePlayer
{
    public string playerName;
    public int currentHP;
    public int currentSP;
    public int turnsStarted;

    public List<BattleCard> deck = new List<BattleCard>();
    public List<BattleCard> hand = new List<BattleCard>();
    public List<BattleCard> graveyard = new List<BattleCard>();

    public BattleMonster[] monsterSlots = new BattleMonster[5];

    public BattlePlayer(
        string name,
        List<BattleCard> cards,
        int startingHP,
        int startingSP)
    {
        playerName = name;
        currentHP = startingHP;
        currentSP = startingSP;

        deck = new List<BattleCard>(cards);
        Shuffle();
    }

    public void Shuffle()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            BattleCard temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
    }

    public bool DrawCard()
    {
        if (deck.Count == 0)
            return false;

        BattleCard card = deck[0];
        deck.RemoveAt(0);
        hand.Add(card);

        return true;
    }

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (!DrawCard())
                break;
        }
    }

    public int FirstEmptyMonsterSlot()
    {
        for (int i = 0; i < monsterSlots.Length; i++)
        {
            if (monsterSlots[i] == null)
                return i;
        }

        return -1;
    }

    public bool HasMonsters()
    {
        foreach (BattleMonster monster in monsterSlots)
        {
            if (monster != null && monster.currentHP > 0)
                return true;
        }

        return false;
    }

    public int HighestMonsterINT()
    {
        int highest = -1;

        foreach (BattleMonster monster in monsterSlots)
        {
            if (monster != null && monster.currentHP > 0)
                highest = Mathf.Max(
                    highest,
                    monster.data.intelligence
                );
        }

        return highest;
    }
}
