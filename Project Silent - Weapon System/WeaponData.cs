using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Ammo Options")]
    public bool infiniteBank = false;
    public bool infiniteClip = false;
    public int maxBankAmmo;
    public int maxClipAmmo;

    [Header("Aim Options")]
    public bool canAim;

    [Header("Weapon Animations")]
    public AnimationClip attackAnimation1;
    public AnimationClip attackAnimation2;
    public AnimationClip attackAnimation3;
    public AnimationClip refreshAnimation;
}
