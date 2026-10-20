using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_SamuraiSliceRvl
{
    public class SmogParticle : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float minRotationSpeed = 150f;
        [SerializeField] private float maxRotationSpeed = 400f;
        [SerializeField] private float moveRadius = 0.25f; // Circular movement radius

        private float currentRotationSpeed;
        private Vector3 initialPos;
        private float currentAngle;

        public void Init()
        {
            initialPos = transform.localPosition;

            // Random initial rotation
            transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            
            // Random speed and direction
            currentRotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
            if (Random.value > 0.5f) currentRotationSpeed *= -1;

            // Random initial angle so they don't all start at the same point
            currentAngle = Random.Range(0f, 360f);
        }

        private void Update()
        {
            // Destroy if game is not active
            if (GameManager.instance.currentGame != "samuraiSliceRvl")
            {
                Destroy(gameObject);
                return;
            }

            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused) return;

            // Update angle for circular movement
            currentAngle += currentRotationSpeed * Time.deltaTime;

            // Calculate new position (Circular offset from initial position)
            float x = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * moveRadius;
            float y = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * moveRadius;

            transform.localPosition = initialPos + new Vector3(x, y, 0);
        }
    }
}
