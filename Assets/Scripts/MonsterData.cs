using UnityEngine;

[System.Serializable]
public class MonsterData
{
    
    // BASIC MONSTER INFORMATION
    

    public string monsterName = "New Monster";


    // APPEARANCE
   

    // These store which appearance option
    // the player selected.

    public int headIndex = 0;
    public int bodyIndex = 0;
    public int legsIndex = 0;


    // STATS
    

    // Every monster has a minimum of 1
    // in every stat.

    public int health = 1;
    public int defence = 1;
    public int attack = 1;
    public int intelligence = 1;


    // SUMMON COST
    

    public int summonCost = 0;


   
    // MOVES
    

   

    public string moveOne = "";
    public string moveTwo = "";


    
    // HELPER VALUES
    

    public int GetTotalStats()
    {
        return health + defence + attack + intelligence;
    }
}