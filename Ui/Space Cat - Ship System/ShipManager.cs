using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Pickup_Part;

public class ShipManager : MonoBehaviour
{
    [Header("Assign inventory slots")]
    public InventorySlot[] partSlots;

    [Header("Assign Ship slots")]
    public ShipSlot[] shipSlots;
    private Dictionary<ShipPart, ShipSlot> shipDict = new Dictionary<ShipPart, ShipSlot>();
    private int currentProgress;

    private void Awake() 
    {
        //creates the ship slots dicitonary
        for (int i = 0; i < shipSlots.Length; i++)
        {
            shipDict.Add(shipSlots[i].partToAssign, shipSlots[i]);
        }


        ShipSave shipSave = JsonReadWrite.ReadFromJSON<ShipSave>("ShipSave", JsonReadWrite.saveFolder);

        //Sets the part slots
        if (shipSave != null)
        {
            for (int i = 0; i < shipSave.shipParts.Count; i++)
            {
                //Retrieves the sprite for the part
                ShipPart shipPart = Resources.Load<ShipPart>(shipSave.shipParts[i].partLocation);

                if (shipPart != null) 
                {
                    if (!shipSave.shipParts[i].isEquipped) {partSlots[i].SetSlot(shipPart);}
                    else
                    {
                        if (!shipDict.ContainsKey(shipPart)) {return;}  
                        shipDict[shipPart].SetShipSlot(shipPart);
                    }
                }
            }

            currentProgress = shipSave.shipProgress;
        }

    }

    public void OnShipFinish()
    {
        print("Shup finished m8");
    }
}

public class ShipSave
{
    public int shipProgress;
    public List<ShipPartSave> shipParts;
}

public class ShipPartSave
{
    public string partName;
    public string partLocation;
    public bool isEquipped = false;
}
