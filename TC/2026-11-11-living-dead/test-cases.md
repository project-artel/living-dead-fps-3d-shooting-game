# Living Dead 모드·전투·회복 TC 20개

## 공통 실행 조건

각 TC는 독립적으로 새 실행 또는 메인 메뉴에서 시작한다. 이전 TC의 탄약·체력·처치 수를 이어받지 않는다. 전투 준비는 `game`에서 해당 모드를 선택하고 로더가 끝나기를 기다린다. 로더는 300 프레임에 걸쳐 진행되므로 고정된 몇 초만 기다리고 실패로 판정하지 않는다. 전투가 60초 안에 열리지 않으면 로더 진행 및 오류를 기록한다.

입력 대상은 해당 실행의 화면과 scene 관찰에서 다시 찾는다. UI 버튼 문구와 selector는 빌드에서 확인하며 임의의 selector를 사용하지 않는다. 전투에서 커서를 잠그고 무기를 조작할 수 있는지 확인한다. 메인 메뉴의 모드 선택 버튼을 시험하는 TC-01/02는 `StartMode`로 대체하지 않는다.

준비에 사용할 수 있는 코드상 지원 기능은 `StartMode(Normal|Zombie)`, `SetMagazineAmmo(rounds)`, `SetPlayerHealth(hp)`, `MoveToTarget(target, stopDistance)`, `AimAtEnemy(target, Body|Head)`다. `ARTEL_GAME_FUNCTIONS` 조건부 코드이므로 **대상 빌드에서 등록·호출이 확인된 경우에만** 사용한다. 없으면 실제 이동·발사·피격으로 준비하며, 재현할 수 없으면 BLOCKED로 기록한다. `MoveToTarget`은 장애물을 우회하는 경로 탐색을 보장하지 않으므로 실패 응답을 도착 증거로 쓰지 않는다.

사격 입력을 검사하는 TC는 실제 마우스 왼쪽 버튼을 사용한다. `FireAndReload`는 별도의 QA 사격 경로를 포함하므로 입력 검사의 대체 수단으로 사용하지 않는다. 회복·재장전·투척 역시 실제 키 입력으로 실행한다.

HUD의 체력 바, 탄약 숫자, 타이머, 처치 수와 실제 상태를 증거로 사용한다. SDK 값은 해당 빌드가 반환하는 것을 확인한 뒤 사용한다. 소스에 필드가 있다는 사실만으로 관찰 가능하다고 가정하지 않는다. 매 TC에서 입력 전후 screenshot, 대상 scene, 사용한 입력, 관찰값, 준비용 cheat 사용 여부를 기록한다. 입력 도구의 성공 응답만으로 게임 반응을 판정하지 않는다.

## TC-01 Normal 모드 진입 확인

- 우선순위: P0
- 상태: **Needs build verification** — 모드 선택 UI와 클릭 전달, 전투 HUD 표시 확인 필요.
- Reach: 새 실행의 `game` 메뉴에서 Normal 모드를 선택할 수 있는 화면을 연다.
- Act: Normal 모드 선택 버튼을 클릭하고 로더 완료를 기다린다.
- Judge: `Map_v2` 전투가 열리고 플레이어 시점과 체력·탄약·타이머 HUD가 표시된다. Zombie 전투가 열리거나 메뉴에 머물면 실패다. 로더의 순간적인 화면 포착은 필수 판정 조건이 아니다.
- 근거: `mode_menu.normal_mode`, `level_loader.loadasyn`, `EditorBuildSettings.asset`.

## TC-02 Zombie 모드 진입 확인

- 우선순위: P0
- 상태: **Needs build verification** — Zombie 선택 UI와 scene/HUD 관찰 확인 필요.
- Reach: 새 실행의 `game` 메뉴에서 Zombie 모드를 선택할 수 있는 화면을 연다.
- Act: Zombie 모드 선택 버튼을 클릭하고 로더 완료를 기다린다.
- Judge: `zombie_level` 전투가 열리고 플레이어 시점과 체력·탄약·타이머 HUD가 표시된다. Normal 전투로 잘못 진입하면 실패다.
- 근거: `mode_menu.zombie_mode`, `level_2_loader.loadasyn`.

## TC-03 전투 타이머 감소 확인

