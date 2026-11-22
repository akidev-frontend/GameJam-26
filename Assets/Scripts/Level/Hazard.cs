using UnityEngine;

namespace GameJam.Level
{
    /// <summary>
    /// Zona mortal (p. ej. los helados, que creen que son pinchos).
    /// Al entrar el Player en la hitbox se "muere": reaparece en el SpawnPoint.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hazard : MonoBehaviour
    {
        [Tooltip("Punto de reaparición; si no se asigna, busca un GameObject llamado 'SpawnPoint'.")]
        [SerializeField] private Transform spawnPoint;

        [Header("Animación de muerte")]
        [Tooltip("Duración de la animación de muerte (clip RataMuerte: 8 frames a 12 fps ≈ 0,67 s) antes de reaparecer.")]
        [SerializeField] private float deathDuration = 0.9f;

        [Tooltip("Segundos que el control queda bloqueado tras reaparecer.")]
        [SerializeField] private float respawnFreeze = 0.1f;

        private Rigidbody2D playerRb;        private void Reset()
        {
            if (TryGetComponent(out Collider2D col)) col.isTrigger = true;
        }

        private void OnValidate()
        {
            if (TryGetComponent(out Collider2D col)) col.isTrigger = true;
        }

        private void Awake()
        {
            if (spawnPoint == null)
                spawnPoint = GameObject.Find("SpawnPoint")?.transform;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var controller2D = other.GetComponent<GameJam.Player.PlayerController2D>()
                               ?? other.GetComponentInParent<GameJam.Player.PlayerController2D>();
            if (controller2D == null) return;

            Debug.Log($"[Hazard] {other.name} tocó un helado: muere y reaparece.", this);
            Die(other.transform, controller2D);
        }

        private void Die(Transform victim, GameJam.Player.PlayerController2D controller2D)
        {
            if (!playerRb) victim.TryGetComponent(out playerRb);

            var animator = victim.GetComponentInParent<GameJam.Player.PlayerAnimator2D>();
            StartCoroutine(PlayerDeathFlow.Run(
                victim, controller2D, animator, playerRb,
                spawnPoint ? spawnPoint.position : Vector3.zero,
                deathDuration, respawnFreeze));
        }
    }
}
