using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIForceSquare : MonoBehaviour
{
    #region Inspector Assigned Variables
    [SerializeField] Canvas m_parentCanvas;
    public Vector2 m_canvasSize;
    //public float m_heightToWidthRatio;
    //public Vector2 m_heightAnchorValues;
    //public Vector2 m_worldRectSize;
    //public Vector2 m_localRectSize;
    //public float m_targetWidth = 0.0f;
    //public float m_currentWidth = 0.0f;
    //public float m_widthDifference = 0.0f;
    public Vector2 m_localSize;
    public float m_desiredAnchorWidthPosition;
    public float m_result;
    public float m_halfResult;

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Helper Methods
    #if UNITY_EDITOR
    [ContextMenu("Clear Rect Offsets")]
    public void ClearOffsets()
    {
        if (TryGetComponent<RectTransform>(out RectTransform rect))
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
    [ContextMenu("Cache Parent Dimensions")]
    public void CacheCanvasDimensions()
    {
        RectTransform canvasRect = m_parentCanvas.GetComponent<RectTransform>();
        m_canvasSize = new Vector2(canvasRect.rect.width, canvasRect.rect.height);
    }
    [ContextMenu("Cache Anchor Width Position")]
    public void CacheAnchorWidthPosition()
    {
        if (TryGetComponent<RectTransform>(out RectTransform rect))
        {
            // Get the current anchor position
            m_desiredAnchorWidthPosition = rect.anchorMin.x + ((rect.anchorMax.x - rect.anchorMin.x) * 0.5f);
        }
    }
    [ContextMenu("Match Width to Height Anchors")]
    public void MatchWidthToHeightAnchors()
    {
        if (TryGetComponent<RectTransform>(out RectTransform rect))
        {
            // First, get the canvas rect component
            RectTransform canvasRect = GetParentCanvasRect();
            // Get the world corners of the rect into local space
            Vector3[] cornerPositions = new Vector3[4];
            rect.GetWorldCorners(cornerPositions);
            for (int i = 0; i < cornerPositions.Length; i++) 
                cornerPositions[i] = canvasRect.transform.InverseTransformPoint(cornerPositions[i]);
            // Get the current local dimensions
            m_localSize = new Vector2(cornerPositions[3].x - cornerPositions[0].x, cornerPositions[1].y - cornerPositions[0].y);
            // Set the target
            float targetWidth = m_localSize.y;
            m_result = targetWidth / m_localSize.x;
            m_halfResult = m_result * 0.5f;

            rect.anchorMin = new Vector2(0.0f, rect.anchorMin.y);
            rect.anchorMax = new Vector2(m_result, rect.anchorMax.y);

            ClearOffsets();
        }
        /*
        if (TryGetComponent<RectTransform>(out RectTransform rect))
        {
            // Get the canvas rect
            RectTransform canvasRect = m_parentCanvas.GetComponent<RectTransform>();
            m_canvasSize = new Vector2(canvasRect.rect.width, canvasRect.rect.height);
            // Get the canvas width to height ratio
            m_heightToWidthRatio = canvasRect.rect.height / canvasRect.rect.width;
            // Get the current world corner positions
            Vector3[] cornerWorldPositions = new Vector3[4];
            rect.GetWorldCorners(cornerWorldPositions);
            // Get the height anchor values
            m_heightAnchorValues = new Vector2(rect.anchorMin.y, rect.anchorMax.y);
            for (int i = 0; i < cornerWorldPositions.Length; i++)
            {
                cornerWorldPositions[i] = canvasRect.InverseTransformPoint(cornerWorldPositions[i]);
            }
            float localWidth = cornerWorldPositions[3].x - cornerWorldPositions[0].x;
            m_currentWidth = localWidth;
            float localHeight = cornerWorldPositions[1].y - cornerWorldPositions[0].y;
            m_localRectSize = new Vector2(localWidth, localHeight);
            m_targetWidth = localHeight;
            m_widthDifference = m_targetWidth - localWidth;
            // Get the current world width
            float potentialResult = m_targetWidth / localWidth;
            float currentWidthAnchorDifference = rect.anchorMax.x - rect.anchorMin.x;
            if ((currentWidthAnchorDifference == potentialResult))
            {
                UnityEngine.Debug.Log($"Width already matches height");
            }
            else
            {
                m_result = potentialResult;
            }
        }
        */

    }

    [ContextMenu("Match Height to Width Anchors")]
    public void MatchHeightToWidthAnchors()
    {
        if (TryGetComponent<RectTransform>(out RectTransform rect))
        {
            // First, get the canvas rect component
            RectTransform canvasRect = GetParentCanvasRect();
            // Get the world corners of the rect into local space
            Vector3[] cornerPositions = new Vector3[4];
            rect.GetWorldCorners(cornerPositions);
            for (int i = 0; i < cornerPositions.Length; i++)
                cornerPositions[i] = canvasRect.transform.InverseTransformPoint(cornerPositions[i]);
            // Get the current local dimensions
            m_localSize = new Vector2(cornerPositions[3].x - cornerPositions[0].x, cornerPositions[1].y - cornerPositions[0].y);
            // Set the target
            float targetHeight = m_localSize.x;
            m_result = targetHeight / m_localSize.y;
            m_halfResult = m_result * 0.5f;

            rect.anchorMin = new Vector2(rect.anchorMin.x, 0.0f);
            rect.anchorMax = new Vector2(rect.anchorMax.x, m_result);

            ClearOffsets();
        }
    }


    [ContextMenu("Fit Rotated Element within Parent")]
    public void FitRotatedElement()
    {
        if (TryGetComponent<RectTransform>(out RectTransform rect))
        {
            // Get parent rect transform
            RectTransform parentRect = rect.transform.parent.GetComponent<RectTransform>();
            // Get the aspect ratio
            float aspectRatio = parentRect.rect.size.x / parentRect.rect.size.y;
            float halfAspectRatio = aspectRatio * 0.5f;
            float halfAspectRatioInvert = (1.0f / aspectRatio) * 0.5f;
            // Set anchor points
            rect.anchorMin = new Vector2(0.5f - halfAspectRatioInvert, 0.5f - halfAspectRatio);
            rect.anchorMax = new Vector2(0.5f + halfAspectRatioInvert, 0.5f + halfAspectRatio);
            // Reset anchor position and offsets
            rect.anchoredPosition = Vector3.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    [ContextMenu("Update Width Based on Parent Vertical Layout Group")]
    public void UpdateWidthBasedOnVerticalLayoutGroup()
    {
        VerticalLayoutGroup verticalLayoutGroup = GetComponentInParent<VerticalLayoutGroup>();
        if (verticalLayoutGroup != null)
        {
            RectTransform _parentRectTransform = verticalLayoutGroup.GetComponent<RectTransform>();
            RectTransform _rectTransform = GetComponent<RectTransform>();
            _rectTransform.pivot = new Vector2(0, 1);
            _rectTransform.sizeDelta = new Vector2(_parentRectTransform.rect.size.x - (verticalLayoutGroup.padding.left + verticalLayoutGroup.padding.right), _rectTransform.sizeDelta.y);
        }
    }
    public RectTransform GetParentCanvasRect()
    {
        if (!m_parentCanvas)
        {
            Canvas[] c = GetComponentsInParent<Canvas>();
            m_parentCanvas = c[c.Length - 1];
        }

        return m_parentCanvas.GetComponent<RectTransform>();
    }
    #endif
    #endregion
}
