
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class MonsterCollectionData
{
    public List<MonsterData> monsters =
        new List<MonsterData>();
}

public static class MonsterSaveSystem
{
    private const string FileName = "monsters.json";

    public static string SavePath
    {
        get
        {
            return Path.Combine(
                Application.persistentDataPath,
                FileName
            );
        }
    }

    // LOAD ALL MONSTERS

    public static List<MonsterData> LoadMonsters()
    {
        if (!File.Exists(SavePath))
        {
            return new List<MonsterData>();
        }

        try
        {
            string json = File.ReadAllText(SavePath);

            MonsterCollectionData collection =
                JsonUtility.FromJson<MonsterCollectionData>(
                    json
                );

            if (collection == null ||
                collection.monsters == null)
            {
                Debug.LogWarning(
                    "Monster save file contains no valid collection."
                );

                return new List<MonsterData>();
            }

            return collection.monsters;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "Failed to load monsters: " +
                exception.Message
            );

            return new List<MonsterData>();
        }
    }

    // SAVE A MONSTER
    

    public static bool SaveMonster(MonsterData monster)
    {
        if (monster == null)
        {
            Debug.LogError("Cannot save a null monster.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(monster.monsterName))
        {
            Debug.LogWarning(
                "Monster must have a name."
            );

            return false;
        }

        if (!HasValidStats(monster))
        {
            Debug.LogWarning(
                "Monster has invalid stat allocation."
            );

            return false;
        }

        try
        {
            List<MonsterData> monsters = LoadMonsters();

            monster.EnsureId();

            foreach (MonsterData existing in monsters)
            {
                if (existing != null &&
                    existing.monsterId == monster.monsterId)
                {
                    Debug.LogWarning(
                        "Monster ID already exists."
                    );

                    return false;
                }
            }

            monsters.Add(monster);

            MonsterCollectionData collection =
                new MonsterCollectionData();

            collection.monsters = monsters;

            string json = JsonUtility.ToJson(
                collection,
                true
            );

            string directory = Path.GetDirectoryName(
                SavePath
            );

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string temporaryPath = SavePath + ".tmp";

            File.WriteAllText(temporaryPath, json);

            File.Copy(temporaryPath, SavePath, true);
            File.Delete(temporaryPath);

            Debug.Log(
                "Monster saved successfully: " +
                monster.monsterName
            );

            Debug.Log("Save path: " + SavePath);

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "Failed to save monster: " +
                exception.Message
            );

            return false;
        }
    }

    // VALIDATE STATS

    private static bool HasValidStats(MonsterData monster)
    {
        if (monster.health < 1 ||
            monster.defence < 1 ||
            monster.attack < 1 ||
            monster.intelligence < 1)
        {
            return false;
        }

        int allocated = monster.GetAllocatedPoints();

        
        if (allocated < 0 || allocated > 100)
        {
            return false;
        }

        int expectedCost =
            Mathf.CeilToInt(allocated / 5f);

        return monster.summonCost == expectedCost;
    }
}
