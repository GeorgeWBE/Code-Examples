using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Object Material")]

public class ObjectMaterial : ScriptableObject
{
   [Header("Effects")]
   public GameObject[] decals;
   public GameObject hitEffect;

   [Header("Audio")]
   public AudioClip[] softHits;
   public AudioClip[] hardHits;
   public AudioClip[] footstepSounds;
}