- 우선순위: P1
- 상태: **Needs build verification** — HUD 타이머 읽기 및 정상 배속 상태 확인 필요.
- Reach: Normal 전투 시작 직후, 적과 교전하지 않는 안전한 위치에서 타이머 T0를 기록한다. 일시정지와 슬로모션 상태가 아니어야 한다.
- Act: 입력하지 않고 실제 시간 5초를 기다린다.
- Judge: 같은 전투의 타이머 T1이 T0보다 작고, 정상 배속에서 약 5초 감소한다(화면 샘플링 허용 오차 ±2초). 형식은 분:초다. 전투 종료나 피격으로 준비 조건이 깨지면 재시도한다.
- 근거: `score_kill.Update`, `DisplayTime`.

## TC-04 ESC 일시정지와 타이머 정지 확인

- 우선순위: P0
- 상태: **Needs build verification** — ESC 입력과 pause UI, 타이머 관찰 확인 필요.
- Reach: Normal 전투의 안전한 위치에서 감소 중인 타이머를 확인한다.
- Act: ESC를 한 번 누른다. 일시정지 메뉴가 열린 뒤 타이머 T0를 기록하고 실제 시간 5초 후 T1을 기록한다.
- Judge: 일시정지 메뉴가 표시되고 T1=T0이다. 메뉴가 열리지 않았다면 타이머 불변만으로 통과시키지 않는다.
- 근거: `pause_menu_script.pause`, `score_kill.Update`.

## TC-05 ESC 재개와 타이머 재시작 확인

- 우선순위: P0
- 상태: **Needs build verification** — ESC 토글과 재개 후 입력 충돌 확인 필요.
- Reach: Normal 전투에서 ESC로 일시정지 메뉴를 열고 타이머가 정지했음을 확인한다.
- Act: ESC를 다시 한 번 누른다. 재개 직후 타이머 T0와 5초 후 T1을 기록한다.
- Judge: 일시정지 메뉴가 닫히고 T1<T0이다. 메뉴만 닫히고 전투 시간이 멈춰 있으면 실패다.
- 근거: `pause_menu_script.resume`.

## TC-06 일시정지에서 메뉴 복귀 후 새 전투 진행 확인

- 우선순위: P1
- 상태: **Needs build verification** — Tab 입력, 메뉴 복귀, 새 전투 배속 확인 필요.
- Reach: Normal 전투에서 ESC로 일시정지 메뉴를 연다.
- Act: Tab을 누른다. `game` 메뉴로 돌아온 뒤 Normal을 선택해 다시 전투에 진입한다.
- Judge: 메뉴 복귀가 되고 새 전투에서 타이머가 감소한다. 일시정지의 시간 정지 상태가 새 전투에 남으면 실패다.
- 근거: `pause_menu_script.Update`의 Tab 분기.

## TC-07 단발 입력 후 탄창 잔탄 감소 확인

- 우선순위: P0
- 상태: **Needs build verification** — Input System 발사 전달과 잔탄 HUD 읽기 확인 필요.
- Reach: Normal 전투에서 권총을 장착하고 재장전·무기 전환이 끝나기를 기다린다. 적과 폭발물이 없는 벽을 조준한다. 잔탄 N≥2를 기록한다.
- Act: 마우스 왼쪽 버튼을 짧게 한 번 눌렀다 뗀다.
- Judge: 동일 무기의 잔탄이 N-1이다. 이 TC는 탄약 소비만 판정하며 적 피해나 이펙트는 판정하지 않는다.
- 근거: `Character.OnTryFire`, `Character.Fire`, `Weapon` 탄약 처리.

## TC-08 Q 무기 전환과 원래 무기 복귀 확인

- 우선순위: P1
- 상태: **Needs build verification** — 두 무기 보유 상태, Q 바인딩과 무기 식별 확인 필요.
- Reach: Normal 전투에서 권총과 소총이 모두 있는지 확인하고 현재 무기의 모델·HUD 아이콘·잔탄을 기록한다.
- Act: Q를 한 번 누르고 전환 완료를 기다린다. 변경된 무기를 기록하고 Q를 다시 눌러 완료를 기다린다.
- Judge: 첫 입력 후 다른 무기로 바뀌며 두 번째 입력 후 원래 무기로 돌아온다. 사격이나 재장전 없이 원래 무기의 잔탄도 유지된다.
- 근거: `IA_Player.inputactions`, `Character.OnTryInventoryNext`.

