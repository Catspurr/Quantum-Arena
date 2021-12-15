using System;
using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace Nicklas_Prototyping
{
    public class PlayerControllerPrototype : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float turnSpeed = 0.1f, aimSpeed = 50f;
        [Space] [Header("Shooting")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform bulletStartPoint;
        [SerializeField] private float fireForce = 20f, fireCooldown = 3f;
        [SerializeField] private GameObject aim;
        private Camera _camera;
        private Rigidbody _rigidbody;
        private WaitForSeconds _fireCooldown;
        private bool _recentlyShot, _moving;
        private Vector3 _ver, _hor;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _camera = Camera.main;
            _fireCooldown = new WaitForSeconds(fireCooldown);
        }

        private void Update()
        {
            if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
            {
                _ver = new Vector3(0f, 0f, Input.GetAxis("Vertical"));
                _hor = new Vector3(0f,Input.GetAxis("Horizontal"),0f);
                _moving = true;
            }
            else
                _moving = false;
            
            if (Input.GetKey(KeyCode.Space) && !_recentlyShot || Input.GetMouseButtonDown(0) && !_recentlyShot)
                Fire();

            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit))
                Aim(hit);
        }

        private void FixedUpdate()
        {
            if (_moving)
                Movement();
        }

        private void Movement()
        {
            var moveVector = transform.TransformDirection(_ver) * moveSpeed;
            _rigidbody.velocity = new Vector3(moveVector.x, _rigidbody.velocity.y, moveVector.z);
            transform.Rotate(_hor * turnSpeed);
        }

        private void Aim(RaycastHit hit)
        {
            var aimTarget = new Vector3(hit.point.x, aim.transform.position.y, hit.point.z);
            var lookTarget = Quaternion.LookRotation(aimTarget - aim.transform.position);
            aim.transform.rotation = Quaternion.RotateTowards(
                aim.transform.rotation, lookTarget, aimSpeed * Time.deltaTime);
        }

        private void Fire()
        {
            _recentlyShot = true;
            var bullet = Instantiate(bulletPrefab, bulletStartPoint.position, bulletStartPoint.rotation);
            bullet.GetComponent<Rigidbody>().velocity = aim.transform.TransformDirection(Vector3.forward * fireForce);
            StartCoroutine(FireCooldown());
        }

        private IEnumerator FireCooldown()
        {
            yield return _fireCooldown;
            _recentlyShot = false;
        }
    }
}