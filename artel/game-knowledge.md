# Living Dead 게임 분석

버전: ARTEL 등록 1.0, `BLD-0NX0W1` (API ID 50). 소스 commit: `94d777bf6f3a4d4bfdcb56e9757d4f0303983660` 및 현재 작업 디렉터리의 미커밋 변경 포함. 분석일: 2026-11-13. Unity 2022.3.62f3와 로컬 `artel-sdk/Packages/kr.artel.sdk`로 개발 빌드를 생성했다. 패키지 설정은 빌드 전 내용으로 복원했다. 게임 동작과 입력은 사용자 요청에 따라 확인하지 않았다.

| 기능 | content role | concrete places | 조건 | 근거 |
| --- | --- | --- | --- | --- |
| 시작 화면 | 최초 메뉴 | `game` | Build Settings에서 처음 활성화된 scene | `ProjectSettings/EditorBuildSettings.asset` |
| 모드 시작 | 모드 선택 | Normal → `level_loader` → `Map_v2`; Zombie → `level_2_loader` → `zombie_level` | `slider.value==slider.maxValue`일 때 전투 scene 비동기 로드. maxValue=300, 프레임마다 progress+=1 | `mode_menu.normal_mode/zombie_mode`, `level_loader.loadasyn`, `level_2_loader.loadasyn` |
| 이동 | 전투 | Normal 대표 | Movement 키보드 바인딩 W/S/A/D. 실제 충돌·위치 변화는 confirm by running | `Assets/Infima Games/Low Poly Shooter Pack - Free Sample/Input/IA_Player.inputactions` |
| 발사 | 전투 | 현재 장착 무기 | `!cursorLocked`이면 반환. `CanPlayAnimationFire()`, `equippedWeapon.HasAmmunition()`, 발사 간격 및 자동/단발 분기 | `Character.OnTryFire`; 같은 Input Actions의 Fire=`<Mouse>/leftButton` |
| 무기 전환 | 전투 | 인벤토리 | `cursorLocked`, `inventory != null`, `CanChangeWeapon()`, `indexCurrent != indexNext` | `Character.OnTryInventoryNext`; Inventory Next=`<Keyboard>/q` |
| 재장전 | 탄약 상자 접근 가능한 전투 | tag `bullet_kit` | `Vector3.Distance(player.transform.position, kit.transform.position) >= 2.5f`인 상자는 건너뜀. 재장전 키 handler에서 커서 잠금 및 `CanPlayAnimationReload()` 필요 | `Character.TryStartReload`, `OnTryPlayReload`, `AnimationEndedReload`; Reload=`<Keyboard>/r` |
| 빈 탄창 경로 | 재장전 | 현재 장착 무기 | `equippedWeapon.HasAmmunition() ? "Reload" : "Reload Empty"` | `Character.TryStartReload` |
| 메디킷 | 회복 가능한 전투 | tag `medikit` | `dis<2.5f && health!=max_health && Input.GetKeyDown(KeyCode.CapsLock)`; max_health=100. `health=100f`, `Destroy(used_kit)` | `Assets/game scripts/player_health.cs`, `Update` |
| 수류탄 보충 | 탄약 상자 접근 가능한 전투 | tag `bullet_kit` | `dis<2.5f && Input.GetKeyDown(KeyCode.R)`이면 기존 `grenade` tag 객체 제거 및 표식 8개 생성 | `can_throw_grenade.Update` |
| 수류탄 투척 | 전투 | tag `grenade` | `Input.GetKeyDown(KeyCode.G) && ctg.can_throw==true`; tag 객체 수>0이면 can_throw=true | `throw_grenade.Update`, `can_throw_grenade.Update/destro_one` |
| 수류탄 수량 판정 | 전투 HUD | `score_kill` | `grenades.Length`를 `/8`로 표시. 투척체와 보유 표식 구분 및 제거 시점은 confirm by running. 스크립트 기본 delay=3, 폭발 후 die 예약=1초이나 prefab override는 미확인 | `score_kill.Update`, `grenade_script.Start/Update/explode` |
| 처치 수 | 전투 HUD | `score_kill` | `initial_enemies.Length-enemies.Length`를 Kill로 표시. tag=`enemy`. 제거에 따른 집계이므로 체력 0만으로 증가 확정 불가 | `score_kill.Start/Update`, `Enemy_health.TakeDamage/PlayAnim/Update` |
| 타이머 | 전투 HUD | `score_kill` | 초기 코드값 `timevalue=1200`; `timevalue>0`일 때 `timevalue-=Time.deltaTime`. 화면은 분:초 | `score_kill.Update/DisplayTime`. scene override는 confirm by running |
| 피격·사망 | 살아 있는 플레이어 | `player_health` | 호출 시작에 `health>0`이면 health-=amount. `else if(health<=0)`인 별도 호출에서 사망 coroutine 시작. `WaitForSeconds(3f)` 뒤 player_dead=true | `player_health.TakeDamage/PlayAnim`; 치명타 뒤 추가 피해 필요 가능성은 결함 후보 |
| 사망 화면 전환 | 전투 결과 | `game over`, `game over 1` | `ph.player_dead==true && time_finish==null && enemy_finish==null` | `player_dead_loadscene.Update`, `player_dead_loadscene2.Update`. 모드별 scene 컴포넌트 연결은 confirm by running |
| 재도전 | Game Over | Normal 로더, Zombie 로더 | Replay가 각각 `level_loader`, `level_2_loader` 로드 | `game_over_script.Replay`, `game_over_script2.Replay`; 실제 버튼 연결은 confirm by running |
| 정지·해제 | 전투 중 | pause menu | ESC에서 `gamepaused`에 따라 pause/resume. pause는 UI 활성+timeScale=0, resume은 UI 비활성+timeScale=1 | `pause_menu_script.Update/pause/resume`; ESC는 Input Actions의 Lock Cursor에도 연결되어 있어 실제 동시 처리 확인 필요 |

공유 사실을 기록 후 별도로 재확인했다: 모드 로더의 목적 scene과 Build Settings 이름 일치, 회복·재장전 거리의 엄격한 `<2.5f` 조건, 메디킷 회복값 100, 수류탄 보충 수 8 및 HUD의 tag 개수 계산을 각 소스에서 다시 확인했다. scene/prefab의 인벤토리·HUD·컴포넌트 연결 및 수치 override는 이 기록에서 확정하지 않는다.

현재 서비스 증거: 새 프로젝트 `Living Dead` / `PRJ-2384W8` (API ID 54), 빌드 `BLD-0NX0W1` (API ID 50), 인스턴스 119 연결 및 무적 치트를 포함한 준비 기능 4개·동작 기능 4개 수신 확인. TC 20개를 버전 1.0에 DRAFT로 등록했다(API ID 3107~3126). 매크로 12개는 서버 문법 검사와 저장 후 소스 비교를 통과했고, 판정용 값 7개·규칙 7개도 저장 후 비교했다. 실제 입력·게임 함수 호출·QA는 미실행이다. 폴더는 만들지 않았다. 이전 `PRJ-A72YY6`/`BLD-0227QX` 및 2026-11-11 파일은 이번 등록의 증거로 사용하지 않는다. 요청·결과는 `TC/2026-11-13-living-dead/registration-request.json`, `registration-result.json`, `version-result.json`, `support-result.json`에 보관했다.
