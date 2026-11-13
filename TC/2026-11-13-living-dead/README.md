# Living Dead TC 등록 결과

[ARTEL TC 목록](https://home.stage.artel.kr/projects/54/test-cases)

- 프로젝트: Living Dead (`PRJ-2384W8`, API ID 54)
- 대상: 개발 빌드 1.0 (`BLD-0NX0W1`, API ID 50), 인스턴스 119
- TC: `TC-001`~`TC-020` / API ID `3107`~`3126`, 등록 당시 DRAFT, 이후 Test Run 생성 시점의 서버 조회에서는 모두 READY, 폴더 미지정
- 현재 소스에는 무적·순간이동을 포함한 준비 기능 5개·동작 기능 3개가 있다. `move_to_target`의 직선 이동은 제거했다.
- 입력 매크로 12개·판정용 값 7개·게임 규칙 7개를 등록했다.
- 범위: Core gameplay flows. 기능 17개·예외 3개이며 전체 콘텐츠 전수 검사가 아니다.

| 구분 | 기능 | 예외 | 내용 |
| --- | ---: | ---: | --- |
| 핵심: 모드·이동 | 3 | 0 | Normal, Zombie, 전진 |
| 핵심: 무기·탄약 | 4 | 1 | 발사, 전환, 부분·빈 탄창 재장전, 거리 제한 |
| 핵심: 회복 | 1 | 2 | 회복, 최대 체력 보존, 거리 제한 |
| 핵심: 수류탄 | 2 | 0 | 보충, 투척 후 보유 수 감소 |
| 핵심: 전투 결과 | 4 | 0 | 처치, 피격, 사망, 재도전 |
| 주변: 시작·일시정지 | 3 | 0 | 모드 선택 열기, 정지, 해제 |

등록 후 각 TC를 단건 API로 읽어 본문·종류·도착 화면·관찰값·초안 상태·폴더를 비교했고 버전 소속도 조회했다. 매크로는 서버 검사 및 저장한 소스의 일치를 확인했다. 판정용 값·규칙은 저장 후 모든 요청 필드를 비교했다. 검증 결과는 JSON 파일에 보관했다.

사용자 요청에 따라 게임 조작과 QA는 실행하지 않았다. 20개 모두 실제 입력·관찰 경로 확인이 필요한 초안이다. 순간 이펙트, 정확한 부위별 피해, 전체 적 처치 후 승리와 시간 만료는 이 20개에 포함하지 않았다. `player_health.TakeDamage`의 치명타 뒤 추가 피해 호출이 필요할 수 있는 사망 처리 결함 후보도 미수정이다.

추가한 `set_invincibility(enabled)`는 현재 체력을 유지하면서 피해 처리를 차단한다. 피격·사망 TC-015/016에서는 꺼야 한다. [치트 목록과 사용 조건](../../artel/cheats.md)에 범위를 기록했다.

ARTEL 서비스 규칙상 시나리오 작성과 QA에는 사람이 콘솔에서 승인한 READY TC가 필요하다. 이후 TC 20개의 READY 상태를 조회하고 [ORDERED·FREE Test Run 두 개](test-runs.md)를 만들었다. 시나리오 6개는 검토 대기 DRAFT이며 QA 런은 만들지 않았다. 준비된 입력 매크로는 실행 성공의 증거가 아니며, 전후 비교가 필요한 6개 매크로는 그 한계를 계약에 명시했다.

요청과 서버 응답:

- [등록한 TC 내용](test-cases.md)
- [등록 요청](registration-request.json), [단건 읽기 결과](registration-result.json), [버전 소속](version-result.json)
- [지원 요소 읽기 결과](support-result.json)
- [입력 매크로](../../artel/macros.json), [판정용 값·규칙](../../artel/facts-rules.json)

개발 빌드는 `.artel/development/LivingDead.exe`, 빌드 로그는 `.artel/development-build.log`에 있다. `Assets/Editor/LivingDeadArtelBuild.cs`가 활성화된 Build Settings scene만 포함해 Development Build를 만든다. 이번 빌드는 로컬 SDK checkout을 임시 참조했으므로 일반 git URL이 가리키는 SDK와 동일하다고 단정하지 않는다.

`register.py`는 환경 변수 `ARTEL_TOKEN`을 사용해 등록·비교하며, 동일 본문을 재사용해 중복 생성을 피한다. `register-support.py`는 저장소의 JSON 선언을 등록한다. 두 스크립트는 게임 입력이나 QA를 실행하지 않는다. 토큰은 파일에 저장하지 않는다.
