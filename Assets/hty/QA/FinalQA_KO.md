# 캐릭터 노멀맵 조명 최종 QA

검수 완료 / 기능 PASS, 전체 clean acceptance 보류.
Task task_c0a3d0ef3db0 / Dispatch ctx_735e5a5d0b23 / Run run_2f84e9f89ac2.
프로젝트 D:/orca/workspaces/GameJam_Study_test/ai-testing, 브랜치 po2zn/ai-testing.
기록된 준비 체크포인트 UTC 2026-11-04T12:27:35Z, 사용량 중단 후 동일 세션 재개 체크포인트 13:01:23Z, 최종 실행 감사 13:15:32Z.
Unity 6000.4.5f1, CLI 1.0.0-beta.13(코디네이터 지정 고정), Intel Arc 140T, Direct3D12.
자기 Editor PID49100만 사용했고 모든 명령에 정확한 ai-testing project-path를 지정했다. 재컴파일 전후 포트7802/7804는 변했지만 프로젝트·PID는 동일하다. 새 세션·worker·브랜치·merge·push는 없다.

## 입력 동결 및 전달

코디네이터 FROZEN READY 메시지 msg_20d67d7cc2b5 이후에만 개발 Assets/hty와 최초 Assets/hty.meta를 복사했다. 목적 경로가 없음을 검사하고 reparse point가 없음을 확인했으며 기존 파일을 덮어쓰지 않았다.
동결 manifest SHA256: 8fe66da9c94a6801479de8b9c6ff7207835c7de0acbae5f1dfb424d862f3f6f5.
163개 payload(메타 포함)와 manifest 자체 총164개 입력을 검수했다. 전달 직후와 실행 후 payload 불일치0. 제작 스크립트, 원본 .meta, Reports/FROZEN.txt와 제작 Reports는 수정하지 않았다.
정확한 검수 파일·해시는 TestedArtifacts.sha256에 있고 원본 동결 manifest도 함께 보존한다. 첫 해시 비교 코드의 PowerShell Matches 변수 재사용으로 전 항목을 불일치로 출력했으나, 즉시 기대해시를 별도 변수에 고정한 재검사에서163/163 일치를 확인했다; 실제 파일 불일치는 아니었다.

주요 해시:
- 원본/복사 diffuse PNG: 55ab26889c1cc4c5d2765c3c1d811909b44d8c0f0fbd7e60839ddf4903c74d7d
- 원본 PNG.meta: 0b5aef7e2264a2838006ad54d43e4fa1bfafa6fc2497e6c3fc32197d7df7e942
- normal PNG: ddcb0d38266e06df2b9b1ea118a20a95054c0df50a3d154cab6da0976f462d61
- scene: 9c0cbf842eef98f617e414e21de80561e71142ce7e2d737399c0f48e38339547
- material: 6f9f54b0c8544dd66f72b49545ad4089a83040a5fb4dfd00ec16d3e77e297558
- runtime script: 2f2ccc174246ebf93f42395de1693c33920d8b079ce95e65c2a94ca9cb2e313f

## 개별 판정

