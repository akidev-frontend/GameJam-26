using System.Collections;
using UnityEngine;

namespace GameJam.UI
{
    /// <summary>
    /// Oculta el SpriteRenderer al arrancar y lo muestra tras un retardo.
    /// Útil para mensajes que deben aparecer unos segundos después de entrar al nivel.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ShowAfterDelay : MonoBehaviour
    {
        [Header("Ajustes")]
        [Tooltip("Segundos de espera antes de mostrar el sprite.")]
        [SerializeField] private float delay = 1f;

        private SpriteRenderer sr;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr) sr.enabled = false;
        }

        private void Start()
        {
            StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (sr) sr.enabled = true;
        }
    }
}
