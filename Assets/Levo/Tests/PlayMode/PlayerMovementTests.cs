using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Levo.Player;

namespace Levo.Tests
{
    /// <summary>Drives the real PlayerMovement with a FakePlayerInput (no keyboard needed).</summary>
    public class PlayerMovementTests : PlayModeTestBase
    {
        const float MaxSpeed = 6f;   // PlayerMovement default

        FakePlayerInput input;
        PlayerMovement movement;
        GameObject player;

        void BuildWorld()
        {
            CreateCube("Floor", new Vector3(0f, -0.5f, 0f), new Vector3(100f, 1f, 100f));

            player = Create("Player");
            player.transform.position = new Vector3(0f, 0.05f, 0f);
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            input = player.AddComponent<FakePlayerInput>();    // must exist before PlayerMovement wakes up
            movement = player.AddComponent<PlayerMovement>();
        }

        static float HorizontalSpeed(PlayerMovement m) => new Vector2(m.Velocity.x, m.Velocity.z).magnitude;

        // ---------- normal ----------
        [UnityTest]
        public IEnumerator Normal_Holding_forward_moves_the_player_forward()
        {
            BuildWorld();
            yield return new WaitForSeconds(0.6f);          // land on the floor
            float startZ = player.transform.position.z;

            input.Move = new Vector2(0f, 1f);
            yield return new WaitForSeconds(1.0f);

            Assert.Greater(player.transform.position.z - startZ, 3f);
        }

        [UnityTest]
        public IEnumerator Normal_Releasing_the_keys_slows_the_player_to_a_stop()
        {
            BuildWorld();
            yield return new WaitForSeconds(0.6f);
            input.Move = new Vector2(0f, 1f);
            yield return new WaitForSeconds(1.0f);

            input.Move = Vector2.zero;
            yield return new WaitForSeconds(1.5f);

            Assert.Less(HorizontalSpeed(movement), 0.05f);
        }

        [UnityTest]
        public IEnumerator Normal_Jump_lifts_the_player_off_the_floor()
        {
            BuildWorld();
            yield return new WaitForSeconds(0.6f);
            float floorY = player.transform.position.y;

            input.JumpPressed = true;
            yield return null;
            input.JumpPressed = false;

            float highest = floorY;
            float end = Time.time + 0.7f;
            while (Time.time < end)
            {
                highest = Mathf.Max(highest, player.transform.position.y);
                yield return null;
            }
            Assert.Greater(highest - floorY, 0.5f);
        }

        // ---------- boundary ----------
        [UnityTest]
        public IEnumerator Boundary_No_input_means_no_horizontal_movement()
        {
            BuildWorld();
            yield return new WaitForSeconds(0.6f);
            Vector3 start = player.transform.position;

            yield return new WaitForSeconds(1.0f);

            Vector3 end = player.transform.position;
            Assert.Less(new Vector2(end.x - start.x, end.z - start.z).magnitude, 0.01f);
        }

        [UnityTest]
        public IEnumerator Boundary_Speed_never_exceeds_the_maximum()
        {
            BuildWorld();
            yield return new WaitForSeconds(0.6f);
            input.Move = new Vector2(0f, 1f);

            float fastest = 0f;
            float end = Time.time + 1.5f;
            while (Time.time < end)
            {
                fastest = Mathf.Max(fastest, HorizontalSpeed(movement));
                yield return null;
            }
            Assert.LessOrEqual(fastest, MaxSpeed + 0.05f);
            Assert.Greater(fastest, MaxSpeed - 0.5f);
        }

        [UnityTest]
        public IEnumerator Boundary_Diagonal_movement_is_not_faster_than_straight()
        {
            BuildWorld();
            yield return new WaitForSeconds(0.6f);
            input.Move = new Vector2(1f, 1f);   // W + D together

            float fastest = 0f;
            float end = Time.time + 1.5f;
            while (Time.time < end)
            {
                fastest = Mathf.Max(fastest, HorizontalSpeed(movement));
                yield return null;
            }
            Assert.LessOrEqual(fastest, MaxSpeed + 0.05f);
        }

        // ---------- failure ----------
        [UnityTest]
        public IEnumerator Failure_Missing_input_component_logs_an_error_and_disables_the_script()
        {
            LogAssert.Expect(LogType.Error, new Regex("PlayerMovement needs"));

            var go = Create("NoInputPlayer");
            go.AddComponent<CharacterController>();
            var move = go.AddComponent<PlayerMovement>();
            yield return null;

            Assert.IsFalse(move.enabled);
        }
    }
}
