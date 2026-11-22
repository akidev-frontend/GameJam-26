using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameJam.UI;

namespace GameJam.Level
{
    /// <summary>
    /// Cuando el DialogueSystem asignado termina su secuencia (evento onFinish),
    /// espera unos segundos, hace un fundido a negro (FadeIn.FadeOut) y carga
    /// la escena destino. Colócalo junto al CanvasFade de la escena.
    /// </summary>
    public class DialogueSceneTransition : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Diálogo cuyo onFinish dispara la transición. Si se deja vacío, busca el primero de la escena.")]
        [SerializeField] private DialogueSystem dialogue;
        [Tooltip("Fundido de salida. Si se deja vacío, busca FadeIn en este mismo objeto.")]
        [SerializeField] private FadeIn fader;

        [Header("Ajustes")]
        [Tooltip("Segundos de espera tras acabar el diálogo antes del fundido.")]
        [SerializeField] private float delayAfterDialogue = 1f;
        [Tooltip("Duración del fundido a negro en segundos.")]
        [SerializeField] private float fadeDuration = 0.8f;
        [Tooltip("Nombre de la escena a cargar (debe estar en Build Settings).")]
        [SerializeField] private string targetScene = "Level2(normal)";
        [Tooltip("Si está activo, la transición solo se dispara una vez.")]
        [SerializeField] private bool triggerOnce = true;

        private bool used;
        private Coroutine routine;

        private void Awake()
        {
            if (!dialogue) dialogue = FindFirstObjectByType<DialogueSystem>();
            if (!fader) fader = GetComponent<FadeIn>();
        }

        private void OnEnable()
        {
            if (dialogue) dialogue.onFinish.AddListener(OnDialogueFinished);
        }

        private void OnDisable()
        {
            if (dialogue) dialogue.onFinish.RemoveListener(OnDialogueFinished);
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
        }

        private void OnDialogueFinished()
        {
            if (used && triggerOnce) return;
            used = true;
            routine = StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            if (delayAfterDialogue > 0f)
                yield return new WaitForSeconds(delayAfterDialogue);

            if (fader) yield return fader.FadeOut(fadeDuration);

            SceneManager.LoadScene(targetScene);
        }
    }
}
