
using System;
using UnityEngine;

[Serializable]
public class MonsterData
{
    // MONSTER IDENTITY

    public string monsterId = "";
    public string monsterName = "New Monster";

    // APPEARANCE

    public int headIndex = 0;
    public int bodyIndex = 0;
    public int legsIndex = 0;

    // STATS

    

    public int health = 1;
    public int defence = 1;
    public int attack = 1;
    public int intelligence = 1;

    // SUMMON COST

    public int summonCost = 0;

    // EQUIPPED EFFECTS

    
    public string moveOne = "";
    public string moveTwo = "";

    // HELPERS

    public int GetTotalStats()
    {
        return health + defence + attack + intelligence;
    }

    public int GetAllocatedPoints()
    {
        return GetTotalStats() - 4;
    }

    public void EnsureId()
    {
        if (string.IsNullOrEmpty(monsterId))
        {
            monsterId = Guid.NewGuid().ToString("N");
        }
    }
}