## TC-09 탄약 상자 근처에서 부분 탄창 재장전 확인

- 우선순위: P0
- 상태: **Needs build verification** — 상자까지 이동, 거리·용량 관찰과 재장전 완료 확인 필요.
- Reach: Normal 전투에서 `bullet_kit` 상자 2.5m 미만으로 이동한다. 현재 무기를 발사해 0<N<C 상태를 만들고 잔탄 N과 탄창 용량 C를 기록한다.
- Act: R을 한 번 누르고 재장전 완료 후 잔탄이 안정될 때까지 최대 10초 기다린다.
- Judge: 동일 무기의 잔탄이 C로 채워진다. 애니메이션이나 입력 응답만으로 통과시키지 않는다.
- 근거: `Character.TryStartReload`, `OnTryPlayReload`, `AnimationEndedReload`.

## TC-10 빈 탄창 재장전 후 실제 발사 확인

- 우선순위: P0
- 상태: **Needs build verification** — 빈 탄창 준비와 empty reload 경로 확인 필요.
- Reach: Normal 전투의 탄약 상자 2.5m 미만에서 동일 무기의 잔탄을 0으로 준비한다. 등록된 `SetMagazineAmmo(0)`를 사용했다면 기록한다.
- Act: R을 누르고 재장전 완료를 기다린다. C발로 채워졌음을 기록하고 안전한 벽에 왼쪽 버튼으로 한 번 발사한다.
- Judge: 빈 탄창이 C로 채워지고 후속 발사 후 C-1이 된다. 재장전 후 무기가 계속 잠겨 있으면 실패다.
- 근거: `Character.TryStartReload`의 `Reload Empty` 분기.

## TC-11 상자 밖 재장전 제한과 상자 안 재장전 비교

- 우선순위: P1
- 상태: **Needs build verification** — 모든 상자와의 거리 및 R 입력 수신 증거 확인 필요. 입력 수신을 확인할 수 없으면 이 음성 판정은 BLOCKED다.
- Reach: Normal 전투에서 0<N<C를 준비하고 모든 `bullet_kit`으로부터 3m 이상 떨어진 위치를 확인한다.
- Act: R을 한 번 누르고 최대 10초 관찰한다. 같은 무기·같은 입력 방식으로 상자 2m 이내에 이동한 뒤 다시 R을 누른다.
- Judge: 밖에서는 N이 유지되고 안에서는 C로 채워진다. 두 시도의 입력 전달 기록과 거리 증거가 있어야 한다. 단순히 밖에서 잔탄이 그대로였다는 이유로 통과시키지 않는다.
- 근거: `Character.TryStartReload`의 2.5m 거리 제한.

## TC-12 메디킷 사용 후 체력 회복 확인

- 우선순위: P0
- 상태: **Needs build verification** — 메디킷 접근, CapsLock 전달, 체력 비교 확인 필요.
- Reach: Normal 전투에서 체력을 50으로 준비하고 준비가 반영됐음을 기록한다. 메디킷 2m 이내로 이동하며 공격받지 않는 상태를 확보한다.
- Act: CapsLock을 한 번 누른다.
- Judge: 다음 상태 갱신 후 체력이 100으로 회복되고 체력 바가 가득 찬다. 등록된 `SetPlayerHealth(50)`는 준비에만 사용하며 회복 결과를 직접 설정하지 않는다.
- 근거: `player_health.Update`의 medikit 분기.

## TC-13 메디킷 소비 후 같은 위치에서 재사용 방지 확인

- 우선순위: P1
- 상태: **Needs build verification** — 단일 메디킷 식별·제거 증거 및 두 번째 입력 수신 확인 필요.
- Reach: 다른 메디킷이 2.5m 안에 없는 대상을 선택한다. 체력 50에서 해당 메디킷을 CapsLock으로 사용해 100 회복을 확인한다. 대상 제거는 명시적인 제거 관찰 또는 전체 범위 재조회와 화면으로 확인한다.
- Act: 같은 위치에서 준비용 기능으로 체력을 다시 50으로 낮춘다. CapsLock을 다시 누른다.
- Judge: 소비한 메디킷이 재생성되지 않고 체력이 50으로 유지된다. 주변 다른 메디킷이나 적 피해가 개입하면 재시도한다. 두 번째 입력 전달을 확인할 수 없으면 BLOCKED다.
- 근거: `player_health.Update`의 `Destroy(used_kit)`.

