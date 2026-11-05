# 기준 빨간 하트 파손 / PixelFX

## 동일성
이전 HeartShimmer/Exports/heart-base.png에서 실제 RGBA 픽셀을 추출하여 Source/canonical-pixels.json에 보존했습니다. 새 하트를 그리지 않고 원본 픽셀을 나누어 이동/축소했습니다. 첫 번째 PNG는 원본 PNG와 파일 바이트까지 동일합니다.
SHA256: C5DC55E45BA8ADA7088F3859D020419B8F51508CFE10785B0E568EFD1501A10C
원본 파일은 수정하지 않았습니다. 48×48px, Center pivot(0.5,0.5), 색상/외곽/크기가 그대로 연결됩니다. 소스 재생성 시 원본 해시가 다르면 중단합니다.

## 파일과 재생
- Exports/heart-break.png: 192×144 투명 RGBA 시트, 48×48 셀, 4열×3행. 위 행부터 좌→우, 첫 10칸만 사용. 마지막 여분 2칸 제외.
- Exports/heart_break_00.png ~ heart_break_09.png: 개별 프레임.
- Exports/heart-break.json: 좌표/시간 데이터.
- Preview.html: 기준 원본과 파손 애니메이션 나란히 보기, 1회 후 정지.
- Exports/contact.png: 모든 프레임 검토용. 어두운 검토 배경이 포함되어 게임용 텍스처로 쓰지 않습니다.

총 10프레임, 권장 12fps(전체 약 0.833초), Loop OFF. 프레임9 시작 시점인 0.75초부터 완전 투명입니다. 더 빠른 연출은 15fps(약 0.667초).

## 단계 (1부터 세는 프레임 번호)
1~2: 원본 하트 그대로 유지.
3~4: 중앙 지그재그 균열이 아래로 이어지며 넓어짐.
5~6: 비스듬한 파단면을 따라 여섯 붉은 조각으로 분리.
7~8: 조각이 바깥으로 흩어지며 작아짐.
9: 아주 작은 잔해.
10: 완전 투명 소멸.

## Unity 메모
Texture Type=Sprite (2D and UI), Filter=Point, Compression=None, Mip Maps OFF, Alpha=Input Texture Alpha. 개별 PNG는 Single, 시트는 Multiple/Grid By Cell Size 48×48/Pivot Center. 기존 반짝임 하트와 같은 PPU와 Transform scale을 사용하세요. 순서대로 10개 프레임을 clip에 넣고 Samples=12, Loop Time OFF. 마지막 완전 투명 프레임을 포함하고 clip 종료 후 오브젝트 비활성화 또는 풀 반환합니다. Animator가 곧바로 기본 하트 상태로 돌아가면 하트가 다시 나타나므로 숨김 상태에서 종료하세요. 일반 알파 블렌딩 Sprite Unlit 소재를 권장합니다. JSON은 기본 Unity가 자동 clip으로 읽는 형식이 아니므로 좌표 참고용입니다.

## 실제 검증
PixelFX painter/renderFrames/spriteSheet/exporter로 제작, 3회 렌더·검토·수정했습니다. 첫 PNG 파일 해시=기준 원본 해시. 각 프레임의 보이는 픽셀 수는 588,588,578,524,487,487,311,124,38,0. 모든 프레임에서 캔버스 테두리에 불투명 픽셀 0개로 끝에서 잘리는 조각이 없습니다. 마지막 프레임은 완전 투명합니다. validation.json에 동일성/투명도 측정값을 보관했습니다. Unity 내 clip 재생/성능은 미검증입니다.

Source/heart-break.mjs가 파손 로직, Source/render.mjs가 출력/검증을 담당합니다. 재생성: node Source/render.mjs. 기존 MetalSparks/Source/pixelfx의 MIT 라이브러리를 재사용합니다. 라이선스는 해당 checkout LICENSE에 있습니다. 다른 에이전트 없이 ai-technical-art 단독 제작했으며 새 HeartBreak 폴더 외 기존 에셋/씬/설정을 수정하지 않았습니다.
