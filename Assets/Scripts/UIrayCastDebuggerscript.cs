
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIRaycastDebugger : MonoBehaviour
{
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (EventSystem.current == null)
        {
            Debug.LogWarning("No EventSystem found!");
            return;
        }

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position = Input.mousePosition;

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        Debug.Log(
            "=== UI OBJECTS UNDER MOUSE ==="
        );

        if (results.Count == 0)
        {
            Debug.Log("No UI elements detected!");
            return;
        }

        foreach (RaycastResult result in results)
        {
            Debug.Log(
                "Detected: " +
                result.gameObject.name +
                " | Path: " +
                GetHierarchyPath(result.gameObject.transform)
            );
        }

        Debug.Log("============================");
    }

    private string GetHierarchyPath(Transform target)
    {
        string path = target.name;

        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }

        return path;
    }
}
