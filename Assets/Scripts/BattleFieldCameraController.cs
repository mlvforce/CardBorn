
using UnityEngine;
using UnityEngine.EventSystems;

public class BattlefieldCameraController : MonoBehaviour
{
    [Header("Battlefield References")]
    [SerializeField] private RectTransform battlefieldContent;
    [SerializeField] private RectTransform battlefieldViewport;

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 0.65f;
    [SerializeField] private float maxZoom = 1.6f;
    [SerializeField] private float zoomSpeed = 0.12f;
    [SerializeField] private float startingZoom = 1f;

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 1f;

    private float currentZoom;
    private Vector2 lastMousePosition;
    private bool isPanning;

    private void Start()
    {
        if (battlefieldContent == null ||
            battlefieldViewport == null)
        {
            Debug.LogError(
                "BattlefieldCameraController: Assign " +
                "the Content and Viewport!",
                this
            );

            enabled = false;
            return;
        }

        if (battlefieldContent.parent !=
            battlefieldViewport)
        {
            Debug.LogWarning(
                "Battlefield_Content should be a direct " +
                "child of BattleField_view_point.",
                this
            );
        }

        startingZoom = Mathf.Clamp(
            startingZoom,
            minZoom,
            maxZoom
        );

        ResetView();
    }

    private void Update()
    {
        HandleZoom();
        HandlePan();
    }

    private void HandleZoom()
    {
        if (!MouseInsideViewport())
            return;

        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Approximately(scroll, 0f))
            return;

        float newZoom = Mathf.Clamp(
            currentZoom + scroll * zoomSpeed,
            minZoom,
            maxZoom
        );

        if (Mathf.Approximately(newZoom, currentZoom))
            return;

        // Keep the point beneath the mouse approximately
        // stationary while zooming.
        Camera uiCamera = GetUICamera();

        Vector2 mouseLocal;

        if (!RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                battlefieldContent,
                Input.mousePosition,
                uiCamera,
                out mouseLocal))
        {
            return;
        }

        float zoomRatio = newZoom / currentZoom;

        Vector2 oldPosition = battlefieldContent.anchoredPosition;

        battlefieldContent.localScale =
            new Vector3(newZoom, newZoom, 1f);

        battlefieldContent.anchoredPosition =
            oldPosition -
            mouseLocal * (newZoom - currentZoom);

        currentZoom = newZoom;

        ClampPosition();
    }

    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(2) &&
            MouseInsideViewport())
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }

        if (!isPanning)
            return;

        Vector2 mousePosition = Input.mousePosition;

        Vector2 previousLocal;
        Vector2 currentLocal;

        Camera uiCamera = GetUICamera();

        bool validPrevious =
            RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                battlefieldViewport,
                lastMousePosition,
                uiCamera,
                out previousLocal
            );

        bool validCurrent =
            RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                battlefieldViewport,
                mousePosition,
                uiCamera,
                out currentLocal
            );

        if (validPrevious && validCurrent)
        {
            Vector2 movement =
                (currentLocal - previousLocal) * panSpeed;

            battlefieldContent.anchoredPosition += movement;

            ClampPosition();
        }

        lastMousePosition = mousePosition;
    }

    private bool MouseInsideViewport()
    {
        return RectTransformUtility
            .RectangleContainsScreenPoint(
                battlefieldViewport,
                Input.mousePosition,
                GetUICamera()
            );
    }

    private Camera GetUICamera()
    {
        Canvas canvas =
            battlefieldViewport.GetComponentInParent<Canvas>();

        if (canvas == null)
            return null;

        if (canvas.renderMode ==
            RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return canvas.worldCamera;
    }

    private void ClampPosition()
    {
        float scaledWidth =
            battlefieldContent.rect.width * currentZoom;

        float scaledHeight =
            battlefieldContent.rect.height * currentZoom;

        float viewportWidth =
            battlefieldViewport.rect.width;

        float viewportHeight =
            battlefieldViewport.rect.height;

        Vector2 position =
            battlefieldContent.anchoredPosition;

        
        if (scaledWidth <= viewportWidth)
        {
            position.x = 0f;
        }
        else
        {
            float maxX =
                (scaledWidth - viewportWidth) / 2f;

            position.x = Mathf.Clamp(
                position.x,
                -maxX,
                maxX
            );
        }

        if (scaledHeight <= viewportHeight)
        {
            position.y = 0f;
        }
        else
        {
            float maxY =
                (scaledHeight - viewportHeight) / 2f;

            position.y = Mathf.Clamp(
                position.y,
                -maxY,
                maxY
            );
        }

        battlefieldContent.anchoredPosition = position;
    }

    public void ResetView()
    {
        if (battlefieldContent == null)
            return;

        currentZoom = startingZoom;

        battlefieldContent.localScale =
            new Vector3(
                currentZoom,
                currentZoom,
                1f
            );

        battlefieldContent.anchoredPosition =
            Vector2.zero;

        ClampPosition();
    }
}
