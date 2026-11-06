# 원형 금속 마찰 스파크 / PixelFX

완성 PNG: Exports/impacts/rail-metal-radial/rail-metal-radial.png
좌표 데이터: 같은 폴더 rail-metal-radial.json
검토 이미지: contact.png (배경 포함, 게임용 아님)
재생 미리보기: Preview_Radial.html
재생성 소스: Source/pixelfx/src/fx/impacts/rail-metal-radial.js

360도로 퍼지는 28개 방향의 날카로운 금속 스파크. 흰색/밝은 노란색 점화 → 노랑·주황 파편 확산 → 짧은 잔광 → 투명 종료. 화염·연기 없이 제작했고 기존 방향성 버전은 보존했습니다.

투명 RGBA PNG 512×384px, 셀 128×128px, 가로 4 × 세로 3칸. 위쪽 행부터 왼쪽→오른쪽으로 첫 10개만 사용하세요. 마지막 2칸은 여분, 프레임9는 완전 투명 종료입니다. 권장 30fps, 1회 약 0.333초, Loop OFF. 더 빠르게는 36fps(약 0.278초).

Unity: Sprite (2D and UI), Multiple, Point, Compression=None, Mip Maps OFF, Input Texture Alpha. Sprite Editor Grid By Cell Size 128×128, Pivot Center(0.5,0.5), PPU는 프로젝트 기준으로 통일합니다. 첫 10개 sprite를 순서대로 clip에 배치하고 Samples=30, Loop Time OFF. SpriteRenderer에는 일반 알파 블렌딩 Unlit 소재를 사용하고 재생 종료 후 비활성화/풀 반환하세요. JSON은 기본 Unity에서 자동 애니메이션으로 변환되지 않으므로 좌표 참고용입니다.

PixelFX 렌더/육안 검토/수정 3회, check.mjs 최종 1 pass, 0 fail, 0 warnings. 알파 픽셀 수는 Radial_AlphaPixelCounts.json 참조. Unity Editor 내 재생 검증은 하지 않았습니다. 미리보기의 반복 대기는 검토 편의용이며 게임 애니메이션 자체는 1회 재생입니다.

재생성(소스 checkout에서):
node tools/render.mjs impacts/rail-metal-radial --sheet --all --no-actor --out ../../Exports
node tools/check.mjs impacts/rail-metal-radial

PixelFX MIT 라이선스는 Source/pixelfx/LICENSE 참조.
