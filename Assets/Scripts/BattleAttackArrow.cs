
using UnityEngine;
using UnityEngine.UI;

public class BattleAttackArrow : MonoBehaviour
{
    [Header("Arrow UI")]
    [SerializeField] private RectTransform arrowLayer;
    [SerializeField] private RectTransform arrowHead;

    [Header("Arrow Settings")]
    [SerializeField] private float shaftThickness = 8f;
    [SerializeField]
    private Vector2 headSize =
        new Vector2(45f, 45f);

    [SerializeField]
    private Color shaftColor =
        new Color(1f, 0.12f, 0.20f, 1f);

    [Tooltip(
        "Your red arrow sprite points left, " +
        "so its correction is 180 degrees."
    )]
    [SerializeField] private float headRotationOffset = 180f;

    private RectTransform shaft;
    private Canvas canvas;

    private bool drawing;
    private Vector2 originScreenPoint;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();

        if (arrowLayer == null)
            arrowLayer = GetComponent<RectTransform>();

        CreateShaft();
        StopArrow();
    }

    private void Update()
    {
        if (!drawing)
            return;

        DrawTo(Input.mousePosition);
    }

    private void CreateShaft()
    {
        GameObject shaftObject = new GameObject(
            "Arrow_Shaft",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        shaftObject.transform.SetParent(
            arrowLayer,
            false
        );

        shaft = shaftObject.GetComponent<RectTransform>();

        shaft.anchorMin = new Vector2(0.5f, 0.5f);
        shaft.anchorMax = new Vector2(0.5f, 0.5f);
        shaft.pivot = new Vector2(0.5f, 0.5f);

        Image shaftImage =
            shaftObject.GetComponent<Image>();

        shaftImage.color = shaftColor;
        shaftImage.raycastTarget = false;

        // Draw the arrowhead over the shaft.
        if (arrowHead != null)
            arrowHead.SetAsLastSibling();
    }

    public void BeginArrow(RectTransform source)
    {
        if (source == null || canvas == null)
            return;

        Camera uiCamera = GetUICamera();

        originScreenPoint =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                source.TransformPoint(source.rect.center)
            );

        drawing = true;

        if (shaft != null)
            shaft.gameObject.SetActive(true);

        if (arrowHead != null)
            arrowHead.gameObject.SetActive(true);

        DrawTo(Input.mousePosition);
    }

    private void DrawTo(Vector2 screenDestination)
    {
        if (arrowLayer == null || shaft == null ||
            arrowHead == null)
            return;

        Vector2 start;
        Vector2 end;

        Camera uiCamera = GetUICamera();

        bool validStart = RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                arrowLayer,
                originScreenPoint,
                uiCamera,
                out start
            );

        bool validEnd = RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                arrowLayer,
                screenDestination,
                uiCamera,
                out end
            );

        if (!validStart || !validEnd)
            return;

        Vector2 direction = end - start;
        float distance = direction.magnitude;

        if (distance < 1f)
            return;

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        Vector2 normalized = direction.normalized;

        float shaftLength = Mathf.Max(
            0f,
            distance - headSize.x * 0.4f
        );

        shaft.sizeDelta = new Vector2(
            shaftLength,
            shaftThickness
        );

        shaft.anchoredPosition =
            start + normalized * (shaftLength * 0.5f);

        shaft.localRotation =
            Quaternion.Euler(0, 0, angle);

        arrowHead.anchorMin =
            new Vector2(0.5f, 0.5f);

        arrowHead.anchorMax =
            new Vector2(0.5f, 0.5f);

        arrowHead.pivot =
            new Vector2(0.5f, 0.5f);

        arrowHead.sizeDelta = headSize;
        arrowHead.anchoredPosition = end;

        arrowHead.localRotation =
            Quaternion.Euler(
                0,
                0,
                angle + headRotationOffset
            );
    }

    public void StopArrow()
    {
        drawing = false;

        if (shaft != null)
            shaft.gameObject.SetActive(false);

        if (arrowHead != null)
            arrowHead.gameObject.SetActive(false);
    }

    private Camera GetUICamera()
    {
        if (canvas == null ||
            canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return canvas.worldCamera;
    }
}
