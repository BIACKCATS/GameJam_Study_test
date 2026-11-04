# 캐릭터 노멀맵·동적 조명 테스트 총괄 보고

판정: **효과 구현 및 독립 QA 기능 PASS. 전체 clean 인수와 본편 통합 승인은 보류.**

## 제출 위치와 협업

- 제출 Worktree: `D:/orca/workspaces/GameJam_Study_test/ai-management`
- 기존 Run: `run_2f84e9f89ac2`
- 개발: Task `task_86229b9e5822`, Dispatch `ctx_43f81724c5d2`
- QA: Task `task_c0a3d0ef3db0`, Dispatch `ctx_735e5a5d0b23`
- 두 worker_done의 Task/Dispatch/발신 세션을 검증하고 수신 확인했다. 두 세션은 사용자 요청으로 retained 처리했다. 새 Codex 세션, Git 브랜치, 병합, 푸시는 생성/실행하지 않았다.
- 최초 사전 조사 Task와 이전 완료 Dispatch는 기존 Run에 보존했다. 사용량 중단 뒤에도 같은 세션과 활성 Dispatch로 재개했다.

## 사전 조사와 구현

Unity `6000.4.5f1`, URP `17.4.0`. 품질 설정이 UniversalRP를 참조하고 기본 Renderer는 Renderer2D다. 전역 GraphicsSettings의 pipeline null만 보고 Built-in으로 판단하지 않았다. 기존 SampleScene에는 Global Light 2D가 있으나 게임 캐릭터는 연결되어 있지 않았다. 전역 파이프라인/패키지는 변경하지 않았다.

선정 원본은 프로젝트에 포함된 Febucci Text Animator 샘플이다:
`Packages/com.febucci.text-animator-unity/Samples~/Use case examples/DamageNumbers/Assets/Textures/PlayerSpriteSheett.png`

이는 실제 저장소의 파란 픽셀 캐릭터이며 프로젝트 고유의 본편 아트라고 주장하지 않는다. 샘플 원본 PNG와 메타데이터는 읽기 전용으로 조사했다. 원본 샘플 씬의 캐릭터는 Sprite-Lit-Default를 참조하며 secondary normal은 없었다.

- 원본 및 복사 diffuse: 96×16, 16×16 여섯 프레임, x=0/16/32/48/64/80, y=0, 중앙 pivot, PPU16, Point, 밉맵 없음.
- 테스트 normal: 같은 96×16과 프레임 배치, 원본 알파 유지, NormalMap/sRGB false/Point/밉맵 없음/비압축.
- Sprite Editor 데이터 공급자와 TextureImporter API로 복사 diffuse의 Secondary Textures에 `_NormalMap`을 연결했다. UI를 수동 클릭한 작업이라고 표현하지 않는다.
- 전용 `Character_SpriteLit.mat`, 지역 Point Light 2D, Accurate normal quality, distance1.4, 위치 반경1.35, intensity0.65, ambient0.25를 사용한다.
- 테스트 카메라는 PPU16, 논리160×120, Windowbox/Point이며 캡처는640×480 정수4배다. 이것은 본편 전역 표준이 아니다.
- ON/OFF는 같은 Lit 머티리얼·광원 조건에서 normal quality Accurate/Disabled만 전환한다. Unlit 비교로 대체하지 않았다.

노멀은 머리·머리 덮개·몸통·옷·팔다리 영역의 방향을 완만하게 지정한 **스타일 근사**다. RGB 밝기에서 실제 깊이를 복원하지 않았다. 중립 평면과 작은 기울기를 혼합했다. 효과는 확산 조명의 명암 변화이며 금속성/거칠기 기반 specular 반사가 아니다. 기본 Sprite Lit으로 목표를 충족하여 커스텀 Shader Graph는 추가하지 않았다.

## 산출물과 실행

| 종류 | 상대 경로 |
|---|---|
| normal PNG | `Assets/hty/Textures/Player_Normal.png` |
| diffuse 복사본 | `Assets/hty/Textures/Player_Diffuse.png` |
| 전용 머티리얼 | `Assets/hty/Materials/Character_SpriteLit.mat` |
| 독립 씬 | `Assets/hty/Scenes/CharacterNormalLighting_Test.unity` |
| 광원·프레임 제어 | `Assets/hty/Scripts/CharacterNormalLighting.cs` |
| 네 방향 확대 비교 | `Assets/hty/Reports/Comparison_Frame0.png` |
| QA 최종 보고 | `Assets/hty/QA/FinalQA_KO.md` |
| 전체 제출 파일 목록·범위 밖 변경 | `Assets/hty/Reports/DeliveryFiles.txt` |

이 산출물이 있는 Worktree의 Unity Editor에서 독립 씬을 열고 Play한다. 방향키로 광원 방향, N으로 노멀 ON/OFF, O로 느린 회전, Space로 애니메이션 정지/재생, 쉼표/마침표로 이전/다음 프레임을 선택한다. 기존 게임 씬이나 빌드 씬 목록에 추가할 필요는 없다.

