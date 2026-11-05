# 날아차기 적중 + 빠른 별 반짝임 — PixelFX

## 최종 사양과 흐름
128×128 RGBA, 12프레임, 24 FPS, 0.5초, 1회 재생. 기준 접촉점 (64,64), Pivot Center.
왼쪽→오른쪽 공격을 기준으로 오른쪽 충격선을 조금 더 길게 만들었습니다. 중심은 발과 상대 몸의 충돌 위치에 놓습니다. 캐릭터/발/무기/배경은 이미지에 포함하지 않았습니다.

1: 작은 중심 섬광.
2: 황금색 외곽·선명한 노란 충격선·연노랑 중심이 날카롭게 확대.
3: 방사형 충격선 최대 확산, 작은 4각 별 7개 생성 시작(타격보다 2프레임 뒤).
4~5: 충격선이 짧아지고 사라지며 별들이 바깥으로 이동.
6~8: 충격선은 없고 별마다 크기/황금색↔노란색/흰 중심 강조가 서로 다른 박자로 바뀜.
9~11: 별이 순차적으로 작아지며 소멸. 끝에 점 형태로 잠깐 줄어듦.
12: 완전 투명.
별은 7개이며 서로 다른 각도/속도/점멸 위상/소멸 시점을 사용합니다. 블러/글로우/화염 없이 선명한 픽셀로 표현했습니다.

## 실제 생성 파일
Exports/KickImpact_01.png ~ KickImpact_12.png — 개별 투명 PNG 12장.
Exports/KickImpact_Spritesheet.png — 512×384, 4열×3행, 128×128 셀.
Exports/KickImpact_Preview.gif — 투명, 12프레임, 비반복, 총 0.5초.
Exports/KickImpact_Burst_Spritesheet.png — 타격만 분리한 4×3 시트.
Exports/KickImpact_Stars_Spritesheet.png — 별만 분리한 4×3 시트.
각 시트의 JSON — 프레임 좌표/시간/원점.
Exports/Contact.png, Stars_Contact.png — 배경/라벨 포함 검토용, 게임용 아님.
Exports/Validation.json, DecodedValidation.json, LayerValidation.json — 실제 검증 결과와 별 위치/크기/밝기 상태.
Source/kick-impact.mjs, render.mjs — PixelFX 제작/재생성 소스.

## Unity 6 적용
Sprite (2D and UI), Point, Compression None, Mip Maps Off, Alpha from Input, Pivot Center. 시트는 Grid By Cell Size 128×128로 슬라이스하고 좌상단부터 행 순서 12칸을 사용합니다. PPU는 캐릭터에 맞춰 설정합니다.
Samples=24, Loop Time Off. 충돌이 확정된 프레임에 접촉점에서 재생합니다. 마지막 투명 프레임을 유지하거나 재생 끝에 비활성화/풀에 반환하세요. 반대 공격은 SpriteRenderer.flipX로 좌우 반전합니다.
분리 시트를 사용할 경우 두 레이어를 같은 위치/크기/시각으로 동시에 시작하고 별을 타격 위에 그립니다. 별 시트 자체의 첫 두 프레임이 비어 있으므로 추가 지연을 주지 않습니다. 합본과 별도 레이어를 동시에 켜 중복 표시하지 마세요.
JSON은 좌표 참고용이며 Unity 기본 자동 임포터가 아닙니다. GIF는 10ms 시간 단위라 40/50ms를 조합하지만 전체는 0.5초입니다. 게임 재생에는 PNG와 24 FPS를 사용합니다.

## 검증
3회 렌더/시각 검토. 12프레임 및 전 프레임 같은 크기/기준점 확인. 별 첫 등장 3번, 타격 마지막 5번, 별 마지막 11번, 12번 alpha 합계 0. 모든 alpha는 0/255, 캔버스 바깥쪽 4픽셀 여백 내 불투명 픽셀 0. 별 상태 기록에서 크기와 밝기의 반복 전환을 확인할 수 있습니다. 독립 두 레이어 합성과 합본 사이 RGBA 차이 0. GIF 실제 디코딩 12프레임/0.5초 확인.
Unity Editor 실시간 충돌/재생/성능은 이번 에셋 제작에서 미검증입니다. 기존 에셋/씬/설정은 수정하지 않았습니다. 신규 경로 Assets/hty/TechnicalArt/KickImpact. ai-technical-art 단독 제작.
기존 MetalSparks/Source/pixelfx의 painter/renderFrames/spriteSheet/PNG exporter를 사용합니다. Node로 Source/render.mjs 실행 시 재생성됩니다. 게임용 PNG 사용에는 별도 설치가 필요하지 않습니다.
