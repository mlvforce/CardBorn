
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Attach to the active Battle_Manager GameObject.
public class BattleGraveyardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private RectTransform listContent;
    [SerializeField] private Button closeButton;

    [Header("Generated Graveyard Rows")]
    [SerializeField]
    private Vector2 rowSize =
        new Vector2(360f, 45f);

    [SerializeField]
    private Color rowColor =
        new Color(0.23f, 0.11f, 0.35f, 1f);

    [SerializeField] private float fontSize = 20f;

    private BattleManager manager;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        Close();
    }

    // OPEN THE GRAVEYARD SELECTION WINDOW

    public void Open(BattleManager battleManager)
    {
        manager = battleManager;

        if (selectionPanel == null ||
            listContent == null ||
            manager == null ||
            manager.Player == null)
        {
            Debug.LogError(
                "BattleGraveyardUI: Missing panel, list or manager.",
                this
            );
            return;
        }

        ClearRows();

        int count = 0;

        for (int i = 0;
             i < manager.Player.graveyard.Count;
             i++)
        {
            BattleCard card =
                manager.Player.graveyard[i];

            if (card == null ||
                card.type != BattleCardType.Monster ||
                card.monster == null)
            {
                continue;
            }

            CreateRow(card, i);
            count++;
        }

        if (count == 0)
        {
            Close();
            return;
        }

        selectionPanel.SetActive(true);
        selectionPanel.transform.SetAsLastSibling();
    }

    // CLOSE THE WINDOW

    public void Close()
    {
        if (selectionPanel != null)
            selectionPanel.SetActive(false);
    }

    // REMOVE PREVIOUS GRAVEYARD ENTRIES

    private void ClearRows()
    {
        if (listContent == null)
            return;

        for (int i = listContent.childCount - 1;
             i >= 0;
             i--)
        {
            GameObject child =
                listContent.GetChild(i).gameObject;

            child.SetActive(false);
            Destroy(child);
        }
    }

    // CREATE A BUTTON FOR A DEFEATED MONSTER

    private void CreateRow(BattleCard card, int index)
    {
        GameObject row = new GameObject(
            "Grave_" + card.Name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(LayoutElement)
        );

        row.transform.SetParent(listContent, false);

        RectTransform rect =
            row.GetComponent<RectTransform>();

        rect.sizeDelta = rowSize;

        LayoutElement layout =
            row.GetComponent<LayoutElement>();

        layout.preferredWidth = rowSize.x;
        layout.preferredHeight = rowSize.y;

        Image background =
            row.GetComponent<Image>();

        background.color = rowColor;
        background.raycastTarget = true;

        Button button =
            row.GetComponent<Button>();

        button.onClick.AddListener(() =>
        {
            if (manager != null)
                manager.TryReturnMonsterFromGrave(index);
        });

        // MONSTER NAME TEXT

        GameObject labelObject = new GameObject(
            "Monster_Name",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        labelObject.transform.SetParent(
            row.transform,
            false
        );

        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();

        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;

        labelRect.offsetMin =
            new Vector2(10f, 0f);

        labelRect.offsetMax =
            new Vector2(-10f, 0f);

        TextMeshProUGUI label =
            labelObject.GetComponent<TextMeshProUGUI>();

        label.text =
            card.Name + " (" +
            card.monster.summonCost + " SP)";

        label.color = Color.white;
        label.fontSize = fontSize;
        label.alignment =
            TextAlignmentOptions.Center;

        label.raycastTarget = false;
    }
}