## TC-14 최대 체력에서 메디킷 보존 확인

- 우선순위: P1
- 상태: **Needs build verification** — 같은 메디킷의 지속 식별과 입력 전달 확인 필요.
- Reach: Normal 전투에서 체력이 100인 상태로 메디킷 2m 이내에 선다. 주변에 다른 메디킷과 공격원이 없어야 한다.
- Act: CapsLock을 누른 뒤 해당 메디킷이 남아 있는지 확인한다. 준비용 기능으로 체력을 50으로 낮춘 뒤 같은 메디킷에 CapsLock을 다시 누른다.
- Judge: 첫 시도에서 체력 100과 메디킷이 유지되고, 두 번째 시도에서 체력 100 회복 및 해당 메디킷 소비가 확인된다. 첫 입력 수신 증거가 없으면 첫 시도의 음성 판정은 보류한다.
- 근거: `player_health.Update`의 `health!=max_health` 조건.

## TC-15 메디킷 거리 제한 확인

- 우선순위: P1
- 상태: **Needs build verification** — 전체 메디킷 거리와 CapsLock 입력 수신 확인 필요.
- Reach: Normal 전투에서 체력 50을 준비하고 모든 메디킷에서 3m 이상 떨어진 안전한 위치에 선다.
- Act: CapsLock을 한 번 누른다. 체력과 대상 메디킷을 기록한 뒤, 선택한 메디킷 2m 이내로 이동해 같은 키를 누른다.
- Judge: 먼 위치에서는 회복·소비가 없고 가까운 위치에서는 체력 100 회복이 일어난다. 먼 위치에서 입력 수신이 확인되지 않으면 회복 불가를 확정하지 않는다.
- 근거: `player_health.Update`의 `dis<2.5f` 조건.

## TC-16 수류탄 보충 후 최대 보유 수 표시 확인

- 우선순위: P1
- 상태: **Needs build verification** — R 입력과 수류탄 HUD 갱신 확인 필요.
- Reach: Normal 전투에서 탄약 상자 2m 이내에 선다. G를 사용해 수류탄 보유 수가 8보다 작은 상태를 준비한다. 기존 투척체가 사라지고 수량이 안정될 때까지 기다린다.
- Act: R을 한 번 누르고 수류탄 수량이 안정될 때까지 기다린다.
- Judge: 수류탄 HUD가 `8/8`로 표시된다. 탄창 재장전의 성공 여부는 이 TC에서 판정하지 않는다.
- 근거: `can_throw_grenade.Update`, `score_kill.Update`.

## TC-17 수류탄 한 번 투척 후 보유 수 감소 확인

- 우선순위: P1
- 상태: **Needs build verification** — 투척 후 안정된 수량과 투척체 수명 확인 필요.
- Reach: Normal 전투의 안전한 위치에서 수류탄을 보충해 `8/8`을 기록한다. 주변에 적·폭발물이 없어야 한다.
- Act: G를 짧게 한 번 누르고 뗀다. 투척체가 폭발해 제거된 뒤 수량을 확인한다.
- Judge: 최종 보유 수가 `7/8`이다. 투척체와 보유 표식이 같은 `grenade` 태그를 사용하므로 투척 직후 수량이나 잠깐 보이는 투척체로 판정하지 않는다. 제거를 확인할 수 없다면 BLOCKED로 남긴다.
- 근거: `throw_grenade.Update`, `can_throw_grenade.destro_one`, `score_kill.Update`, `grenade_script`.

## TC-18 지정 적 처치 후 Kill 카운터 증가 확인

- 우선순위: P0
- 상태: **Needs build verification** — 지정 적 조준·접근, 대상 사망/제거와 HUD 비교 확인 필요.
- Reach: Normal 전투에서 다른 공격원과 폭발물이 없는 적 한 개체를 지정한다. 총 사거리 안에서 시야를 확보하고 초기 처치 수 K 및 분모 D를 기록한다. 필요 시 등록된 이동·조준 behavior로 준비한다.
- Act: 실제 마우스 발사로 지정한 적을 처치하고 사망 처리와 제거가 끝날 때까지 관찰한다.
- Judge: 지정 대상의 처치·제거 증거와 함께 HUD가 `Kill:K+1/D`가 된다. 약 5.1초의 적 사망 처리 지연을 고려한다. 다른 적이 함께 죽으면 단일 처치 증가 판정에 사용하지 않는다.
- 근거: `target_player.shoot`, `Enemy_health.Update`, `PlayAnim`, `score_kill.Update`.

