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

        private Transform player;
        private Rigidbody2D playerRb;

        private void Reset()
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
            player = victim;
            if (!playerRb) player.TryGetComponent(out playerRb);

            var animator = player.GetComponentInParent<GameJam.Player.PlayerAnimator2D>();
            StartCoroutine(RespawnRoutine(victim, controller2D, animator, spawnPoint ? spawnPoint.position : Vector3.zero));
        }

        private System.Collections.IEnumerator RespawnRoutine(
            Transform victim,
            GameJam.Player.PlayerController2D controller2D,
            GameJam.Player.PlayerAnimator2D animator,
            Vector3 target)
        {
            // Congelar controles y frenar al player durante la animación de muerte
            float scale = controller2D ? controller2D.InputScale : 1f;
            if (controller2D) controller2D.InputScale = 0f;
            if (playerRb) playerRb.linearVelocity = Vector2.zero;
            if (animator) animator.PlayDeath();

            // Esperar a que termine la animación de muerte...
            if (deathDuration > 0f)
                yield return new WaitForSeconds(deathDuration);

            // ...y reaparecer en el SpawnPoint
            if (playerRb)
            {
                playerRb.position = target;
                playerRb.linearVelocity = Vector2.zero;
            }
            if (victim) victim.position = target;
            if (animator) animator.OnRespawned();

            // Breve congelación extra tras reaparecer
            if (respawnFreeze > 0f)
                yield return new WaitForSeconds(respawnFreeze);
            if (controller2D) controller2D.InputScale = scale;
            player = null;
        }
    }
}
