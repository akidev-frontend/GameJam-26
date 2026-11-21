using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameJam.EditorTools
{
    /// <summary>
    /// Configuración de una sola vez para el jugador rata:
    /// crea el clip de caminar desde la hoja de sprites, el AnimatorController
    /// y ajusta los componentes del Player al nuevo tamaño.
    /// </summary>
    public static class PlayerAnimationSetup
    {
        private const string SheetPath = "Assets/Art/Sprites/Rata/rata_caminando-sheet-sheet.png";
        private const string ClipPath = "Assets/Animations/Player/RataWalk.anim";
        private const string ControllerPath = "Assets/Animators/PlayerRata.controller";

        [MenuItem("Tools/MCP/Setup Player Animation")]
        public static void Setup()
        {
            // 1. Frames de la hoja de sprites (sub-assets _0.._5, ordenados por nombre)
            var sprites = AssetDatabase.LoadAllAssetsAtPath(SheetPath)
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToArray();
            if (sprites.Length == 0)
            {
                Debug.LogError("[PlayerAnimationSetup] No hay sprites en " + SheetPath);
                return;
            }

            // 2. Clip de caminar: intercambio de sprites a 12 fps en bucle
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath) != null)
                AssetDatabase.DeleteAsset(ClipPath);

            var clip = new AnimationClip { frameRate = 12 };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            float step = 1f / clip.frameRate;
            var keys = new ObjectReferenceKeyframe[sprites.Length + 1];
            for (int i = 0; i < sprites.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i * step, value = sprites[i] };
            keys[sprites.Length] = new ObjectReferenceKeyframe { time = sprites.Length * step, value = sprites[0] };

            AnimationUtility.SetObjectReferenceCurve(
                clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
            AssetDatabase.CreateAsset(clip, ClipPath);

            // 3. AnimatorController con un único estado Walk por defecto
            if (!AssetDatabase.IsValidFolder("Assets/Animators"))
                AssetDatabase.CreateFolder("Assets", "Animators");
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
                AssetDatabase.DeleteAsset(ControllerPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var stateMachine = controller.layers[0].stateMachine;
            var walk = stateMachine.AddState("Walk");
            walk.motion = clip;
            stateMachine.defaultState = walk;

            // 4. Ajustar el Player de la escena activa
            var player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("[PlayerAnimationSetup] No se encontró 'Player' en la escena activa.");
                return;
            }

            var spriteRenderer = player.GetComponent<SpriteRenderer>();
            spriteRenderer.color = Color.white;                 // quitar el tinte azul del cubo
            spriteRenderer.sprite = sprites[0];

            // Collider al tamaño real del sprite (el cubo tenía uno mucho mayor)
            var collider = player.GetComponent<BoxCollider2D>();
            var spriteSize = sprites[0].bounds.size;
            collider.size = spriteSize;
            collider.offset = Vector2.zero;

            // GroundCheck justo debajo del nuevo collider
            var groundCheck = player.transform.Find("GroundCheck");
            if (groundCheck != null)
                groundCheck.localPosition = new Vector3(0f, -(spriteSize.y * 0.5f) - 0.02f, 0f);

            // Animator con el controller nuevo (ojo: usar == de Unity, no ??, por los fake-null)
            var animator = player.GetComponent<Animator>();
            if (animator == null)
                animator = player.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(player.scene);
            Debug.Log($"[PlayerAnimationSetup] OK: {sprites.Length} frames, collider {spriteSize.x}x{spriteSize.y}, controller asignado.");
        }
    }
}
