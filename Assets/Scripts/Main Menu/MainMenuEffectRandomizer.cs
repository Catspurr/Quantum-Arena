using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MainMenuEffectRandomizer : MonoBehaviour
{
    [Header("Scene effects for main menu")]
    [SerializeField][Tooltip("These will show to the right of the screen in the main menu")] 
    private GameObject[] effects;

    private void OnEnable()
    {
        foreach (var effect in effects)
        {
            effect.SetActive(false);
        }
        effects[Random.Range(0, effects.Length)].SetActive(true);
    }
}