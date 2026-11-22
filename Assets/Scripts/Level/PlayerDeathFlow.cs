using System.Collections;
using UnityEngine;

namespace GameJam.Level
{
    /// <summary>
    /// Flujo compartido de muerte + respawn: congela controles, reproduce la
    /// animación de muerte, espera, teletransporta al spawn y devuelve el
    /// control. Lo usan Hazard (helados) y XuxoPickup (xuixo malo).
    /// </summary>
    public static class PlayerDeathFlow
    {
        public static IEnumerator Run(
            Transform victim,
            GameJam.Player.PlayerController2D controller,
            GameJam.Player.PlayerAnimator2D animator,
            Rigidbody2D rb,
            Vector3 target,
            float deathDuration,
            float respawnFreeze)
        {
            float scale = controller ? controller.InputScale : 1f;
            if (controller) controller.InputScale = 0f;
            if (rb) rb.linearVelocity = Vector2.zero;
            if (animator) animator.PlayDeath();

            if (deathDuration > 0f)
                yield return new WaitForSeconds(deathDuration);

            if (rb)
            {
                rb.position = target;
                rb.linearVelocity = Vector2.zero;
            }
            if (victim) victim.position = target;
            if (animator) animator.OnRespawned();

            if (respawnFreeze > 0f)
                yield return new WaitForSeconds(respawnFreeze);
            if (controller) controller.InputScale = scale;
        }
    }
}
