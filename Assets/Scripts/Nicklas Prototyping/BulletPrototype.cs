using System;
using UnityEngine;

namespace Nicklas_Prototyping
{
    public class BulletPrototype : MonoBehaviour
    {
        private void Awake()
        {
            Destroy(gameObject, 5f);
        }

        private void OnCollisionEnter(Collision other)
        {
            Destroy(gameObject);
        }
    }
}