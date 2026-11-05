# 빨간 하트 회복 — PixelFX

## 원본과 동일성
기존 반짝임 작업에서 확정한 실제 원본은 `../HeartShimmer/Exports/heart-base.png`입니다. 요청 명칭 Heart_Base.png는 Exports 폴더에 바이트 그대로 복사한 같은 이미지입니다. 새 하트를 그리지 않았습니다. 원본 PNG를 디코딩한 픽셀 좌표/색상을 PixelFX painter로 재사용했습니다.

48×48, 중심 Pivot (0.5, 0.5), 기존 크기/외곽선/좌표 고정. 10번 프레임과 기준 PNG는 RGBA 픽셀 차이 0이며 파일 SHA256도 같습니다:
`C5DC55E45BA8ADA7088F3859D020419B8F51508CFE10785B0E568EFD1501A10C`
따라서 최종 해상도, 실루엣, 픽셀 배치, 외곽선, 색상, 중심 위치, 크기가 모두 동일합니다.

## 연출 및 재생
10프레임 / 15 FPS / 1회 재생 / 10÷15 = 약 0.6667초.
1~2: 매우 어두운 기존 하트에 회복 시작.
3~6: 불규칙한 작은 분홍빛 경계를 따라 아래→위로 붉은색이 차오름.
7~8: 윗부분까지 복원.
9: 표면의 작은 흰색 반짝임.
10: 모든 추가 효과가 사라지고 기준 원본과 정확히 동일.

파손 마지막의 투명 상태에서 어두운 하트가 회복되는 시작 상태로 연결할 수 있습니다. 회복 마지막은 기준 하트이며 반짝임의 첫 프레임과 연결 가능합니다. 실루엣의 스케일/회전/중심 이동은 없습니다. 추가 점광은 잠깐만 등장합니다.

## 파일
- Exports/Heart_Base.png — 기준 원본의 동일 사본
- Exports/Heart_Heal_01.png ~ Heart_Heal_10.png — 투명 RGBA 48×48, 총 10장
- Exports/Heart_Heal_Spritesheet.png — 192×144, 4열×3행, 셀 48×48, 좌상단부터 행 순서의 첫 10칸 사용
- Exports/Heart_Heal_Spritesheet.json — 프레임 좌표와 15 FPS 정보
- Exports/Heart_Heal_Preview.gif — 투명 배경, 10프레임, 자동 반복 없음, 총 0.67초
- Exports/Contact.png — 검토용 배경과 라벨 포함, 게임용 시트가 아님
- Exports/Validation.json, DecodedValidation.json — 생성 및 독립 PNG 디코딩 비교 결과
- Source/heart-heal.mjs, render.mjs, canonical-pixels.json — PixelFX 효과 및 재생성 소스

GIF는 1/100초 단위만 지원하므로 60/70ms를 조합합니다. Unity에서는 정확한 15 FPS를 사용하십시오. GIF 뷰어의 자체 반복/프레임 지연 정책은 게임용 PNG와 무관합니다.

## Unity 적용
PNG: Sprite (2D and UI), Point, Compression None, Mip Maps Off, Alpha from Input. Pivot Center. PPU는 기존 HeartShimmer와 동일하게 설정합니다.
개별 PNG를 순서대로 Animation Clip에 넣고 Samples=15, Loop Time=Off. 마지막 10번 프레임을 유지하거나 다음 상태 HeartShimmer로 전환합니다. 시트 사용 시 Grid By Cell Size 48×48로 잘라 첫 10칸만 사용하십시오. JSON은 좌표 참고용이며 Unity 기본 자동 임포터가 아닙니다.

## 검증과 범위
3회 렌더/시각 검토 후 최종 출력. 모든 PNG alpha 0/255만 사용, 캔버스 경계의 불투명 픽셀 0, 최종 원본 픽셀 차이 0, GIF 실제 디코딩 10프레임/0.67초 확인. 원본/기존 반짝임/파손/설정/씬을 수정하지 않았습니다. 이번 요청은 이미지 에셋 제작이며 Unity Editor 재생은 새로 검증하지 않았습니다.
재생성은 기존 MetalSparks/Source/pixelfx 로컬 라이브러리를 참조하여 Node로 Source/render.mjs를 실행합니다. 별도 패키지 설치는 없습니다. 소스의 duration .6은 마지막 표본 시각이며, 10번째 프레임의 표시 시간까지 포함한 클립 길이는 10/15초입니다. 일반 소멸 VFX와 달리 마지막에 기준 하트가 남는 것은 이번 요청의 의도된 동작입니다.
