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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [ArtelCheat("지정한 대상의 distance 이내에 있는 바닥의 빈 위치로 순간이동합니다. 이동 시험에는 사용하지 않습니다.",
        when: nameof(CanControl))]
    private static string TeleportToTarget(string target, [CheatRange(0.5f, 2f)] float distance)
    {
        var destination = Resolve(target);
        var character = Object.FindAnyObjectByType<Character>();
        if (!destination || !destination.gameObject.activeInHierarchy || destination.IsChildOf(character.transform))
            throw new System.InvalidOperationException("현재 씬에서 플레이어 이외의 활성 대상을 지정하세요.");

        var body = character.GetComponent<Rigidbody>();
        var capsule = character.GetComponent<CapsuleCollider>();
        if (!body || !capsule || body.isKinematic)
            throw new System.InvalidOperationException("이동 가능한 플레이어 Rigidbody와 CapsuleCollider가 필요합니다.");

        Physics.SyncTransforms();
        if (!TryFindLanding(character.transform, capsule, destination, distance, out var landing))
            throw new System.InvalidOperationException("지정 거리 안에 바닥과 충분한 빈 공간을 찾지 못했습니다. 위치는 변경하지 않았습니다.");

        body.position = landing;
        character.transform.position = landing;
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        Physics.SyncTransforms();
        return $"teleported=true, target={target}, distance={Vector3.Distance(landing, destination.position):F2}";
    }

    private static bool TryFindLanding(Transform player, CapsuleCollider capsule, Transform destination,
        float distance, out Vector3 landing)
    {
        landing = player.position;
        if (capsule.direction != 1 || Vector3.Dot(player.up, Vector3.up) < 0.999f)
            return false;

        var scale = player.lossyScale;
        float radius = capsule.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        float halfHeight = Mathf.Max(capsule.height * Mathf.Abs(scale.y) * 0.5f, radius);
        var centerOffset = player.TransformVector(capsule.center);
        var towardPlayer = Vector3.ProjectOnPlane(player.position - destination.position, Vector3.up).normalized;
        if (towardPlayer.sqrMagnitude < 0.01f)
            towardPlayer = Vector3.ProjectOnPlane(destination.forward, Vector3.up).normalized;
        if (towardPlayer.sqrMagnitude < 0.01f)
            towardPlayer = Vector3.forward;

        for (int ring = 0; ring < 3; ring++)
        {
            for (int side = 0; side < 8; side++)
            {
                var direction = Quaternion.AngleAxis(side * 45f, Vector3.up) * towardPlayer;
                var probe = destination.position + direction * distance * (1f - ring * 0.25f);
                var hits = Physics.RaycastAll(probe + Vector3.up * 5f, Vector3.down, 10f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
                foreach (var hit in hits)
                {
                    if (hit.collider.transform.IsChildOf(player) || hit.collider.transform.IsChildOf(destination)
                        || hit.normal.y < 0.7f)
                        continue;

                    var candidate = hit.point + Vector3.up * (halfHeight + 0.05f) - centerOffset;
                    if (Vector3.Distance(candidate, destination.position) > distance)
                        continue;

                    var center = candidate + centerOffset;
                    var extent = Vector3.up * (halfHeight - radius);
                    bool blocked = false;
                    foreach (var obstacle in Physics.OverlapCapsule(center - extent, center + extent, radius,
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    {
                        if (!obstacle.transform.IsChildOf(player))
                        {
                            blocked = true;
                            break;
                        }
                    }
                    if (blocked)
                        continue;

                    landing = candidate;
                    return true;
                }
            }
        }
        return false;
    }
#endif

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