| 검수 항목 | 판정 | 독립 근거 |
|---|---|---|
| 실제 기존 캐릭터·출처 | PASS | Febucci Text Animator DamageNumbers의 PlayerSpriteSheett.png를 실제 이미지로 확인하고 코디네이터 선택대로 사용; 프로젝트 오리지널 아트라고 주장하지 않음 |
| diffuse 원본 보존 | PASS | 원본/복사 바이트 SHA256 동일,1536픽셀 RGBA 불일치0; 원본 meta 해시 유지 |
| 모든 프레임 | PASS | 실제 Sprite subasset6개, rect x=0/16/32/48/64/80,y0,16x16,PPU16,pivot(8,8) |
| normal 크기·알파 | PASS | 실제 raw PNG96x16, 원본 alpha 불일치0, 불투명 중 flat114/directional290픽셀 |
| 임포트·secondary | PASS | Sprite Multiple/Point/mip off/Uncompressed, normal NormalMap/sRGB false/Point/mip off/Uncompressed/convert false, secondary _NormalMap이 해당normal을 참조; 조사한 platform override 모두 false |
| 머티리얼·렌더 파이프라인 | PASS | 실제 currentRenderPipeline UniversalRP / scriptableRenderer Renderer2D, Sprite-Lit-Default, sortingLayer0 및 Light2D 대상0 |
| Light2D normal | PASS | Point, Accurate, normalDistance1.4,intensity0.65,color(1,.96,.9),inner2.4,outer4.5; OFF는 Disabled만 변경 |
| 좌/우/상/하 × 6프레임 × ON/OFF | PASS | 자기 Editor 실제 SRP 카메라48장,24쌍 모두 차이; Lit 머티리얼·강도는 동일 |
| 방향성 분포 | PASS | 모든 프레임의 Left-Right/Up-Down에서 양·음 변화 픽셀이 모두 존재; OFF 반대방향 차이는0 |
| 궤도 | PASS | 고정F0,0..360도30도 간격13장,0/360 출력 픽셀 차이0 |
| 자연 애니메이션·저속궤도 | PASS | 실제 endCameraRendering13샘플,0→1→2→3→4→5→0 두 주기,프레임 증가24449→24654,217.5851→219.4281초,각 전환0.1503~0.1580초; orbit2.832724→3.477772,약0.35rad/s |
| 입력 제어 | PASS | 임시 InputSystem Keyboard 이벤트를 실제 runtime Update가 처리:4방향,N OFF/ON,6단계 전진·후진 wrap,Space,O. 장치 제거·원래 keyboard 복구; 물리 키보드 수동시험은 아님 |
| 픽셀 선명도 | PASS(테스트 조건 한정) | PixelPerfectCamera PPU16/reference160x120/UpscaleRT/Windowbox/Point,640x480의 캐릭터 영역4x4블록 불균일0; 모든프레임/궤도/자연전환 크롭을 육안확인,알파 테두리·시트 번짐·피벗점프 관찰없음 |
| 제작 캡처 재현 | PASS | 제작60장과 자기 재렌더 대응60장이 PNG SHA256까지 모두 동일 |
| 최종 컴파일 | PASS | compiling=false,compilationFailed=false; QA코드 초기API 오류 수정 후 최종0 |
| Console 완전 청정 | FAIL/경고 남음 | 13:14:39Z groundTruth errors0/warnings1. Account API 접근 타임아웃 경고. 역사적 CLI command 오류·QA CS1061도 증거에 보존 |
| 보호 경로 완전 무변경 | FAIL/정리 승인 대기 | Editor 자동 설정 변경이 남음; 아래 참조. 무단 복원하지 않음 |
| 다른 해상도·플랫폼·성능·실행파일 빌드 | NOT RUN | 이번 독립 씬 검수 범위 밖; 코디네이터가 추가 빌드 불필요 지시. 독립 씬 실행을 standalone executable 빌드 성공으로 표현하지 않음 |

## 실제 렌더 방법과 정량 결과

QA/Editor/IndependentLightingQA.cs가 기존 runtime rig의 SetState를 사용하되 자체 SRP endCameraRendering에서 세 렌더 프레임 안정화 후 RenderTexture를 읽었다. 카메라 출력은 실제 Unity GPU 렌더이며 CPU 조명 합성은 하지 않았다. 캡처 동안 rig 업데이트를 정지시켜 프레임·광원을 고정했고 마지막에 복구했다.
48개의 조합과13개 궤도 총61장, Unity frame499~679(각3프레임). 별도로 자연 Update를 유지한13장 전환 캡처를 추가하여 총74장.
모든 ON/OFF 비교의 changedPixels1024~1104, total RGB delta58224~92176,0차이 실패0. 동일한 diffuse Lit 조명을 유지했으므로 Unlit 비교가 아니다.
DirectionalPixels.csv는 각프레임 반대 방향에서 밝아짐·어두워짐을 모두 기록한다. 예:F0 Left-Right 양400/음608픽셀, Up-Down 양592/음432픽셀; 단순한 전체 밝기 차이만 관찰한 것이 아니다.
4x4 screen pixel block 검사범위 x272..367,y192..287이며 픽셀 선명도 결과는640x480 정수배 조건에 한한다. 비정수창·카메라 이동·다른해상도까지 일반화하지 않는다.

