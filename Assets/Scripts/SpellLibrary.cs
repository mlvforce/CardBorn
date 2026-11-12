
using System;
using System.Collections.Generic;
using UnityEngine;

public class SpellLibrary : MonoBehaviour
{
    [Header("Spell Definitions")]

    [Tooltip(
        "Add spells here. Each spell needs a unique ID."
    )]
    [SerializeField]
    private List<SpellData> spells =
        new List<SpellData>();

    // GET ALL SPELLS

    public IReadOnlyList<SpellData> GetAllSpells()
    {
        return spells;
    }

    // FIND SPELL BY ID

    public SpellData GetSpellById(string spellId)
    {
        if (string.IsNullOrWhiteSpace(spellId))
            return null;

        foreach (SpellData spell in spells)
        {
            if (spell == null)
                continue;

            if (string.Equals(
                spell.spellId,
                spellId,
                StringComparison.OrdinalIgnoreCase))
            {
                return spell;
            }
        }

        return null;
    }

    
    // FIND SPELL BY NAME

    public SpellData GetSpellByName(string spellName)
    {
        if (string.IsNullOrWhiteSpace(spellName))
            return null;

        foreach (SpellData spell in spells)
        {
            if (spell == null)
                continue;

            if (string.Equals(
                spell.spellName,
                spellName,
                StringComparison.OrdinalIgnoreCase))
            {
                return spell;
            }
        }

        return null;
    }

    // INT REQUIREMENT CHECK

    public bool CanCastSpell(
        string spellId,
        int highestFriendlyMonsterInt)
    {
        SpellData spell = GetSpellById(spellId);

        if (spell == null)
            return false;

        return spell.MeetsIntRequirement(
            highestFriendlyMonsterInt
        );
    }

    // CHECK DATA VALIDITY

    public bool ValidateSpellLibrary()
    {
        bool valid = true;

        HashSet<string> usedIds =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

        for (int i = 0; i < spells.Count; i++)
        {
            SpellData spell = spells[i];

            if (spell == null)
            {
                Debug.LogWarning(
                    "Spell Library: Null spell at index " + i,
                    this
                );

                valid = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(spell.spellId))
            {
                Debug.LogWarning(
                    "Spell Library: Spell " + i +
                    " has no Spell ID.",
                    this
                );

                valid = false;
                continue;
            }

            string id = spell.spellId.Trim();

            if (!usedIds.Add(id))
            {
                Debug.LogError(
                    "Spell Library: Duplicate Spell ID: " +
                    id,
                    this
                );

                valid = false;
            }

            if (string.IsNullOrWhiteSpace(spell.spellName))
            {
                Debug.LogWarning(
                    "Spell Library: " + id +
                    " has no display name.",
                    this
                );

                valid = false;
            }

            if (spell.requiredInt < 0 ||
                spell.effectStrength < 0)
            {
                Debug.LogWarning(
                    "Spell Library: " + id +
                    " has invalid negative values.",
                    this
                );

                valid = false;
            }
        }

        return valid;
    }

    // STARTUP
    

    private void Awake()
    {
        if (ValidateSpellLibrary())
        {
            Debug.Log(
                "Spell Library loaded successfully! " +
                spells.Count + " spells available.",
                this
            );
        }
        else
        {
            Debug.LogWarning(
                "Spell Library contains configuration issues.",
                this
            );
        }
    }
}
