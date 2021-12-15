using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Health : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100;

    [SerializeField] private float health = 100;

    [SerializeField] private HealthDisplay healthDisplay;

    /*public delegate void HealthChangedDeligate(float health, float maxHealth);

    public event HealthChangedDeligate healthChanged;*/

    [ClientRpc]
    private void SetHealth()
    {
        health = maxHealth;
        //healthChanged?.Invoke(health, maxHealth);
    }

    /*public void OnStartAutority()
    {
        SetHealth();
    }*/

    [Command]
    public void CmdDealDamage(float damage)
    {
        /*if (!hasAuthority)
        {
            return;
        }*/
        health -= damage;
        healthDisplay.HandleHealthChanged(health);
    }

    /*[ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            health -= 5;
            healthDisplay.HandleHealthChanged(health);
        }
    }
    /*[ClientCallback]
    private void Update()
    {
        if (!hasAuthority) { return; }

        CmdDealDamage(10);
    }*/
}
