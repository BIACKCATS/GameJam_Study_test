# 바닥 충돌 · 함몰 · 균열 — PixelFX

160×128 투명 RGBA, 12프레임, 20 FPS, 총 0.6초, 1회 재생.
충돌 기준점은 이미지 좌상단 기준 (80,80)이며 모든 프레임과 레이어가 이 좌표를 공유합니다.

## 연출
01: 투명한 충돌 직전 상태(기존 게임 바닥이 그대로 보임).
02: 짧고 강한 냉회색 접촉 섬광과 작은 함몰.
03~04: 안쪽의 어두운 벽면/바닥이 드러나며 함몰 확대, 각진 균열 급속 확산.
05: 비대칭 균열과 가지 균열이 최대 범위에 도달.
06~08: 청회색 석재 파편이 튀고 작아지며 소멸.
09~12: 파편 없이 최종 함몰과 균열 상태 유지.

중심은 어두운 틈, 그 안쪽의 중간 청회색 벽면과 바닥, 일부 들린 밝은 깨진 테두리로 높이 차를 표현했습니다. 균열은 길이와 굵기가 다른 8갈래 주 균열과 3갈래 가지 균열입니다. 열기/화염 없이 차가운 콘크리트·석재 색상을 사용했습니다. 캐릭터는 포함하지 않았습니다.

## 생성 파일
Exports/Ground_Impact_01.png ~ Ground_Impact_12.png — 투명 PNG 12장.
Exports/Ground_Impact_Spritesheet.png — 640×384, 4열×3행, 셀 160×128. 좌상단부터 행 순서.
Exports/Ground_Impact_Preview.gif — 투명 단일 재생 미리보기, 12프레임/0.6초.
Exports/Ground_Impact_FinalCrack.png — 최종 함몰·균열 정지 이미지, 12번 프레임과 PNG 바이트 동일.
Exports/Ground_Impact_Debris.png — 파편 1프레임 분리 이미지(04번 상태).
Exports/Ground_Impact_Crack_Spritesheet.png — 함몰·균열만 분리한 12프레임 시트.
Exports/Ground_Impact_Debris_Spritesheet.png — 접촉 섬광·파편만 분리한 12프레임 시트.
각 시트의 JSON — 프레임 좌표/시간/픽셀 원점.
Exports/Ground_Impact_FloorPreview.png — 청회색 바닥 위 오프라인 합성 확인용. 게임용 투명 이미지가 아닙니다.
Exports/Contact.png — 배경과 번호가 포함된 검토용 이미지.
Exports/Validation.json, LayerValidation.json, DecodedValidation.json — 생성/레이어/실제 GIF 디코딩 검증.
Source/ground-impact.mjs, render.mjs — PixelFX 제작과 재생성 소스.

## Unity 적용
Sprite (2D and UI), Point, Compression None, Mip Maps Off, Alpha Source=Input. 시트는 160×128 Grid로 분할합니다.
Unity의 Custom Pivot은 **(0.5, 0.375)**입니다. 접촉점이 이미지 아래쪽에 있으므로 기본 Center Pivot과 다릅니다. 픽셀 원점 (80,80)은 좌상단 기준이며 Unity Pivot은 좌하단 기준으로 변환한 값입니다.
12프레임을 Samples=20, Loop Time Off로 재생하고 최종 균열 이미지를 유지합니다. 재생 끝에 자동으로 지우거나 첫 투명 프레임으로 돌아가면 파손 흔적이 사라집니다. 사용 후 파손 자국을 지우는 시점은 게임에서 별도로 결정하세요.
균열은 바닥보다 위/캐릭터보다 아래, 파편은 캐릭터보다 위에 그릴 수 있습니다. 분리 시트는 같은 시간/크기/기준점에서 동시에 시작해야 합본과 같습니다. 픽셀 아트 바닥 위에서 정수배 확대를 권장합니다. JSON은 좌표 참고용이며 Unity 기본 자동 임포터가 아닙니다.

## 검증과 범위
3회 렌더/시각 검토 후 생성 과정을 재실행했습니다. 캔버스 바깥쪽 4픽셀 여백에 불투명 픽셀 0, alpha는 0/255만 존재, 파편은 09번부터 없음, 마지막 4프레임 동일. 분리 레이어 합성과 합본의 RGBA 차이 0. 실제 GIF 디코딩 12프레임/160×128/0.6초. 최종 균열 PNG와 12번 프레임은 바이트 동일합니다.
청회색 바닥 합성 확인은 오프라인 이미지 검토이며 Unity 실행 캡처가 아닙니다. Unity 실시간 재생/성능은 이번 에셋 제작에서 미검증입니다.
신규 경로 Assets/hty/TechnicalArt/GroundImpact에서만 작업했습니다. 기존 에셋/씬/설정은 변경하지 않았습니다. Node로 Source/render.mjs를 실행하면 기존 MetalSparks/Source/pixelfx의 painter/renderFrames/spriteSheet/PNG exporter를 사용해 재생성됩니다.
