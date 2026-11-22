using UnityEngine;

namespace GameJam.Level
{
    /// <summary>
    /// Xuixo comestible: cuando el Player toca el collider (trigger),
    /// suma 1 al contador global de xiuxos comidos y el xuixo desaparece.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class XuxoPickup : MonoBehaviour
    {
        [Tooltip("Si está activo, el xuixo desaparece al comérselo.")]
        [SerializeField] private bool desaparecerAlComer = true;

        private bool eaten;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (eaten) return;
            if (!other.GetComponent<GameJam.Player.PlayerController2D>()) return;

            eaten = true;
            GameJam.UI.XuixoCounterText.NotifyEaten();

            if (desaparecerAlComer) gameObject.SetActive(false);
        }
    }
}
