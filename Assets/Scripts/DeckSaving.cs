
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class DeckData
{
    public string deckId = "";
    public string deckName = "New Deck";

    // Each ID refers to a monster in monsters.json.
    public List<string> monsterIds = new List<string>();
}

[Serializable]
public class DeckCollectionData
{
    public List<DeckData> decks = new List<DeckData>();
}

public static class DeckSaveSystem
{
    private const string FileName = "decks.json";

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

    // LOAD SAVED DECKS

    public static List<DeckData> LoadDecks()
    {
        if (!File.Exists(SavePath))
        {
            return new List<DeckData>();
        }

        try
        {
            string json = File.ReadAllText(SavePath);

            DeckCollectionData data =
                JsonUtility.FromJson<DeckCollectionData>(json);

            if (data == null || data.decks == null)
            {
                Debug.LogError(
                    "Deck save file has an invalid structure."
                );
                return new List<DeckData>();
            }

            return data.decks;
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Failed to load decks: " + e.Message
            );

            return new List<DeckData>();
        }
    }

    // SAVE OR UPDATE A DECK

    public static bool SaveDeck(DeckData deck)
    {
        if (deck == null)
        {
            Debug.LogError("Cannot save a null deck.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(deck.deckName))
        {
            Debug.LogWarning("Deck name cannot be empty.");
            return false;
        }

        if (deck.monsterIds == null ||
            deck.monsterIds.Count > 34)
        {
            Debug.LogWarning("Invalid deck size.");
            return false;
        }

        // Avoid overwriting an unreadable existing file.
        if (File.Exists(SavePath))
        {
            try
            {
                string existingJson = File.ReadAllText(SavePath);
                DeckCollectionData existingData =
                    JsonUtility.FromJson<DeckCollectionData>(
                        existingJson
                    );

                if (existingData == null ||
                    existingData.decks == null)
                {
                    Debug.LogError(
                        "Existing deck file cannot be loaded. " +
                        "Saving cancelled to protect existing data."
                    );
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(
                    "Existing deck file cannot be read: " +
                    e.Message
                );
                return false;
            }
        }

        try
        {
            List<DeckData> decks = LoadDecks();

            if (string.IsNullOrEmpty(deck.deckId))
            {
                deck.deckId = Guid.NewGuid().ToString("N");
            }

            int existingIndex = decks.FindIndex(
                d => d != null && d.deckId == deck.deckId
            );

            if (existingIndex >= 0)
            {
                decks[existingIndex] = deck;
            }
            else
            {
                decks.Add(deck);
            }

            DeckCollectionData collection =
                new DeckCollectionData();

            collection.decks = decks;

            string json = JsonUtility.ToJson(
                collection,
                true
            );

            string directory =
                Path.GetDirectoryName(SavePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string temporaryPath = SavePath + ".tmp";

            File.WriteAllText(temporaryPath, json);

           
            File.Copy(temporaryPath, SavePath, true);
            File.Delete(temporaryPath);

            Debug.Log(
                "Deck saved: " + deck.deckName
            );

            Debug.Log(
                "Deck save path: " + SavePath
            );

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Failed to save deck: " + e.Message
            );

            return false;
        }
    }
}
