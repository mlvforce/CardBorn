
using System;
using System.Collections.Generic;

public static class BattleTargetSelector
{
   
    public static List<T> GetBattlefieldTargets<T>(
        IEnumerable<T> monsters,
        int currentPlayerId,
        Func<T, int> getOwnerId,
        Func<T, bool> isAlive,
        bool targetFriendly,
        bool targetEnemy)
    {
        List<T> results = new List<T>();

        if (monsters == null)
            return results;

        foreach (T monster in monsters)
        {
            if (monster == null || !isAlive(monster))
                continue;

            bool friendly =
                getOwnerId(monster) == currentPlayerId;

            if ((friendly && targetFriendly) ||
                (!friendly && targetEnemy))
            {
                results.Add(monster);
            }
        }

        return results;
    }

    // ALL MONSTERS: both players.
    public static List<T> AllMonsters<T>(
        IEnumerable<T> monsters,
        int playerId,
        Func<T, int> getOwnerId,
        Func<T, bool> isAlive)
    {
        return GetBattlefieldTargets(
            monsters, playerId,
            getOwnerId, isAlive,
            true, true
        );
    }

    // ALL ENEMY MONSTERS: opponents only.
    public static List<T> AllEnemyMonsters<T>(
        IEnumerable<T> monsters,
        int playerId,
        Func<T, int> getOwnerId,
        Func<T, bool> isAlive)
    {
        return GetBattlefieldTargets(
            monsters, playerId,
            getOwnerId, isAlive,
            false, true
        );
    }

    // ALL FRIENDLY MONSTERS: caster's side only.
    public static List<T> AllFriendlyMonsters<T>(
        IEnumerable<T> monsters,
        int playerId,
        Func<T, int> getOwnerId,
        Func<T, bool> isAlive)
    {
        return GetBattlefieldTargets(
            monsters, playerId,
            getOwnerId, isAlive,
            true, false
        );
    }
}
