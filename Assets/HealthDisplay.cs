using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;

public class HealthDisplay : NetworkBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Slider slider;


    [ClientRpc]
    public void HandleHealthChanged(float hp)
    {
        slider.value = hp;
    }
}
