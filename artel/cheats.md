# Living Dead 준비 치트와 동작 기능

개발 빌드 1.0을 프로젝트 `PRJ-2384W8`에 연결해 아래 준비 치트 4개와 동작 기능 4개의 등록을 확인했다. 게임 내 호출과 QA는 실행하지 않았다.

| 치트 | 인자 | 효과 |
| --- | --- | --- |
| `start_mode` | `mode: Normal 또는 Zombie` | 메인 메뉴에서 해당 모드의 로더를 실행하고 전투 씬까지 기다린다. |
| `set_magazine_ammo` | `rounds: 0~200` | 현재 무기의 탄창 잔탄을 지정하며 탄창 용량으로 제한한다. |
| `set_player_health` | `hp: 1~100` | 살아 있는 플레이어의 현재 체력을 지정한다. |
| `set_invincibility` | `enabled: true 또는 false` | 플레이어 피해를 차단하거나 원래 피해 처리를 재개한다. 현재 체력은 변경하지 않는다. |

`set_invincibility`는 살아 있는 플레이어가 있는 Normal/Zombie 전투에서 시간이 흐르는 동안 사용할 수 있다. `true`로 켜면 `player_health.TakeDamage`가 체력 감소·피격 음향·사망 처리 전에 반환한다. `false`로 끄면 기존 피해 경로로 돌아간다. 이미 죽은 플레이어를 되살리지 않으며 제한 시간 만료도 막지 않는다.

무적 상태는 플레이어 인스턴스의 비직렬화 속성으로 저장하므로 기본값은 꺼짐이다. 새 전투의 플레이어에게 유지되지 않는다. 속성·피해 차단·치트 선언 모두 `ARTEL_GAME_FUNCTIONS`와 `UNITY_EDITOR || DEVELOPMENT_BUILD` 조건이 만족할 때만 컴파일된다. 일반 배포 빌드에는 포함되지 않는다.

피격·사망을 검사하는 TC-015/016은 무적을 끈 상태에서 실행해야 한다. 무적 치트는 다른 기능의 시작 조건을 준비하는 용도로 사용하고, 생존이나 피해 차단 자체를 시험할 때 결과를 대신 만들지 않는다.

동작 기능은 `aim_at_enemy`, `aim_at_nearest_enemy`, `move_to_target`, `fire_and_reload`다. 사격 입력 자체를 검사할 때는 `fire_and_reload`로 대체하지 않는다.

확인 범위: Unity 2022.3.62f3 Windows Development Build 성공, SDK 기능 목록에서 `set_invincibility(enabled: bool)` 수신 확인. 실제 피해 차단·해제·씬 재시작 동작과 일반 배포 빌드는 실행 검증하지 않았다.
