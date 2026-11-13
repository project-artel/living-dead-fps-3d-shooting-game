using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LivingDead.Tests
{
    public class TeleportPlacementTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private Transform player;
        private CapsuleCollider capsule;
        private Transform target;
        private MethodInfo findLanding;

        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("LivingDeadNavigationBehaviors"))
                .First(candidate => candidate != null);
            findLanding = type.GetMethod("TryFindLanding", BindingFlags.Static | BindingFlags.NonPublic);
            CreateBox("Ground", new Vector3(0, -0.5f, 0), new Vector3(30, 1, 30));
            player = CreateObject("Player").transform;
            player.position = new Vector3(-10, 0, 0);
            capsule = player.gameObject.AddComponent<CapsuleCollider>();
            capsule.radius = 0.3f;
            capsule.height = 1.8f;
            capsule.center = Vector3.up;
            target = CreateObject("Target").transform;
            target.position = new Vector3(0, 0.5f, 0);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var created in objects)
                if (created) UnityEngine.Object.DestroyImmediate(created);
            objects.Clear();
        }

        [Test]
        public void ClearGround_PlacesFeetAboveFloorWithinInteractionRange()
        {
            Assert.That(Find(out var landing), Is.True);
            Assert.That(Vector3.Distance(landing, target.position), Is.LessThanOrEqualTo(2f));
            float feet = landing.y + capsule.center.y - capsule.height * 0.5f;
            Assert.That(feet, Is.EqualTo(0.05f).Within(0.001f));
        }

        [Test]
        public void BlockedFront_UsesClearSpaceBesideTheTarget()
        {
            CreateBox("Wall in front", new Vector3(-1.5f, 1, 0), new Vector3(1, 2, 1));
            Assert.That(Find(out var landing), Is.True);
            Assert.That(Mathf.Abs(landing.z), Is.GreaterThan(0.8f));
            Assert.That(Vector3.Distance(landing, target.position), Is.LessThanOrEqualTo(2f));
        }

        [Test]
        public void EntireDestinationBlocked_RefusesAndPreservesPlayerPosition()
        {
            CreateBox("Blocked destination", new Vector3(0, 9.5f, 0), new Vector3(10, 20, 10));
            var original = player.position;
            Assert.That(Find(out var landing), Is.False);
            Assert.That(landing, Is.EqualTo(original));
            Assert.That(player.position, Is.EqualTo(original));
        }

        [Test]
        public void NoGround_RefusesInsteadOfDroppingPlayer()
        {
            target.position = new Vector3(50, 0.5f, 0);
            Assert.That(Find(out _), Is.False);
        }

        private bool Find(out Vector3 landing)
        {
            Physics.SyncTransforms();
            object[] arguments = { player, capsule, target, 2f, Vector3.zero };
            bool found = (bool)findLanding.Invoke(null, arguments);
            landing = (Vector3)arguments[4];
            return found;
        }

        private GameObject CreateObject(string name)
        {
            var created = new GameObject(name);
            objects.Add(created);
            return created;
        }

        private void CreateBox(string name, Vector3 position, Vector3 size)
        {
            var box = CreateObject(name);
            box.transform.position = position;
            box.AddComponent<BoxCollider>().size = size;
        }
    }
}
