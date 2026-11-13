
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellCardView : MonoBehaviour
{
    [Header("Spell Information")]
    [SerializeField] private TMP_Text spellNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text requiredIntText;

    [Header("Optional Spell Information")]
    [SerializeField] private TMP_Text effectTypeText;
    [SerializeField] private TMP_Text effectStrengthText;

    [Header("Spell Artwork")]
    [SerializeField] private Image spellArtworkImage;

    [Header("Default Artwork")]
    [SerializeField] private Sprite defaultSpellArtwork;

    private SpellData currentSpell;

    // DISPLAY A SPELL

    public void DisplaySpell(SpellData spell)
    {
        if (spell == null)
        {
            Debug.LogWarning(
                "SpellCardView: Cannot display a null spell.",
                this
            );

            ClearCard();
            return;
        }

        currentSpell = spell;

        if (spellNameText != null)
            spellNameText.text = spell.spellName;

        if (descriptionText != null)
            descriptionText.text = spell.description;

        if (requiredIntText != null)
        {
            requiredIntText.text =
                "INT Required: " + spell.requiredInt;
        }

        if (effectTypeText != null)
        {
            effectTypeText.text =
                spell.GetEffectTypeText();
        }

        if (effectStrengthText != null)
        {
            effectStrengthText.text =
                "Power: " + spell.effectStrength;
        }

        if (spellArtworkImage != null)
        {
            Sprite artwork = spell.spellArtwork != null
                ? spell.spellArtwork
                : defaultSpellArtwork;

            spellArtworkImage.sprite = artwork;
            spellArtworkImage.enabled = artwork != null;
            spellArtworkImage.preserveAspect = true;
        }
    }

    // GET CURRENT SPELL

    public SpellData GetCurrentSpell()
    {
        return currentSpell;
    }

    // CLEAR THE CARD

    public void ClearCard()
    {
        currentSpell = null;

        if (spellNameText != null)
            spellNameText.text = "";

        if (descriptionText != null)
            descriptionText.text = "";

        if (requiredIntText != null)
            requiredIntText.text = "";

        if (effectTypeText != null)
            effectTypeText.text = "";

        if (effectStrengthText != null)
            effectStrengthText.text = "";

        if (spellArtworkImage != null)
        {
            spellArtworkImage.sprite = defaultSpellArtwork;
            spellArtworkImage.enabled =
                defaultSpellArtwork != null;
        }
    }
}
