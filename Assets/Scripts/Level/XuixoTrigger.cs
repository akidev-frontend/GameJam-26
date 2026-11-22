using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Level
{
    /// <summary>
    /// Trigger colocado sobre xuixo_plat_0 (Level1). Cuando el Player entra en
    /// la hitbox (BoxCollider2D en modo trigger) ejecuta una serie de
    /// acontecimientos: diálogo opcional, activar/desactivar objetos y un
    /// UnityEvent que se puede cablear desde el Inspector.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class XuixoTrigger : MonoBehaviour
    {
        [Header("Referencias opcionales")]
        [SerializeField] private GameJam.UI.DialogueSystem dialogue;
        [Tooltip("Texto que desaparece justo al tocar (p. ej. el DialogueText de bienvenida).")]
        [SerializeField] private GameObject textoAnterior;
        [SerializeField, TextArea] private string[] dialogueLines =
        {
            "¡Regla rota!",
            "A partir de ahora todo cambia.",
        };

        [Header("Objetos")]
        [SerializeField] private GameObject[] activateOnTouch;
        [SerializeField] private GameObject[] deactivateOnTouch;

        [Header("Cocinero")]
        [SerializeField] private GameObject cocinero;
        [SerializeField] private Sprite spriteEnfadado;
        [Tooltip("Si no se asignó cocinero, busca el primero llamado cocinero en la escena.")]
        [SerializeField] private bool buscarCocineroAutomatico = true;

        [Header("Evento")]
        [Tooltip("Cableá aquí los acontecimientos extra del nivel (cámara, reglas, sonido, etc.).")]
        public UnityEvent onPlayerTouch;

        private bool triggered;

        private void Awake()
        {
            // Si no se asignó diálogo desde el Inspector, busca uno en la escena.
            if (!dialogue)
                dialogue = UnityEngine.Object.FindFirstObjectByType<GameJam.UI.DialogueSystem>();

            // Fallback: localizar el cocinero por nombre si no se asignó.
            if (!cocinero && buscarCocineroAutomatico)
                cocinero = GameObject.Find("cocinero_0") ?? GameObject.Find("cocinero");
        }

        private void Reset()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col) col.isTrigger = true;
        }

        private void OnValidate()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered) return;
            if (!other.GetComponent<GameJam.Player.PlayerController2D>()) return;

            Debug.Log("[XuixoTrigger] Player tocó la hitbox de xuixo_plat_0", this);
            triggered = true;

            // El texto de arriba (DialogueText) desaparece justo al tocar;
            // en su lugar aparece otro con la secuencia nueva.
            if (textoAnterior != null)
            {
                textoAnterior.SetActive(false);
                textoAnterior.GetComponent<GameJam.UI.DialogueSystem>()?.Vaciar();
            }

            if (dialogue != null)
            {
                PlayDialogue();
            }
            else
            {
                Debug.LogWarning("[XuixoTrigger] No hay DialogueSystem asignado.", this);
            }

            foreach (var go in activateOnTouch)
                if (go) go.SetActive(true);
            foreach (var go in deactivateOnTouch)
                if (go) go.SetActive(false);

            // Cambiar sprite del cocinero al enfadado (mismas posición y rotación).
            if (cocinero != null && spriteEnfadado != null)
            {
                var sr = cocinero.GetComponent<SpriteRenderer>();
                if (sr)
                {
                    Debug.Log("[XuixoTrigger] Cambiando sprite del cocinero a enfadado.", this);
                    sr.sprite = spriteEnfadado;
                }
                else
                {
                    Debug.LogWarning("[XuixoTrigger] El cocinero no tiene SpriteRenderer.", this);
                }
            }
            else
            {
                Debug.LogWarning("[XuixoTrigger] Cocinero o spriteEnfadado no asignados.", this);
            }

            onPlayerTouch.Invoke();
        }

        /// <summary>
        /// Dispara el diálogo asignado una sola vez: el texto y su voz viven
        /// en el propio DialogueSystem del objeto (líneas, duraciones y audio),
        /// de modo que cualquier DialogueText se puede duplicar y recablear
        /// sin tocar este trigger.
        /// </summary>
        private void PlayDialogue()
        {
            if (!dialogue) return;
            dialogue.Vaciar();
            dialogue.Play();
        }
    }
}
