
using UnityEngine;
using UnityEngine.UI;

public class BattleCardPreview : MonoBehaviour
{
    [Header("Preview Panel")]
    [SerializeField] private GameObject previewPanel;

    [Header("Large Card Views")]
    [SerializeField] private MonsterCardView monsterCardView;
    [SerializeField] private SpellCardView spellCardView;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }

        Hide();
    }

    // NEW: OPEN A CARD DIRECTLY FROM THE HAND

    public void ShowHandCard(BattleCard card)
    {
        if (card == null)
            return;

        if (card.type == BattleCardType.Monster)
        {
            ShowMonsterData(card.monster);
        }
        else if (card.type == BattleCardType.Spell)
        {
            ShowSpell(card.spell);
        }
    }

    // SHOW A MONSTER THAT IS ON THE BATTLEFIELD

    public void ShowMonster(BattleMonster monster)
    {
        if (monster == null || monster.data == null)
            return;

        ShowMonsterData(monster.data);

        // Replace original HP with live battle HP.
        if (monsterCardView != null)
        {
            monsterCardView.SetBattleHealth(
                monster.currentHP
            );
        }
    }

    // SHOW SAVED MONSTER DATA

    private void ShowMonsterData(MonsterData monster)
    {
        if (monster == null)
            return;

        if (previewPanel == null ||
            monsterCardView == null)
        {
            Debug.LogWarning(
                "BattleCardPreview: Monster preview missing.",
                this
            );
            return;
        }

        previewPanel.SetActive(true);

        monsterCardView.gameObject.SetActive(true);

        if (spellCardView != null)
            spellCardView.gameObject.SetActive(false);

        monsterCardView.DisplayMonster(
            monster,
            "Effect descriptions will be connected next.",
            false
        );

        monsterCardView.SetCompact(false);
    }

    // SHOW A SPELL FROM THE HAND

    public void ShowSpell(SpellData spell)
    {
        if (spell == null)
            return;

        if (previewPanel == null ||
            spellCardView == null)
        {
            Debug.LogWarning(
                "BattleCardPreview: Spell preview missing.",
                this
            );
            return;
        }

        previewPanel.SetActive(true);

        spellCardView.gameObject.SetActive(true);

        if (monsterCardView != null)
            monsterCardView.gameObject.SetActive(false);

        spellCardView.DisplaySpell(spell);
    }

    // CLOSE PREVIEW

    public void Hide()
    {
        if (previewPanel != null)
            previewPanel.SetActive(false);
    }
}
