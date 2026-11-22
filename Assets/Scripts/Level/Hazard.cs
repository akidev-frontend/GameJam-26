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

        [Tooltip("Segundos que el control queda bloqueado tras reaparecer.")]
        [SerializeField] private float respawnFreeze = 0.1f;

        private Transform player;
        private GameJam.Player.PlayerController2D controller;
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
            controller = controller2D;
            if (!playerRb) player.TryGetComponent(out playerRb);

            var target = spawnPoint ? spawnPoint.position : Vector3.zero;
            if (playerRb)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.position = target;
            }
            player.position = target;

            if (respawnFreeze > 0f)
                StartCoroutine(FreezeControls());
        }

        private System.Collections.IEnumerator FreezeControls()
        {
            if (!controller) yield break;
            float scale = controller.InputScale;
            controller.InputScale = 0f;
            yield return new WaitForSeconds(respawnFreeze);
            if (controller) controller.InputScale = scale;
            player = null;
        }
    }
}
