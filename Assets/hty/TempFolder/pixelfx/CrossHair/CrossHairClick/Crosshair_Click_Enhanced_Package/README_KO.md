# V1 크로스헤어 클릭 반응 — 강화판

기준 원본은 TargetingCrosshair/Exports/Targeting_Crosshair_14.png(V1)입니다. V2 디자인으로 바꾸지 않았으며 기존 원본/이전 클릭 버전도 보존했습니다.

## 강화한 클릭감
1 기본 상태 → 2 중심 충격 시작 및 수축 → 3 최대 9% 수축, 날카로운 중심 십자 섬광, 링의 4방향 접점 강조 → 4 최대 4% 반동과 확산 링 → 5~6 바깥 링으로 충격 전달 및 안정화 → 7~8 V1 원본 복귀.
충격 링은 중심에서 반경 10→20→35→49→57px로 확산한 뒤 사라집니다. 밝은 강조는 최고 반응 프레임에 집중됩니다. 총기/탄피/총알/궤적/화염/폭발/주변 점 파티클은 없습니다.

## 사양
8프레임, 24 FPS, 약 0.3333초, 1회 재생. 320×180 투명 RGBA, 중심(160,90), Point 픽셀 표현.
첫/마지막 PNG는 기준 V1과 SHA256까지 동일합니다. 최종 형태/색상/실루엣/위치는 정확히 복귀합니다. Validation.json 참조.

## 파일
Exports/Crosshair_Click_01.png ~ Crosshair_Click_08.png
Exports/Crosshair_Click_Spritesheet.png — 960×540, 3열×3행, 셀 320×180, 위 행부터 좌→우 첫 8칸. 마지막 빈 칸 제외.
Exports/Crosshair_Click_Spritesheet.json — 프레임 좌표와 시간.
Exports/Crosshair_Click_Preview.gif — 투명, 비반복, 8프레임, GIF 시간 양자화로 0.33초.
Exports/Contact.png — 검토용 배경/번호 포함 프레임 모음.
Exports/Scene_Click_Peak.png — 기존 열차 이미지와 최고 반응 프레임의 오프라인 합성. Unity 실행 캡처가 아님.
Exports/Validation.json, GifValidation.json — 생성 검증과 실제 GIF 디코딩 결과.
Source/crosshair-click.mjs, render.mjs — PixelFX 소스.

## 게임 적용
V1 기본 상태에서 클릭 시 클립을 재생하고 종료 후 V1 상태로 돌아갑니다. Sprite(2D and UI), Point, Compression None, Mip Maps Off, Pivot Center, V1과 같은 PPU/위치/크기. Samples=24, Loop Time Off.
첫 프레임은 기본 상태이므로 반응 시작은 약 42ms 뒤입니다. 클릭 즉시 반응을 보여야 하는 게임에서는 기본 이미지를 이미 표시하고 있으므로 재생을 두 번째 프레임부터 시작할 수 있습니다(그 경우 길이 7/24초). 반복 클릭 재시작 정책/마우스 입력 코드는 게임에서 연결합니다.

## 검증/보존
3회 렌더/시각 검토 및 열차 배경 합성 확인. 모든 프레임 alpha 0/255, 8프레임 출력, 첫/마지막 PNG 바이트 동일 검증. GIF 실제 디코딩 8프레임/0.33초 확인. Unity 실시간 입력/재생은 이번 이미지 작업에서 미검증입니다.
신규 출력은 Assets/hty/TechnicalArt/CrosshairClickEnhanced 하위에만 생성. 기존 씬/설정/에셋 미수정. Node 재생성은 기존 TargetingCrosshair 및 MetalSparks/Source/pixelfx 라이브러리를 참조합니다.
