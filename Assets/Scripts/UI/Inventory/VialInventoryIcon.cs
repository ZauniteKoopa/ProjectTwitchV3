﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class VialInventoryIcon : VialIcon, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    //Variables for managing drag and drop
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    [SerializeField]
    private Canvas canvas = null;
    [SerializeField]
    private Image vialHighlight;
    public bool dropped;
    public UnityEvent iconSelectedEvent;

    // UI layer (render order)
    private Transform defaultParent;
    private Transform selectedParent;

    // Start is called before the first frame update
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        startPosition = rectTransform.anchoredPosition;
        defaultParent = transform.parent;
        dropped = false;
    }

    //Event handler when beginning to drag
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Assert(selectedParent != null);

        if (GetVial() != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.6f;
            transform.SetParent(selectedParent);
        }

        iconSelectedEvent.Invoke();
    }

    //Event handler when dragging icon
    public void OnDrag(PointerEventData eventData)
    {
        if (GetVial() != null)
        {
            rectTransform.anchoredPosition += (eventData.delta / canvas.scaleFactor);
        }
    }

    //Event handler when dropping icon
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dropped && GetVial() != null)
        { 
            transform.SetParent(defaultParent);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
            rectTransform.anchoredPosition = startPosition;
        }

        dropped = false;
    }


    // Main function to highlight this vial icon
    //  Pre: on represents whether to turn highlight on or off
    //  Post: sets highlight depending on boolean sent in
    public void setHighlight(bool on) {
        vialHighlight.enabled = on;
    }


    // Main function to set selected layer
    //  Pre: layer != null
    //  Post: sets where to send the icon when player selects it
    public void setSelectedLayer(Transform sLayer) {
        Debug.Assert(sLayer != null);
        selectedParent = sLayer;
    }


    // Main function to handle when the inventory closes
    //  Pre: inventory is about to close
    //  Post: sets it to this automatically
    public void onInventoryClose() {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = startPosition;
        dropped = false;
    }
}
