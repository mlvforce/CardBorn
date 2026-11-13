
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DeckSlotSwitcher : MonoBehaviour
{
    private const int SlotCount = 4;

    [Header("Deck Builder")]
    [SerializeField]
    private DeckBuilderController deckBuilder;

    [Header("Deck Selection Buttons")]
    [SerializeField] private Button deckSlot1;
    [SerializeField] private Button deckSlot2;
    [SerializeField] private Button deckSlot3;
    [SerializeField] private Button deckSlot4;

    [Header("Button Colours")]
    [SerializeField]
    private Color normalColour =
        new Color(0.4f, 0.12f, 0.5f);

    [SerializeField]
    private Color selectedColour =
        new Color(0.7f, 0.1f, 0.9f);

    [Serializable]
    private class SavedDeckSlots
    {
        public int activeSlot = 0;
        public List<DeckData> decks =
            new List<DeckData>();
    }

    private SavedDeckSlots savedData;
    private Button[] buttons;
    private bool initialized;

    private string SavePath
    {
        get
        {
            return Path.Combine(
                Application.persistentDataPath,
                "deck_slots.json"
            );
        }
    }

    // STARTUP

    private IEnumerator Start()
    {
        buttons = new Button[]
        {
            deckSlot1,
            deckSlot2,
            deckSlot3,
            deckSlot4
        };

        if (deckBuilder == null)
        {
            Debug.LogError(
                "DeckSlotSwitcher: Deck Builder is missing!"
            );
            yield break;
        }

        
        yield return null;

        LoadSavedSlots();

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null)
                continue;

            int slotIndex = i;

            buttons[i].onClick.AddListener(
                () => SwitchToSlot(slotIndex)
            );
        }

        savedData.activeSlot = Mathf.Clamp(
            savedData.activeSlot,
            0,
            SlotCount - 1
        );

        initialized = true;

        deckBuilder.ImportDeckSnapshot(
            savedData.decks[savedData.activeSlot]
        );

        UpdateButtonColours();

        Debug.Log(
            "Loaded deck slot " +
            (savedData.activeSlot + 1)
        );
    }

    
    // CREATE EMPTY DECK

    private DeckData CreateEmptyDeck(int index)
    {
        DeckData deck = new DeckData();

        deck.deckId = "deck-slot-" + (index + 1);
        deck.deckName = "New Deck " + (index + 1);
        deck.monsterIds = new List<string>();

        return deck;
    }

    private DeckData CopyDeck(DeckData source, int index)
    {
        DeckData copy = CreateEmptyDeck(index);

        if (source == null)
            return copy;

        if (!string.IsNullOrWhiteSpace(source.deckName))
            copy.deckName = source.deckName;

        if (source.monsterIds != null)
        {
            copy.monsterIds =
                new List<string>(source.monsterIds);
        }

        return copy;
    }

    // LOAD ALL DECK SLOTS

    private void LoadSavedSlots()
    {
        savedData = null;

        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);

                savedData =
                    JsonUtility.FromJson<SavedDeckSlots>(json);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "Could not load deck slots: " +
                    exception.Message
                );
            }
        }

        if (savedData == null)
        {
            savedData = new SavedDeckSlots();

            
            DeckData originalDeck =
                deckBuilder.ExportDeckSnapshot();

            for (int i = 0; i < SlotCount; i++)
            {
                savedData.decks.Add(
                    i == 0
                        ? CopyDeck(originalDeck, i)
                        : CreateEmptyDeck(i)
                );
            }

            savedData.activeSlot = 0;
            WriteSavedSlots();

            Debug.Log(
                "Created four deck slots. " +
                "Imported the existing deck into Slot 1."
            );

            return;
        }

        if (savedData.decks == null)
        {
            savedData.decks = new List<DeckData>();
        }

        
        while (savedData.decks.Count < SlotCount)
        {
            savedData.decks.Add(
                CreateEmptyDeck(savedData.decks.Count)
            );
        }

        for (int i = 0; i < SlotCount; i++)
        {
            savedData.decks[i] =
                CopyDeck(savedData.decks[i], i);
        }
    }

    // SWITCH DECK

    public void SwitchToSlot(int slotIndex)
    {
        if (!initialized || savedData == null)
            return;

        if (slotIndex < 0 || slotIndex >= SlotCount)
            return;

        if (slotIndex == savedData.activeSlot)
            return;

        int oldSlot = savedData.activeSlot;

        
        savedData.decks[oldSlot] = CopyDeck(
            deckBuilder.ExportDeckSnapshot(),
            oldSlot
        );

        savedData.activeSlot = slotIndex;

        deckBuilder.ImportDeckSnapshot(
            savedData.decks[slotIndex]
        );

        UpdateButtonColours();
        WriteSavedSlots();

        Debug.Log(
            "Switched to Deck " + (slotIndex + 1)
        );
    }

    // SAVE CURRENT DECK

    public bool SaveActiveDeck()
    {
        if (!initialized || savedData == null)
            return false;

        int slotIndex = savedData.activeSlot;

        savedData.decks[slotIndex] = CopyDeck(
            deckBuilder.ExportDeckSnapshot(),
            slotIndex
        );

        bool success = WriteSavedSlots();

        if (success)
        {
            Debug.Log(
                "Saved Deck Slot " +
                (slotIndex + 1) +
                " to: " + SavePath
            );
        }

        return success;
    }

    // WRITE JSON FILE

    private bool WriteSavedSlots()
    {
        try
        {
            string json =
                JsonUtility.ToJson(savedData, true);

            string directory =
                Path.GetDirectoryName(SavePath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string temporaryPath = SavePath + ".tmp";

            File.WriteAllText(temporaryPath, json);

            
            if (File.Exists(SavePath))
            {
                string backupPath = SavePath + ".bak";

                File.Copy(
                    SavePath,
                    backupPath,
                    true
                );

                File.Delete(SavePath);
            }

            File.Move(temporaryPath, SavePath);

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "Failed to save deck slots: " +
                exception.Message
            );

            return false;
        }
    }

    // BUTTON HIGHLIGHTING

    private void UpdateButtonColours()
    {
        if (buttons == null || savedData == null)
            return;

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];

            if (button == null)
                continue;

            Color colour =
                i == savedData.activeSlot
                    ? selectedColour
                    : normalColour;

            
            ColorBlock colours = button.colors;

            colours.normalColor = Color.white;
            colours.selectedColor = Color.white;
            colours.highlightedColor = Color.white;
            colours.pressedColor =
                new Color(0.85f, 0.85f, 0.85f, 1f);

            button.colors = colours;

            if (button.image != null)
            {
                button.image.color = colour;
            }
        }
    }

    // SAVE ON EXIT
    

    private void OnApplicationPause(bool paused)
    {
        if (paused && initialized)
            SaveActiveDeck();
    }

    private void OnApplicationQuit()
    {
        if (initialized)
            SaveActiveDeck();
    }
}
