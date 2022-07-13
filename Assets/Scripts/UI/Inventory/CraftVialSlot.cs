﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class VialSelectDelegate : UnityEvent<IVial> {}

public class CraftVialSlot : MonoBehaviour, IDropHandler, IPointerDownHandler
{
    //Reference variable to update
    private IVial vial = null;

    [SerializeField]
    private Image vialSlot = null;

    //Audio
    private AudioSource audioFX = null;

    //Event method for selection
    public VialSelectDelegate OnCraftVialSelect;

    //On awake, set up event
    void Awake()
    {
        audioFX = GetComponent<AudioSource>();
    }

    //Public method to set Craft Vial slot to this craft vial
    public void SetUpCraftVial(IVial pv, VialIcon ui)
    {
        if (pv != null)
        {
            audioFX.Play(0);
            vial = pv;
            vialSlot.sprite = ui.GetSprite();
            vialSlot.color = pv.getColor();
        }
        else
        {
            Reset();
        }
    }

    //Public method to reset this craft slot
    public void Reset()
    {
        vial = null;
        vialSlot.color = Color.black;
        vialSlot.sprite = null;
    }

    //Accessor method to craft vial
    public IVial GetVial()
    {
        return vial;
    }

    //Event handler method for dropping
    //Method to drop Ingredient Icon in this slot
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            VialInventoryIcon vialIcon = eventData.pointerDrag.GetComponent<VialInventoryIcon>();
            if (vialIcon != null && vialIcon.GetVial() != null)
            {
                SetUpCraftVial(vialIcon.GetVial(), vialIcon);
                OnCraftVialSelect.Invoke(vial);
            }
        }
    }

    //Event handler method when clicking on this image
    public void OnPointerDown(PointerEventData eventData)
    {
        if (vial != null)
            OnCraftVialSelect.Invoke(vial);
    }
}
