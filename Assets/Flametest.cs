using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Mirror;

public class Flametest : NetworkBehaviour
{
    public GameObject flame;
    public VisualEffect flameEffect;
    [SyncVar(hook = nameof(HandleIsBurning))]
    public bool isBurning;
    public float cooldown;
    public float nextFireTime;
    
    // Start is called before the first frame update
    void Start()
    {
        flameEffect.Stop();
        //flameEffect.Play();
    }
    public void HandleIsBurning(bool oldValue, bool newValue) => Burn(newValue);
    [ClientCallback]
    void Update()
    {
        if (!isBurning)
        {
            /*if (Time.time < nextFireTime) return;
            nextFireTime = Time.time + cooldown;
            Instantiate(flame, transform.position, transform.rotation);*/

            flameEffect.Play();
        }
        /*else
        {
            flameEffect.Stop();
        }*/
        /*if (!isBurning)
        {
            flameEffect.Stop();
        }*/


        //Debug.Log(flameEffect.);

    }
    [Command]
    void Burn(bool isburn)
    {
        Debug.Log("testburn");
        //isBurning = isburn;
        if (isburn)
        {
            flameEffect.Play();
        } else
        {
            flameEffect.Stop();
        }
    }
}
