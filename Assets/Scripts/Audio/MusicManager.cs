using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameJam.Audio
{
    /// <summary>
    /// Música de juego persistente. Cada escena de nivel trae su propio
    /// "MusicaJuego": el primero que aparece se queda vivo (DontDestroyOnLoad)
    /// y las copias de los siguientes niveles se autodestruyen. Así musica_2
    /// sigue sonando y NO se reinicia al cambiar de escena.
    /// Al volver a una escena que no es nivel (menú/inicio) se para y se destruye.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicManager : MonoBehaviour
    {
        private static MusicManager instance;

        private AudioSource source;

        private void Awake()
        {
            // Ya hay música de juego sonando: esta copia de la escena sobra.
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            source = GetComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D: ambiente, no posicional

            SceneManager.sceneLoaded += OnSceneLoaded;
            MaybePlay(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => MaybePlay(scene);

        private void MaybePlay(Scene scene)
        {
            // Solo suena dentro de las escenas de nivel; en el menú se para.
            if (IsLevel(scene))
            {
                if (source != null && !source.isPlaying)
                    source.Play();
            }
            else
            {
                if (source != null) source.Stop();
                Destroy(gameObject);
            }
        }

        private static bool IsLevel(Scene scene) => scene.path.Contains("/Levels/");
    }
}
