using UnityEngine;
using GameJam.UI;

namespace GameJam.Level
{
    /// <summary>
    /// Muestra un DialogueText (con su audio) cuando el jugador muere.
    /// Se coloca en un objeto siempre activo y el DialogueText puede ser un
    /// hijo desactivado: se activa al morir y se oculta al acabar la frase.
    /// </summary>
    public class DialogueOnDeath : MonoBehaviour
    {
        [Tooltip("Diálogo a mostrar. Si está vacío se busca en los hijos (aunque estén desactivados).")]
        [SerializeField] private DialogueSystem dialogue;

        [Tooltip("Objeto que se activa al morir. Si está vacío se usa el del DialogueSystem.")]
        [SerializeField] private GameObject dialogueRoot;

        [Tooltip("Ocultar el diálogo cuando termina de reproducirse.")]
        [SerializeField] private bool hideWhenFinished = true;

        [Tooltip("Mostrarlo solo la primera vez que mueres.")]
        [SerializeField] private bool onlyFirstDeath = true;

        private bool shownOnce;

        private void Awake()
        {
            if (dialogue == null)
                dialogue = GetComponentInChildren<DialogueSystem>(true);

            if (dialogueRoot == null && dialogue != null)
                dialogueRoot = dialogue.gameObject;

            // El diálogo empieza oculto hasta la primera muerte.
            if (dialogueRoot != null && dialogueRoot != gameObject)
                dialogueRoot.SetActive(false);

            if (dialogue != null)
                dialogue.onFinish.AddListener(OnDialogueFinished);
        }

        private void OnEnable() => PlayerDeathFlow.PlayerDied += OnPlayerDied;

        private void OnDisable() => PlayerDeathFlow.PlayerDied -= OnPlayerDied;

        private void OnPlayerDied(Transform victim)
        {
            if (this == null) return;
            if (onlyFirstDeath && shownOnce) return;

            shownOnce = true;

            // Si hay otro diálogo en marcha (p. ej. el de intro), se corta y se
            // oculta para dejar el sitio al de la muerte.
            SilenciarOtrosDialogos();

            if (dialogueRoot != null && !dialogueRoot.activeSelf)
                dialogueRoot.SetActive(true);

            if (dialogue != null)
                dialogue.Play();
        }

        /// <summary>Corta y oculta los demás diálogos que estén reproduciéndose en la escena.</summary>
        private void SilenciarOtrosDialogos()
        {
            var dialogos = FindObjectsByType<DialogueSystem>(FindObjectsSortMode.None);
            foreach (var otro in dialogos)
            {
                if (otro == null || otro == dialogue) continue;
                if (!otro.IsPlaying) continue;
                otro.Ocultar();
            }
        }

        private void OnDialogueFinished()
        {
            if (!hideWhenFinished || dialogueRoot == null) return;
            if (dialogueRoot == gameObject) return; // no apagar el objeto que lleva este script
            dialogueRoot.SetActive(false);
        }
    }
}
