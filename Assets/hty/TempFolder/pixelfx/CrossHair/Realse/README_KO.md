# V1 크로스헤어 끄기 / 소멸

기존 V1 생성 효과의 PixelFX draw 함수를 역순으로 재사용해 장식/분절 링/십자선을 회수합니다. 끝부분은 중심 십자선만 짧게 줄인 뒤 완전히 비웁니다. 원본 V1/V2/클릭 버전은 수정하지 않았습니다.

12프레임 / 20 FPS / 0.6초 / 1회 재생. 320×180 투명 RGBA, 중심 (160,90).
첫 PNG는 TargetingCrosshair/Exports/Targeting_Crosshair_14.png와 SHA256까지 동일합니다. 마지막 PNG는 모든 픽셀 alpha=0입니다.

## 파일
Exports/Crosshair_Dismiss_01.png ~ Crosshair_Dismiss_12.png
Exports/Crosshair_Dismiss_Spritesheet.png — 960×720, 3열×4행, 셀 320×180, 좌상단부터 행 순서.
Exports/Crosshair_Dismiss_Spritesheet.json — 프레임 좌표/시간.
Exports/Crosshair_Dismiss_Preview.gif — 투명, 비반복, 12프레임, 0.6초.
Exports/Contact.png — 검토용 배경/번호 포함.
Exports/Validation.json, DecodedValidation.json — 원본 일치, 투명도와 실제 GIF 디코딩 검증.
Source/crosshair-dismiss.mjs, render.mjs — PixelFX 생성 소스.

## Unity 적용
V1 기본 상태 → 클릭 반응(필요 시) → V1 복귀 → 끄기 클립 → 비활성화 순서로 사용합니다. 클릭 반동 중 바로 이 클립으로 바꾸면 첫 프레임에서 원본 형태로 바뀌므로 반동 종료 후 재생을 권장합니다.
Sprite(2D and UI), Point, Compression=None, Mip Maps=Off, Pivot Center, V1과 동일 PPU/위치/표시 크기. Samples=20, Loop Time=Off. 마지막 투명 프레임까지 재생한 뒤 SpriteRenderer/UI Image를 비활성화하거나 투명 프레임을 유지합니다. Animator가 V1 기본 상태로 자동 복귀하지 않도록 전이를 설정하십시오. GIF 뷰어 자체 반복 설정과 게임 재생은 별개입니다.

## 검증/보존
3회 렌더/시각 검토. 생성 프레임 12개, 알파 0/255, 첫 PNG 바이트 원본 일치, 마지막 PNG 별도 디코딩 alpha 합계 0, GIF 실제 디코딩 12프레임/0.6초 확인. 주변 파티클/신규 점 장식은 없으며 기존 링 조각만 해체됩니다. Unity 실시간 플레이 검증은 이번 이미지 작업에서 미실시입니다.
신규 파일은 Assets/hty/TechnicalArt/CrosshairDismiss 하위에만 생성했고 기존 설정/씬/에셋은 보존했습니다.
Node로 Source/render.mjs를 실행해 재생성합니다. 기존 TargetingCrosshair 및 MetalSparks/Source/pixelfx 라이브러리를 참조합니다. 게임용 PNG는 별도 설치 없이 사용할 수 있습니다.
