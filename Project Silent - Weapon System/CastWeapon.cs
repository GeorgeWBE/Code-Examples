using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastWeapon : Weapon
{
    public enum CastType
    {
        RayCast,
        BoxCast
    }

    [Header("Cast Weapon -- Options")]
    public CastType castType;
    public float spread = 0.1f; 
    public float castDistance = 10;
    public float hitForce = 20;
    public bool attackDecal = true;
    public bool attackEffect = true;   

    [Header("Boxcast Weapon -- Settings")]
    public Vector3 boxSize = new Vector3(1f, 1f, 1f);

    //Info
    public RaycastHit shotHit; 
    private float spreadX; 
    private float spreadY;
    private RaycastHit[] attackHits = new RaycastHit[1];
    public override void Start()
    {
        base.Start();
    }
    public override void OnAttack()
    {
        //Adds recoil
        spreadX = Random.Range(-spread, spread);
        spreadY = Random.Range(-spread, spread);
        if (isAiming) 
        {
            if (spreadX != 0) 
            {
                spreadX = spreadX / 2;
            }

            if (spreadY != 0) 
            {
                spreadY = spreadY / 2;
            }
        }

        //Shoots a raycast
        //Vector3 pos = new Vector3(PlayerManager.instance.pCam.transform.position.x, PlayerManager.instance.pCam.transform.position.y,  PlayerManager.instance.pCam.transform.position.z);
        Vector3 dir = PlayerManager.instance.mainCam.transform.forward + new Vector3(spreadX, spreadY, 0);

        //Does the cast based on the cast type
        if (castType == CastType.RayCast)
        {
            Physics.Raycast(PlayerManager.instance.mainCam.transform.position, dir, out shotHit, castDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            attackHits[0] = shotHit;

        }
        else if (castType == CastType.BoxCast)
        {
            Vector3 boxCastOffset = transform.forward * (boxSize.magnitude * 1.1f); // Offset by half the diagonal of the box
            attackHits = Physics.BoxCastAll(PlayerManager.instance.mainCam.transform.position + boxCastOffset, boxSize * 0.5f, dir, transform.rotation, castDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        }

        for (int i = 0; i < attackHits.Length; i++)
        {
            //Damages hit
            if (attackHits[i].collider && attackHits[i].collider.tag != "Player")
            {
                //Gets the hit materal
                ObjectProperties objectProperties = attackHits[i].collider.GetComponent<ObjectProperties>();

                //Object Mat actions
                if (objectProperties && objectProperties.objectMaterial)
                {
                    ObjectMaterial objMat = objectProperties.objectMaterial;

                    //Decal
                    if (attackDecal && objMat.decals.Length > 0)
                    {
                        int randIndex = Random.Range(0, objMat.decals.Length);
                        Vector3 decalPos = new Vector3(attackHits[i].point.x, attackHits[i].point.y, attackHits[i].point.z);
                        GameObject decal = ObjectPooler.Instance.SpawnFromPool(objMat.decals[randIndex].name, decalPos, Quaternion.LookRotation(-attackHits[i].normal));
                        decal.transform.parent = attackHits[i].transform;
                    }
                    
                    //Shoot effect 
                    if (attackEffect && objMat.hitEffect)
                    {
                        Vector3 effectPos = new Vector3(attackHits[i].point.x, attackHits[i].point.y, attackHits[i].point.z);
                        GameObject effect = ObjectPooler.Instance.SpawnFromPool(objMat.hitEffect.name, effectPos, Quaternion.LookRotation(attackHits[i].normal));
                        ParticleSystem particleSystem = effect.GetComponent<ParticleSystem>();
                        particleSystem.Play();
                    }
                }

                //Applies force to target
                if (attackHits[i].collider.attachedRigidbody)
                {
                    attackHits[i].collider.attachedRigidbody.AddForceAtPosition(hitForce * PlayerManager.instance.mainCam.transform.forward, attackHits[i].point, ForceMode.Impulse); 
                }

                //Damages the target
                if (attackHits[i].collider.gameObject.GetComponent<IDamagable>() != null)
                {
                    print("HIT DEST");
                    attackHits[i].collider.GetComponent<IDamagable>().Damage(attackDamage);
                }
            }
        }

    }

    private void OnDrawGizmos() 
    {
        if (castType == CastType.BoxCast)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, boxSize);
        }
    }
}
