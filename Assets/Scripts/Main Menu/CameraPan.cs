using UnityEngine;

namespace Main_Menu
{
    public class CameraPan : MonoBehaviour
    {
        [SerializeField] private GameObject cameraObject;
        [SerializeField] private float rotationSpeed, waveSpeed, waveAngle;
        private void Update()
        {
            transform.Rotate(
                0f,
                rotationSpeed*Time.deltaTime,
                0f);
            cameraObject.transform.Rotate(
                Mathf.Sin(Time.time * waveSpeed) * waveAngle,
                0f,
                0f);
        }
    }
}