## TC-19 적 공격으로 플레이어 체력 감소 확인

- 우선순위: P0
- 상태: **Needs build verification** — 공격원 한 개체와 피격 전후 체력의 연관 확인 필요.
- Reach: Normal 전투에서 체력 100을 기록하고 공격 가능한 적 한 개체에 접근한다. 다른 적·수류탄·폭발 배럴 피해가 개입하지 않는 위치를 확보한다.
- Act: 공격하지 않고 지정 적의 공격을 받는다. 체력이 처음 감소한 뒤 공격 범위 밖으로 물러난다.
- Judge: 같은 플레이어의 체력이 100보다 낮아지고 체력 바도 감소한다. 공격원 식별이 불가능하면 해당 적의 공격 성공으로 확정하지 않는다. 정확한 단발 피해량은 이 TC의 판정 대상이 아니다.
- 근거: 적 controller의 공격 경로, `player_health.TakeDamage`, `health_bar_script`.

## TC-20 전투 중 사망 후 Game Over 및 Replay 확인

- 우선순위: P0
- 상태: **Needs build verification** — 실제 사망까지의 도달, 모드별 결과 scene 연결, Replay 버튼 확인 필요.
- Reach: Normal과 Zombie에서 각각 독립 실행한다. 적이 남아 있고 제한 시간이 충분한 상태에서 플레이어 체력을 1로 준비한다. 준비 기능이 없다면 실제 피해로 낮춘다.
- Act: 적의 실제 공격을 받아 체력이 0 이하가 되도록 하고 Game Over 화면을 기다린다. 결과 화면에서 Replay 버튼을 클릭한다.
- Judge: 사망 후 Game Over 화면이 표시되고 Replay로 같은 모드의 새 전투가 열린다. Normal은 `Map_v2`, Zombie는 `zombie_level`이어야 한다. 새 전투에서 플레이어가 살아 있고 타이머가 감소한다. 두 모드 결과를 따로 기록한다.
- 결함 후보: `player_health.TakeDamage`는 체력을 0 이하로 만드는 호출과 사망 coroutine 시작이 서로 다른 분기에 있다. 추가 피격이 없으면 사망 전환이 늦거나 멈출 가능성이 있다. **체력 0 이하 후 결과 화면 전환**이라는 기대 결과를 유지하며 실제 실행에서 판정한다. `player_dead=true` 직접 설정으로 우회하지 않는다.
- 근거: `player_health.TakeDamage/PlayAnim`, `player_dead_loadscene*`, `game_over_script*`. 결과 scene과 모드의 실제 컴포넌트 연결은 빌드에서 확인한다.

## 자동화 보류 범위

아래 항목은 위 20개와 별도의 보류 후보다.

| 항목 | 상태 | 보류 이유 및 다음 확인 |
| --- | --- | --- |
| 총구 화염·피격 이펙트·투척체 비행 모양 | Deferred | 순간 이벤트를 screenshot으로 잡는 데 의존한다. 수동 확인 대상으로 둔다. |
| 헤드샷 80과 몸통 10의 정확한 적용 피해 비교 | Deferred | 조준 behavior만으로 실제 탄착 부위와 단발 피해 귀속을 확정할 수 없다. 적 체력 회복도 비교에 개입한다. 빌드의 대상별 피격 증거가 필요하다. |
| 전 적 처치 승리 및 20분 제한 시간 만료 | Deferred | 재현 가능한 전체 처치 경로와 시간 준비 기능을 현재 실행에서 확인하지 못했다. 승리/시간 종료 표식을 직접 생성하면 시험할 동작을 우회한다. |

## 저장 전 검토 결과

20개 모두 시작 조건·실제 행동·판정 증거를 따로 검토했다. 음성 TC는 입력 미전달과 게임의 거부를 구분하도록 작성했다. 정확한 피해량이나 순간 애니메이션은 지속되는 HUD 결과로 대신 검증했다고 주장하지 않는다. 이 문서의 상태는 작성 가능성 분류이며 QA 실행의 PASS/FAIL 결과가 아니다.
