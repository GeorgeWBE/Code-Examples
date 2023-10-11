using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
public abstract class Upgrade : MonoBehaviour
{
    [Header("Assign")]
    public int upgradeLevel; 
    public float[] upgradeValues; 
    public GameObject[] upgradeIndicators; 
    public int[] upgradePrices; 
    public TextMeshProUGUI costText;
    [Header("Debug")]
    public PlayerData playerData; 
    public abstract void SetValues(); 
    public void SetUpgradeLevels()
    {
        upgradeLevel = PlayerPrefs.GetInt(this.gameObject.name); 
        for (int i = 0; i < upgradeIndicators.Length; i++)
        {
            if (i < upgradeLevel)
            {
                upgradeIndicators[i].SetActive(true); 
            }
        }
    }
    public void UpgradeBrought()
    {
        if (upgradeLevel < upgradeValues.Length && playerData.amountOfFish >= upgradePrices[upgradeLevel])
        { 
            playerData.amountOfFish -= upgradePrices[upgradeLevel]; 
            playerData.SavePlayerVariables();
            playerData.SetPlayerVariables();
            MainMenuManager.instance.fishText.text = playerData.amountOfFish.ToString(); 
            upgradeLevel++;
            if (upgradeLevel == upgradeValues.Length)
            {
                //costText.text = upgradePrices[upgradeLevel - 1].ToString();
                costText.text = "Max";
            }
            else 
            {
                costText.text = upgradePrices[upgradeLevel].ToString();
            }
            upgradeIndicators[upgradeLevel - 1].SetActive(true); 
            PlayerPrefs.SetInt(this.gameObject.name, upgradeLevel); 
            SetValues(); 
        }
    }
    private void Awake() 
    {
        //Assigns the playerdata scriptableobj 
        playerData = MainMenuManager.instance.playerData;
        SetUpgradeLevels(); 
        SetValues();
        if (upgradeLevel == upgradeValues.Length)
            {
                costText.text = "Max";
            }
            else 
            {
                costText.text = upgradePrices[upgradeLevel].ToString();
            } 
    }
}