시각 증거:
- AllFramesActualCrops.png: 행F0..5,열Left ON/OFF,Right ON/OFF,Up ON/OFF,Down ON/OFF. 실제 캡처 중앙96x96 크롭,조명 재계산 없음.
- OrbitActualCrops.png: 좌→우/상→하 순서0..360도,5열; 실제96x96 크롭을최근접2배 확대.
- TransitionActualCrops.png: 자연전환13장의 같은 크롭,0..5두주기와마지막0.
- Captures/*.png 전체74장 및 CaptureEvents.csv,LiveTransitions.csv.
- IndependentAudit.txt,IndependentPixels.txt,DirectionalPixels.csv,IndependentControls.txt.

노멀은 손으로 해석한 스타일화된 근사이며 실제3D 지오메트리 복원이 아니다. 육안 반응은 절제되어 있고 일부 ON 출력은 OFF보다 어둡다. 알파 실루엣·의도한 계단 경계가 유지되며 볼륨 방향 변화가 검증됐다. 이 Sprite-Lit baseline에 금속성/거칠기 기반 specular 반사 구현은 없고 이를 요구하거나 구현했다고 주장하지 않는다.

## Console 및 오류 이력

최종 groundTruth compilationFailed=false,compiling=false,consoleErrors0,consoleWarnings1.
ConsoleHistory.json을 지우지 않고 보존했다. 초기 command editor status를 한 문자열로 전달한 CLI 오류, QA script의 사용 불가 PixelPerfectCamera.filterMode 조회 CS1061은 QA 준비 중 발생했다. QA 코드만 SerializedObject의 m_FilterMode 조회로 수정하여 해소했다.
PowerShell 문자열 전달의 따옴표 제거로 몇 eval snippet 컴파일 진단도 발생했으며 파일 생성/씬 변경 실행 전 실패했다. 뒤에는 문자열 escape를 명시하여 해결했다.
제작 보고의 Search database 예외와 AI Account 타임아웃은 제작 Editor의 이력이다. 자기 Editor의 최종 경고는 Account API 타임아웃이며 공유 패키지를 수정하여 없애려 하지 않았다. 따라서 Console clean이라고 보고하지 않는다.

## 범위 및 무결성

의도적 QA 생성물은 Assets/hty/QA/** 및 그 폴더meta뿐이다. 개발 동결산출물은 승인된 복사 전달이며 수정하지 않았다. 기존 원본·공유 씬·renderer·Packages·ProjectSettings를 명시적으로 편집/저장하지 않았다.
시작Git clean이었지만 자기 Editor 최초 초기화 뒤 아래 자동변경이 생겼다:
- ProjectSettings/ProjectSettings.asset: Android 기본 아이콘 항목 +93/-1.
- ProjectSettings/EditorBuildSettings.asset: byte/newline 정규화, 의미diff없음.
- ProjectSettings/ShaderGraphSettings.asset: byte/newline 정규화, 의미diff없음.
- .vsconfig: 신규.
사용자 cleanup 승인은 아직 없으므로 그대로 보존했다. QA 리뷰가 끝났다는 사실이 정리 승인을 뜻하지 않는다. ProtectedDiff.txt/GitStatus.txt에 실제 상태를 기록했다.
ProtectedIntegrity.json의 전체파일 SHA256 집계는 Packages,Assets/Scenes,Assets/Settings가 baseline과 동일하고 ProjectSettings만 다르다. 원본PNG/PNG.meta도 baseline 동일.
최종 Play Mode stopped,임시 auto tick disabled. Editor와 Codex 세션은 유지한다.

## 한계와 재현

동결 입력에서 자기 Unity Editor를 열고 정확한 project-path를 고정한다. QA 스크립트 임포트 후 동결씬을 열고 Audit,자기 Play Mode,Begin,61장 완료후 Measure/Controls를 실행한다. ObservationSnippet.txt는 자연재생13회 실제렌더 관찰을 재현한다. 원본 FROZEN marker를 제거하거나 제작도구 Build/Begin을 실행하지 않는다. QA출력 경로가 이미 있다면 기존검수 증거를 무단덮어쓰지 말고 별도동결입력을 정한다.
사람이 직접 누른 키,플랫폼실행파일,비정수출력/다른aspect,GPU frame time/VRAM/overdraw 벤치마크는 수행하지 않았다. 픽셀 raw6144바이트/텍스처는 실제GPU메모리 측정이 아니다. URP17의 private m_NormalMapQuality 반사 필드에 의존하므로 패키지업그레이드시 재검증이 필요하다.
기능증거는충분하지만 Console 경고 및 범위밖자동설정 정리 승인 전까지 전체clean인수는 보류한다. 이 Task의 독립QA검토·상세보고는 완료했다.