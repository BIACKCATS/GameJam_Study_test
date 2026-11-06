# 기준 빨간 하트 / 반짝임 v1

## 최종 사양
- 디자인: 중앙의 밝은 빨강, 진한 빨강 테두리와 오른쪽/아래 음영, 읽기 쉬운 두 둥근 윗부분과 중앙 홈. 작은 표면 반사점과 사선 빛만 변화하며 외곽 장식은 없습니다.
- 기준 원본: Exports/heart-base.png (48×48 투명 RGBA, 중심 pivot 0.5/0.5).
- 기준 PNG SHA256: C5DC55E45BA8ADA7088F3859D020419B8F51508CFE10785B0E568EFD1501A10C
- 애니메이션: 10프레임, 10fps 권장, 1초 loop. 더 빠르게는 12fps(약 0.83초).
- 시트: Exports/heart-shimmer.png, 192×144, 48×48 셀, 4열×3행. 위 행부터 좌→우 순서로 첫 10칸만 사용하고 남은 2칸은 제외합니다.
- 개별 PNG: Exports/heart_00.png ~ heart_09.png. 좌표 JSON도 함께 제공합니다.
- Preview.html: 기준 원본과 loop를 나란히 확인. contact.png는 검토용 배경 포함 이미지로 게임용이 아닙니다.

## 향후 깨지는 하트 재사용 계약
heart-base.png를 기준 원본으로 확정합니다. 이후 깨지는 연출의 온전한 첫 프레임은 이 PNG를 그대로 사용하세요. 파편은 이 원본의 픽셀/알파를 분할하여 만들고, 하트를 새로 그리거나 다른 크기로 재생성하지 마세요. 원본 크기 48×48, 중심 pivot, 색상과 실루엣을 유지하세요. 반짝임이 없는 원본과 애니메이션 첫 프레임은 동일한 픽셀입니다. 향후 의도적 디자인 변경은 별도 버전으로 관리하세요.

## Unity 적용
Texture Type=Sprite (2D and UI), Filter=Point, Compression=None, Mip Maps OFF, Alpha Source=Input Texture Alpha. PPU는 프로젝트와 통일합니다. 개별 PNG는 Single, 시트는 Multiple로 Grid By Cell Size 48×48, Pivot Center에 맞춥니다. 10개 프레임을 이름 순으로 clip에 넣고 Samples=10, Loop Time ON. 투명 마지막 칸 2개는 clip에 넣지 마세요. SpriteRenderer에는 Sprite Unlit/일반 알파 블렌딩을 사용합니다. UI Image에서는 같은 sprite 목록을 교체하는 방식으로 재생할 수 있습니다. JSON은 Unity 기본 자동 임포터가 아니므로 좌표 참고용입니다.

## 실제 검증과 범위
PixelFX의 painter/renderFrames/spriteSheet/PNG exporter를 사용했습니다. 3회 이상 렌더·검토·수정했습니다. 모든 10프레임의 알파 마스크는 기준 원본과 완전 동일하며 프레임마다 불투명 픽셀 588개, 알파 불일치 0개입니다. t=0과 t=1 렌더 바이트 차이 0, 한 주기 뒤의 동일 시점 재생 차이 0입니다. validation.json에서 확인할 수 있습니다. 하트는 사라지거나 스케일/회전/비율이 변하지 않고 표면 색상만 변화합니다.

Unity 내 clip 재생/성능은 미검증입니다. 기존 씬/설정/원본 스파크는 수정하지 않았습니다. 작업은 ai-technical-art 단독으로 진행했습니다. 새 산출물은 Assets/hty/TechnicalArt/HeartShimmer에만 저장했습니다.

## 소스 재현
Source/heart-shimmer.mjs가 고정 실루엣과 색상 구조, 반짝임을 정의합니다. Source/render.mjs가 시트/개별 프레임/원본/검증 JSON을 내보냅니다. 기존 MetalSparks/Source/pixelfx의 MIT 라이브러리를 참조합니다(라이선스는 해당 checkout LICENSE). Node 실행: node Source/render.mjs. 재실행은 원본 파일을 덮어쓰므로 확정 PNG를 먼저 보존하고 디자인 변경 시 새 버전을 사용하세요.
