
using TMPro;
using UnityEngine;

public class BattleCardTooltip : MonoBehaviour
{
    [Header("Tooltip References")]
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private TMP_Text tooltipText;

    [Header("Position Settings")]
    [SerializeField] private float horizontalOffset = 15f;
    [SerializeField] private float screenPadding = 10f;

    private Canvas canvas;
    private RectTransform canvasRect;

    private BattleMonster hoveredMonster;
    private RectTransform hoveredCard;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null && tooltipPanel != null)
            canvas = tooltipPanel.GetComponentInParent<Canvas>();

        if (canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();

        Hide();
    }

    private void LateUpdate()
    {
        if (hoveredMonster == null || hoveredCard == null)
            return;

        UpdateText();
        UpdatePosition();
    }

    // SHOW TOOLTIP BESIDE A MONSTER

    public void ShowMonster(
        BattleMonster monster,
        RectTransform cardRect)
    {
        if (monster == null ||
            cardRect == null ||
            tooltipPanel == null)
            return;

        hoveredMonster = monster;
        hoveredCard = cardRect;

        tooltipPanel.gameObject.SetActive(true);

        UpdateText();
        UpdatePosition();
    }

    // HIDE TOOLTIP

    public void Hide()
    {
        hoveredMonster = null;
        hoveredCard = null;

        if (tooltipPanel != null)
            tooltipPanel.gameObject.SetActive(false);
    }

    // UPDATE MONSTER INFORMATION

    private void UpdateText()
    {
        if (tooltipText == null ||
            hoveredMonster == null)
            return;

        BattleMonster monster = hoveredMonster;

        tooltipText.text =
            monster.data.monsterName +
            "\nHP: " + monster.currentHP +
            "/" + monster.MaxHP +
            "\nATK: " + monster.Attack +
            " | DEF: " + monster.Defence +
            "\nINT: " + monster.data.intelligence;
    }

    // POSITION BESIDE THE CARD

    private void UpdatePosition()
    {
        if (canvasRect == null ||
            tooltipPanel == null ||
            hoveredCard == null)
            return;

        Camera uiCamera = null;

        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;

        
        Vector3[] corners = new Vector3[4];
        hoveredCard.GetWorldCorners(corners);

        Vector2 bottomLeft =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera, corners[0]);

        Vector2 topRight =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera, corners[2]);

        float middleY =
            (bottomLeft.y + topRight.y) * 0.5f;

        
        Vector2 wantedScreenPoint = new Vector2(
            topRight.x + horizontalOffset,
            middleY
        );

        Vector2 localPoint;

        if (!RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                wantedScreenPoint,
                uiCamera,
                out localPoint))
            return;

        
        float halfWidth = tooltipPanel.rect.width * 0.5f;
        float halfHeight = tooltipPanel.rect.height * 0.5f;

        
        if (localPoint.x + halfWidth >
            canvasRect.rect.xMax - screenPadding)
        {
            Vector2 leftScreenPoint = new Vector2(
                bottomLeft.x - horizontalOffset,
                middleY
            );

            Vector2 leftLocal;

            if (RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    leftScreenPoint,
                    uiCamera,
                    out leftLocal))
            {
                
                localPoint.x =
                    leftLocal.x - halfWidth;
            }
        }
        else
        {
            localPoint.x += halfWidth;
        }

        localPoint.x = Mathf.Clamp(
            localPoint.x,
            canvasRect.rect.xMin + halfWidth + screenPadding,
            canvasRect.rect.xMax - halfWidth - screenPadding
        );

        localPoint.y = Mathf.Clamp(
            localPoint.y,
            canvasRect.rect.yMin + halfHeight + screenPadding,
            canvasRect.rect.yMax - halfHeight - screenPadding
        );

        tooltipPanel.anchoredPosition = localPoint;
    }
}
