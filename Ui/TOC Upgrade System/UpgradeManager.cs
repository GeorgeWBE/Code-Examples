using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("Assign")]
    public Upgrade[] upgradeScripts; 
    [Header("Debug")]
    [SerializeField] private MainMenuManager mainMenuManager;
    private void Awake() 
    {
        mainMenuManager = MainMenuManager.instance; 
    }
    private void OnEnable() 
    {
        SetAllUpgrades();
    }
    private void OnDisable() 
    {
        SetAllUpgrades();
    }
    public void SetAllUpgrades()
    {
        for (int i = 0; i < upgradeScripts.Length; i++)
        {
            upgradeScripts[i].SetUpgradeLevels(); 
            upgradeScripts[i].SetValues();
        }
    } 
}
