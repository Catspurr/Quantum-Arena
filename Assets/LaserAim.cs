using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserAim : MonoBehaviour
{
    public LayerMask layerMask;
    public float maxRange = 100;
    private LineRenderer lr;
    public TankController tank;
    public Material[] material;
    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.SetPosition(0, new Vector3(0, 0, 0));

        
    }

    // Update is called once per frame
    void Update()
    {
        if (tank != null)
        {
            //maxRange = character.myWeapon.WeaponStats.GetStatValue(StatType.Range);
            maxRange = tank.range;

            if (material != null && lr != null)
            {
                lr.material = material[tank.colorId];
                //material.color.r = tank.color.r;
                /*material.color = tank.color;
                material.color = Color.HSVToRGB(1, 1, 1, 1);*/
            }
        }
        
        lr.SetPosition(0, transform.position);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxRange, layerMask))
        {
            if (hit.collider)
            {
                lr.SetPosition(1, hit.point);
            }
        }
        else
        {
            lr.SetPosition(1, transform.position + transform.forward * maxRange);
        }
    }
}
