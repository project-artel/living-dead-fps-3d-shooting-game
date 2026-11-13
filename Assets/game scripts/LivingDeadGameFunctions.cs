#if ARTEL_GAME_FUNCTIONS
using System.Collections;
using Artel;
using InfimaGames.LowPolyShooterPack;
using UnityEngine;
using UnityEngine.SceneManagement;

internal static class LivingDeadGameFunctions
{
    public enum Mode
    {
        Normal,
        Zombie
    }

    [ArtelCheat("메인 메뉴에서 선택한 모드의 로더를 실행하고 전투 씬이 열릴 때까지 기다립니다.",
        when: nameof(CanStartMode))]
    private static IEnumerator StartMode(Mode mode)
    {
        var menu = Object.FindAnyObjectByType<mode_menu>();
        if (mode == Mode.Zombie)
            menu.zombie_mode();
        else
            menu.normal_mode();

        var battleScene = mode == Mode.Zombie ? "zombie_level" : "Map_v2";
        while (SceneManager.GetActiveScene().name != battleScene)
            yield return null;
    }

    [ArtelCheat("현재 무기의 탄창 잔탄을 지정합니다. 탄창 용량보다 큰 값은 용량으로 제한합니다.",
        when: nameof(CanUseWeapon))]
    private static string SetMagazineAmmo([CheatRange(0, 200)] int rounds)
    {
        var weapon = Object.FindAnyObjectByType<Character>().QaEquippedWeapon;
        int target = Mathf.Min(rounds, weapon.GetAmmunitionTotal());
        int change = target - weapon.GetAmmunitionCurrent();
        if (change != 0)
            weapon.FillAmmunition(change);

        return $"rounds={weapon.GetAmmunitionCurrent()}/{weapon.GetAmmunitionTotal()}";
    }

    [ArtelCheat("전투 중 살아 있는 플레이어의 현재 체력을 지정합니다.", when: nameof(CanUseBattle))]
    private static string SetPlayerHealth([CheatRange(1, 100)] int hp)
    {
        var health = Object.FindAnyObjectByType<player_health>();
        health.health = hp;
        return $"health={health.health}";
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [ArtelCheat("살아 있는 플레이어의 무적을 켜거나 끕니다. 무적이면 피해를 받지 않으며 현재 체력은 유지합니다. 새 전투에서는 꺼집니다.",
        when: nameof(CanUseBattle))]
    private static string SetInvincibility(bool enabled)
    {
        var health = Object.FindAnyObjectByType<player_health>();
        health.QaInvincible = enabled;
        return $"invincible={health.QaInvincible}, health={health.health}";
    }
#endif

    [ArtelBehavior("현재 조준 방향으로 발사합니다. 탄창이 비면 탄약 상자 근처에서 재장전합니다.",
        group: "combat", when: nameof(CanUseWeapon))]
    private static IEnumerator FireAndReload([CheatRange(1, 200)] int maxShots, ArtelBehaviorContext context)
    {
        var scene = SceneManager.GetActiveScene();
        var character = Object.FindAnyObjectByType<Character>();
        var health = Object.FindAnyObjectByType<player_health>();
        var hitDetection = health.GetComponent<target_player>();
        var weapon = character.QaEquippedWeapon;
        int shots = 0;
        bool waitingForReload = false;

        while (shots < maxShots)
        {
            if (SceneManager.GetActiveScene() != scene || !character || !IsAlive(health)
                || !character.QaControlsAvailable || character.QaEquippedWeapon != weapon
                || GameObject.FindGameObjectWithTag("time_finish") != null
                || GameObject.FindGameObjectWithTag("enemy_finish") != null)
            {
                context.Complete($"전투 또는 무기 상태가 바뀌었습니다. shots={shots}");
                yield break;
            }

            if (weapon.HasAmmunition())
            {
                waitingForReload = false;
                if (character.TryQaFire())
                {
                    if (hitDetection)
                        hitDetection.ApplyQaShot();
                    shots++;
                    context.Report($"shots={shots}/{maxShots}, rounds={weapon.GetAmmunitionCurrent()}");
                }
            }
            else if (!waitingForReload && !character.QaWeaponBusy)
            {
                if (!character.TryQaReload())
                {
                    context.Fail($"재장전할 수 없습니다. 탄약 상자 위치를 확인하세요. shots={shots}");
                    yield break;
                }

                waitingForReload = true;
                context.Report($"재장전 중, shots={shots}/{maxShots}");
            }
            else if (waitingForReload && !character.QaWeaponBusy)
            {
                context.Fail($"재장전이 끝났지만 탄약이 보충되지 않았습니다. shots={shots}");
                yield break;
            }

            yield return null;
        }

        context.Complete($"shots={shots}");
    }

    private static string CanStartMode()
    {
        return SceneManager.GetActiveScene().name == "game" && Object.FindAnyObjectByType<mode_menu>()
            ? null : "메인 메뉴에서 실행하세요.";
    }

    private static string CanUseBattle()
    {
        var scene = SceneManager.GetActiveScene().name;
        if (scene != "Map_v2" && scene != "zombie_level")
            return "전투 씬에서 실행하세요.";

        return IsAlive(Object.FindAnyObjectByType<player_health>()) && Time.timeScale > 0
            ? null : "살아 있는 플레이어가 조작 가능한 상태여야 합니다.";
    }

    private static string CanUseWeapon()
    {
        var unavailable = CanUseBattle();
        if (unavailable != null)
            return unavailable;

        var character = Object.FindAnyObjectByType<Character>();
        return character && character.QaControlsAvailable && character.QaEquippedWeapon
            ? null : "조작할 수 있는 무기가 필요합니다.";
    }

    private static bool IsAlive(player_health health)
    {
        return health && health.health > 0 && !health.player_dead;
    }
}
#endif
