using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>
    /// Secuencia de arranque del juego: funde desde negro, muestra el logo del
    /// estudio durante un instante y vuelve a negro antes de cargar el menu principal.
    /// Colocar en un GameObject junto a un Canvas con "FadeOverlay" (Image negra a
    /// pantalla completa) y "Logo" (RawImage con la textura del logo).
    /// </summary>
    public class SplashSequence : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Image negra a pantalla completa que hace de fundido.")]
        [SerializeField] private Image fadeOverlay;
        [Tooltip("RawImage del logo que se muestra en el centro.")]
        [SerializeField] private RawImage logoImage;

        [Header("Ajustes")]
        [Tooltip("Duracion del fundido de negro a visible.")]
        [SerializeField] private float fadeInDuration = 0.8f;
        [Tooltip("Tiempo que el logo permanece visible en segundos.")]
        [SerializeField] private float logoHoldDuration = 1.5f;
        [Tooltip("Duracion del fundido de visible a negro.")]
        [SerializeField] private float fadeOutDuration = 0.6f;
        [Tooltip("Escena que se carga al terminar la secuencia.")]
        [SerializeField] private string nextScene = "MainMenu";

        private void Awake()
        {
            if (fadeOverlay == null)
            {
                var go = GameObject.Find("FadeOverlay");
                if (go != null) fadeOverlay = go.GetComponent<Image>();
            }

            if (logoImage == null)
            {
                var go = GameObject.Find("Logo");
                if (go != null) logoImage = go.GetComponent<RawImage>();
            }

            if (fadeOverlay != null)
            {
                fadeOverlay.color = Color.black;
                fadeOverlay.raycastTarget = true;
            }
        }

        private IEnumerator Start()
        {
            // 1. Negro -> visible
            yield return Fade(1f, 0f, fadeInDuration);

            // 2. Logo en pantalla
            yield return new WaitForSecondsRealtime(logoHoldDuration);

            // 3. Visible -> negro
            yield return Fade(0f, 1f, fadeOutDuration);

            // 4. Al menu principal
            if (!string.IsNullOrEmpty(nextScene))
                SceneManager.LoadScene(nextScene);
        }

        private IEnumerator Fade(float fromAlpha, float toAlpha, float duration)
        {
            if (fadeOverlay == null) yield break;

            var color = fadeOverlay.color;
            color.a = fromAlpha;
            fadeOverlay.color = color;

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                color.a = Mathf.SmoothStep(fromAlpha, toAlpha, Mathf.Clamp01(t / duration));
                fadeOverlay.color = color;
                yield return null;
            }

            color.a = toAlpha;
            fadeOverlay.color = color;
        }
    }
}
