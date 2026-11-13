# 최신 SDK로 개발 빌드 갱신

- 빌드 날짜: 2026-11-13
- SDK: `develop` 조회 시 최신 커밋 `465e18c88512f858db83741fb9b96d64a658b2be`
- Unity: 2022.3.62f3, Windows 64-bit Development Build
- 출력: `.artel/development-latest-sdk/LivingDead.exe`
- 로그: `.artel/latest-sdk-build.log`
- 모듈 SHA-256 및 빌드 확인 기록: `.artel/latest-sdk-build-receipt.json`

이전 개발 빌드는 오래된 로컬 SDK checkout을 임시 참조했다. 이번 빌드는 `Packages/manifest.json`에 정확한 Git 커밋을 지정하고 Unity Package Manager가 직접 받은 패키지로 생성했다. `packages-lock.json`의 해시도 같은 커밋이다. 기존 로컬 SDK checkout과 이전 실행 파일은 수정하지 않았다.

빌드 로그의 `Build Finished, Result: Success.`와 종료 코드 0을 확인했다. 출력의 `Artel.InputSystem.dll`에 `InputSystemKeyboardBackend`·`KeyCodeToKey`, `Artel.Runtime.dll`에 `NewInputKeyboard`가 포함되는지도 확인했다. 이 구현은 이미 병합된 SDK PR #199의 키보드 Input System 전달 경로를 포함한다.

실제 W 이동, QA 재실행, 기존 `score_kill.Update` HUD 예외의 수정은 이번 재빌드 검증에 포함하지 않았다.
