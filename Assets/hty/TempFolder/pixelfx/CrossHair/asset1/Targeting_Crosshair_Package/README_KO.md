# 대형 현대형 타겟팅 크로스헤어 — PixelFX

기존 스나이퍼 구상을 대체하는 신규 디자인. 기존 파일은 보존하고 본 폴더에 독립 제작했다.
14프레임, 16 FPS, 1회 재생, 총 0.875초. 마지막 4프레임은 동일한 완성 상태로 0.25초 유지한다.

## 디자인과 실제 크기
320×180 투명 캔버스, 중심 (160,90). 주 원의 중심선 지름 112px(화면 높이 62%), 외곽 분절 장식 지름 128px. 기존 중심 조준 마커 약 14px 대비 주 원 지름 8배. 가로선 297px, 세로선 161px.
진한 빨강 테두리/선명한 빨강/주황빛 강조의 제한된 팔레트. 큰 주 원, 외곽 4분절 보조 링, 4방향 접합 마커, 좌우 짧은 프레임 장식으로 구성. 군용 거리 눈금, 숫자, 룬, 마법진, 글로우/블러는 사용하지 않았다.
1~2 약한 부분 원과 축 → 3~6 긴 십자선 생성 → 7~9 원형 결합 → 10~11 장식 마무리 → 11~14 완성 상태 유지.

## 생성 파일
Exports/Targeting_Crosshair_01.png ~ Targeting_Crosshair_14.png: 각 320×180 투명 PNG.
Exports/Targeting_Crosshair_Spritesheet.png: 960×900, 3열×5행, 셀 320×180. 좌상단부터 행 순서 첫 14칸 사용, 마지막 1칸 제외.
Exports/Targeting_Crosshair_Spritesheet.json: 좌표, 프레임 시간, pivot.
Exports/Targeting_Crosshair_Preview.gif: 투명 14프레임, 자동 반복 없음. GIF의 10ms 시간 단위로 반올림되어 총 0.88초.
Exports/Contact.png: 검토용 배경/번호 포함 프레임 모음, 게임용 아님.
Exports/Scene_Readability.png: 기존 열차 배경 이미지 위 최종 PNG의 오프라인 합성 시인성 확인용. 실제 Unity 실행 캡처가 아님.
Exports/Validation.json, GifValidation.json: 생성 검증 및 GIF 디코딩 결과.
Source/targeting-crosshair.mjs, render.mjs: PixelFX 효과와 출력 소스.

## Unity 적용
Sprite (2D and UI), Point, Compression None, Mip Maps Off, Pivot Center. 개별 프레임을 순서대로 Animation Clip에 넣고 Samples=16, Loop Time Off. 시트는 Grid By Cell Size 320×180으로 분할해 첫 14칸만 사용. JSON은 좌표 참고용이다.
인게임 오버레이로 화면 또는 타겟 중심에 배치한다. 16:9 화면에서 크게 표시하며 320×180 기준 정수배 확대를 권장한다(1920×1080에서는 6배). 작은 UI 아이콘 크기로 축소하지 않는다. 대상 추적, 상태 전환과 마지막 프레임 유지/해제는 게임 코드에서 연결한다. 게임 화면에는 개별 PNG 또는 시트만 사용한다.

## 검증/범위
PixelFX painter/renderFrames/spriteSheet/PNG exporter 사용, 3회 렌더/시각 검토. 모든 프레임 alpha 0/255만 존재, 최종 4프레임 RGBA 동일, 생성 14프레임. GIF 실제 디코딩 320×180/14프레임/0.88초 확인. 복잡한 열차 배경 합성으로 원과 축의 읽힘 확인. 이번 작업에서는 Unity 플레이 검증을 수행하지 않았다.
기존 설정/씬/에셋은 수정하지 않았다. ai-technical-art 단독 제작. 생성 경로 Assets/hty/TechnicalArt/TargetingCrosshair.
재현은 기존 MetalSparks/Source/pixelfx MIT 라이브러리를 참조해 Node로 Source/render.mjs 실행. 게임용 PNG는 별도 설치 없이 사용 가능. 마지막 프레임이 유지되는 것은 요청된 완성형 연출이며 일반 소멸 VFX의 빈 마지막 프레임 규칙을 적용하지 않는다.
