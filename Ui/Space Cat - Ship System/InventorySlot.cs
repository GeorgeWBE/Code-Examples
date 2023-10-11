using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class InventorySlot : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Header("Assign - Slot Image")]
    public Image slotImage;
    public ShipManager shipManager;
    private Button button => gameObject.GetComponent<Button>();
    private Camera pCam => Camera.main;
    private Vector2 mousePos;
    public ShipPart currentPart;

    public void SetSlot(ShipPart shipPart)
    {
        currentPart = shipPart;

        //Sets the part image
        if (shipPart.partSprite) {slotImage.sprite = shipPart.partSprite;}
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (slotImage.sprite && currentPart)
        {
            slotImage.transform.position = eventData.position;
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        slotImage.transform.localPosition = Vector3.zero;

        if (!currentPart) {return;}
        
        ShipSlot shipSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<ShipSlot>();

        if (shipSlot && shipSlot.partToAssign == currentPart) 
        {
            //Adds it the ship slot
            shipSlot.SetShipSlot(currentPart);

            //Saves the new state of the part
            ShipSave shipSave = JsonReadWrite.ReadFromJSON<ShipSave>("ShipSave", JsonReadWrite.saveFolder);
            for (int i = 0; i < shipSave.shipParts.Count; i++)
            {
                if (shipSave.shipParts[i].partName == currentPart.name)
                {
                    shipSave.shipParts[i].isEquipped = true;
                    shipSave.shipProgress++;
                    if (shipSave.shipProgress >= 5)
                    {
                        shipManager.OnShipFinish();
                    }
                }
            }
            JsonReadWrite.SaveToJSON<ShipSave>(shipSave, "ShipSave", JsonReadWrite.saveFolder);

            //Removes it from the inventory
            currentPart = null;
            slotImage.sprite = null;

        }
    }
}