개발 캡처60장과 독립 QA 캡처74장, 측정 CSV, Console 이력, 원본 및 제출 해시가 포함된다. 제작본 FROZEN marker와 원본 manifest는 유지했다. 개발 payload163항목과 QA payload188항목을 총괄 복사 후 해시 검증했으며 불일치는 없었다. 각 manifest는 자기 자신의 재귀 해시를 제외한다.

## 검증 결과

| 항목 | 결과 |
|---|---|
| 원본과 diffuse 바이트·RGBA 일치 | PASS |
| normal 크기·알파·모든6프레임 분할·피벗 | PASS, alpha mismatch0 |
| 실제 Renderer2D와 secondary normal 사용 | PASS |
| 6프레임×4방향×ON/OFF, 총48조건 | PASS,24/24쌍 차이 |
| 반대 방향의 표면 명암 분포 변화 | PASS, 모든 프레임 양·음 변화, OFF 반대방향 차이0 |
| 고정 프레임360도 궤도 | PASS,0도와360도 픽셀차이0 |
| 자연 애니메이션·느린 회전 | PASS,0→5→0 두 주기, 약0.15초/프레임 및0.35rad/s |
| 입력 경로 | PASS, 실제 runtime Update가 임시 Input System 키 이벤트 처리 |
| 픽셀·외곽선 | PASS,4배 조건의4×4블록 불균일0, 육안검수에서 테두리 번짐·피벗 점프 없음 |
| 개발60장과 QA 대응60장 재현 | PNG SHA256까지 동일 |
| QA 최종 컴파일·Console | 컴파일 실패0, Console 오류0, Account API 경고1 |
| 보호 파일 완전 무변경 | 보류: 아래 자동 설정 변경 존재 |
| 다른 해상도·플랫폼·실행파일 빌드·성능 벤치마크 | 미실행 |

독립 QA는 Intel Arc140T / Direct3D12 / Unity6000.4.5f1에서 수행했다. 직접 누른 물리 키보드, 비정수 배율·다른 화면비·카메라 이동까지 일반화하지 않는다. 개발 중 Search 인덱스 예외, 초기 자체/QA API 오류와 Account 경고 이력을 삭제하지 않았다. 자체 API 오류는 수정 후 재검증했다. 현재 QA Console 오류0이라는 결과와 과거 개발 Console 오류 이력은 구분한다.

## 쓰기 범위 감사와 승인 대기

총괄 ai-management는 최종 검사에서 `Assets/hty/**`와 최초 `Assets/hty.meta`만 신규다. 관리 Worktree의 공유 설정·기존 자산은 변경하지 않았다.

개발·QA는 최초 Editor 실행 때 자동 변경을 발견했고 원복도 쓰기 범위 밖이므로 임의로 복구/삭제하지 않았다. 작업 시작 Git 상태는 양쪽 모두 clean이었다.

| Worktree | 남아 있는 범위 밖 경로 |
|---|---|
| ai-development | ProjectSettings/ProjectSettings.asset, EditorBuildSettings.asset, ShaderGraphSettings.asset, 신규 .vsconfig, 신규 ProjectSettings/SceneTemplateSettings.json |
| ai-testing | ProjectSettings/ProjectSettings.asset, EditorBuildSettings.asset, ShaderGraphSettings.asset, 신규 .vsconfig |

ProjectSettings.asset의 실질 diff는 Android 기본 아이콘 항목 자동 추가(+93/-1)이며 나머지 tracked 설정은 의미 diff 없는 줄바꿈 정규화다. 전체 정확한 Worktree별 목록은 DeliveryFiles.txt에 기록했다. 사용자에게 자동 변경만 검증 종료 후 복구하는 예외를 요청했으나 아직 명시적 승인이 없다. **일반적인 작업 재개 지시를 범위 밖 복구 승인으로 해석하지 않았다.**

Packages, 기존 Assets/Scenes, Assets/Settings와 원본 PNG/meta는 QA 기준 해시가 유지됐다. 원본 `D:/Project/GameJam_Study_test` Editor를 제어하지 않았으며 기존 캐릭터 프리팹·본편 씬에 적용하지 않았다.

## 성능·유지보수 및 별도 통합안

96×16 RGBA32 한 장의 원시 데이터는6144바이트다. 이는 GPU 실측 메모리가 아니며 normal pass와 Light2D 렌더타깃·오버드로우 비용은 추가된다. 프레임 시간/GPU 메모리/플랫폼 벤치마크는 미실행이다. runtime은 URP17의 private normal-quality 필드를 사용하므로 패키지 업그레이드 때 재검증해야 한다.

본편 통합은 별도 승인 후 실제 캐릭터 담당자가 수행하도록 제안한다. 확정된 본편 스프라이트·PPU·프레임 배치에 맞춰 normal을 다시 검수하고, 승인된 프리팹 복사본에 전용 Lit 머티리얼과 secondary normal을 연결한다. 실제 씬에서 광원 수·sorting layer·노멀 강도·정수 배율·성능을 비교한 뒤 공유 씬 변경을 따로 승인한다. 이번 작업은 그 통합을 실행하지 않았다.

기술 참고: [Unity 공식 Secondary Textures 지침](https://docs.unity.com/en-us/engine/6000.3/manual/unity2d/2d-urp/2d-index/secondary-textures). 실제 버전의 연결과 동작은 로컬 URP 소스와 위 독립 렌더로 검증했다.
