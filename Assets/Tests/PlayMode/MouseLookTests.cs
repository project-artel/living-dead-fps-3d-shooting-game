using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace LivingDead.Tests
{
    public class MouseLookTests
    {
        private bool ignoredLogs;

        [SetUp]
        public void SetUp()
        {
            ignoredLogs = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            foreach (var system in UnityEngine.Object.FindObjectsOfType<UnityEngine.EventSystems.EventSystem>())
                system.enabled = false;
        }

        [TearDown]
        public void TearDown()
        {
            LogAssert.ignoreFailingMessages = ignoredLogs;
        }

        [UnityTest]
        public IEnumerator SdkMouseMovement_RotatesCombatCamera()
        {
            LogAssert.ignoreFailingMessages = true;
            var artelInput = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Artel.ArtelInput")).First(t => t != null);
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            artelInput.GetProperty("PointerHeldForRun", flags).SetValue(null, true);
            var move = artelInput.GetMethod("MoveMouse", flags);
            move.Invoke(null, new object[] { new Vector2(50, 50) });
            for (int i = 0; i < 3; i++) yield return null;
            SceneManager.LoadScene("Map_v2");
            for (int i = 0; i < 10; i++) yield return null;
            var character = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>()
                .First(x => x.GetType().FullName == "InfimaGames.LowPolyShooterPack.Character");
            var input = character.GetComponent<PlayerInput>();
            var look = input.actions.FindAction("Look", true);
            var cameraLook = character.GetComponentsInChildren<MonoBehaviour>()
                .First(x => x.GetType().Name == "CameraLook");
            move.Invoke(null, new object[] { new Vector2(100, 100) });
            for (int i = 0; i < 3; i++) yield return null;
            var before = cameraLook.transform.rotation;
            string trace = "";
            Action<InputAction.CallbackContext> record = ctx => trace += ctx.phase + ":" + ctx.ReadValue<Vector2>() + ";";
            look.performed += record;
            look.canceled += record;
            move.Invoke(null, new object[] { new Vector2(400, 300) });
            for (int i = 0; i < 4; i++) yield return null;
            float angle = Quaternion.Angle(before, cameraLook.transform.rotation);
            Debug.Log("MOUSE_LOOK_PROBE angle=" + angle + " trace=" + trace + " locked=" +
                character.GetType().GetMethod("IsCursorLocked").Invoke(character, null) +
                " devices=" + string.Join(",", input.devices.Select(d => d.displayName)));
            look.performed -= record;
            look.canceled -= record;
            artelInput.GetMethod("ReleaseAllVirtualInput", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Invoke(null, null);
            Assert.That(angle, Is.GreaterThan(1f), trace);
        }
    }
}
