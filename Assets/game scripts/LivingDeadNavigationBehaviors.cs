#if ARTEL_GAME_FUNCTIONS
using System.Collections;
using Artel;
using InfimaGames.LowPolyShooterPack;
using UnityEngine;
using UnityEngine.SceneManagement;

internal static class LivingDeadNavigationBehaviors
{
    public enum AimPoint { Body, Head }

    [ArtelBehavior("보이는 적 중 가장 가까운 적의 몸통을 조준합니다.", group: "aim", when: nameof(CanControl))]
    private static IEnumerator AimAtNearestEnemy(ArtelBehaviorContext context)
    {
        var look = Object.FindAnyObjectByType<CameraLook>();
        Collider closest = null;
        float distance = float.PositiveInfinity;
        foreach (var enemy in Object.FindObjectsByType<Enemy_health>(FindObjectsSortMode.None))
            Consider(enemy.transform, look.transform, ref closest, ref distance);
        foreach (var enemy in Object.FindObjectsByType<fly_enemy_health_script>(FindObjectsSortMode.None))
            Consider(enemy.transform, look.transform, ref closest, ref distance);

        return Aim(look, closest, context);
    }

    [ArtelBehavior("현재 씬의 ARTEL 셀렉터로 지정한 적의 몸통 또는 머리를 조준합니다.",
        group: "aim", when: nameof(CanControl))]
    private static IEnumerator AimAtEnemy(string target, AimPoint aimPoint, ArtelBehaviorContext context)
    {
        return Aim(Object.FindAnyObjectByType<CameraLook>(), AimCollider(Resolve(target), aimPoint), context);
    }

    [ArtelBehavior("현재 씬의 ARTEL 셀렉터로 지정한 물체까지 걸어갑니다.",
        group: "movement", when: nameof(CanControl))]
    private static IEnumerator MoveToTarget(string target, [CheatRange(0.5f, 5f)] float stopDistance,
        ArtelBehaviorContext context)
    {
        var destination = Resolve(target);
        var character = Object.FindAnyObjectByType<Character>();
        var scene = SceneManager.GetActiveScene();
        var deadline = Time.unscaledTime + 12f;
        try
        {
            while (destination && destination.gameObject.activeInHierarchy && character
                && SceneManager.GetActiveScene() == scene && character.QaControlsAvailable
                && Time.unscaledTime < deadline)
            {
                var delta = destination.position - character.transform.position;
                if (delta.sqrMagnitude <= stopDistance * stopDistance)
                {
                    context.Complete($"도착했습니다. distance={delta.magnitude:F2}");
                    yield break;
                }

                var local = character.transform.InverseTransformDirection(delta);
                character.QaMovement = Vector2.ClampMagnitude(new Vector2(local.x, local.z), 1f);
                yield return null;
            }

            context.Fail(!destination ? "대상을 찾을 수 없습니다."
                : "지정 거리 안에 도착하지 못했습니다. 경로와 조작 상태를 확인하세요.");
        }
        finally
        {
            if (character)
                character.QaMovement = Vector2.zero;
        }
    }

    private static IEnumerator Aim(CameraLook look, Collider target, ArtelBehaviorContext context)
    {
        var scene = SceneManager.GetActiveScene();
        var character = Object.FindAnyObjectByType<Character>();
        var deadline = Time.unscaledTime + 5f;
        try
        {
            while (look && target && target.enabled && target.gameObject.activeInHierarchy
                && character && character.QaControlsAvailable && SceneManager.GetActiveScene() == scene
                && Time.unscaledTime < deadline)
            {
                if (CanSee(look.transform, target))
                {
                    var point = target.bounds.center;
                    look.QaAimPoint = point;
                    if (Vector3.Angle(look.transform.forward, point - look.transform.position) < 1.5f)
                    {
                        context.Complete($"조준했습니다. target={target.name}");
                        yield break;
                    }
                }
                else
                {
                    look.QaAimPoint = null;
                }

                yield return null;
            }

            context.Fail(!target ? "조준할 적을 찾을 수 없습니다."
                : "조준을 완료하지 못했습니다. 시야와 조작 상태를 확인하세요.");
        }
        finally
        {
            if (look)
                look.QaAimPoint = null;
        }
    }

    private static void Consider(Transform enemy, Transform view, ref Collider closest, ref float distance)
    {
        var candidate = AimCollider(enemy, AimPoint.Body);
        if (!candidate)
            return;

        float squared = (candidate.bounds.center - view.position).sqrMagnitude;
        if (squared < distance && CanSee(view, candidate))
        {
            closest = candidate;
            distance = squared;
        }
    }

    private static Collider AimCollider(Transform target, AimPoint aimPoint)
    {
        if (!target)
            return null;

        var enemy = target.GetComponentInParent<Enemy_health>();
        if (enemy && enemy.health > 0)
            return aimPoint == AimPoint.Head ? enemy.head : enemy.GetComponent<Collider>();

        var flying = target.GetComponentInParent<fly_enemy_health_script>();
        if (flying && flying.health > 0)
            return aimPoint == AimPoint.Head ? flying.eye_collider : flying.GetComponent<Collider>();

        return null;
    }

    private static bool CanSee(Transform view, Collider target)
    {
        var delta = target.bounds.center - view.position;
        return Physics.Raycast(view.position, delta.normalized, out var hit, delta.magnitude + 0.05f,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            && hit.collider == target;
    }

    private static string CanControl()
    {
        var character = Object.FindAnyObjectByType<Character>();
        var health = Object.FindAnyObjectByType<player_health>();
        return character && character.QaControlsAvailable && health && health.health > 0
            && !health.player_dead && Object.FindAnyObjectByType<CameraLook>()
            ? null : "전투에서 살아 있는 플레이어를 조작할 수 있어야 합니다.";
    }

    // SDK의 셀렉터 해석기는 내부 API이므로 현재 씬의 이름과 형제 인덱스만 확인합니다.
    private static Transform Resolve(string selector)
    {
        if (string.IsNullOrWhiteSpace(selector))
            return null;

        Transform current = null;
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (string segment in selector.Split('/'))
        {
            int bracket = segment.LastIndexOf('[');
            if (bracket <= 0 || !segment.EndsWith("]")
                || !int.TryParse(segment.Substring(bracket + 1, segment.Length - bracket - 2), out int index)
                || index < 0 || index >= (current ? current.childCount : roots.Length))
                return null;

            current = current ? current.GetChild(index) : roots[index].transform;
            if (current.name != segment.Substring(0, bracket))
                return null;
        }

        return current;
    }
}
#endif
