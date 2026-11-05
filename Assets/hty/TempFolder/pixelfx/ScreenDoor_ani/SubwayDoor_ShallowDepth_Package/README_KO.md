# 후면 벽 + 완만한 바닥 원근 수정판

문 뒤 상부에 청회색 벽을 두고, 하부 약 1/3에만 바닥이 보이도록 조정했습니다. 바닥 타일의 앞뒤 폭 변화는 기존 22~100%에서 78~100%로 줄여 소실점으로 급하게 몰리는 느낌을 완화했습니다. 벽 하단의 얇은 걸레받이와 타일 간격으로 거리감을 표현했습니다.

기존 중앙 문/검정 창문/좌석/창문/손잡이/조명/앞쪽 바닥, 간판 없는 상태와 문 이동은 유지했습니다. 문 뒤 배경용 Passage.rgba만 변경했으며 이전 버전/원본은 보존했습니다.

426×307, 36프레임, 12 FPS, 총 3초, 1회 재생.
01~09 열림 → 10~27 완전 열림 18프레임(1.5초) → 28~36 닫힘.

Exports/SubwayDoor_Wide_OpenHoldClose_01.png ~ 36.png
Exports/SubwayDoor_Wide_OpenHoldClose_Spritesheet.png: 2556×1842, 6열×6행, 셀 426×307.
Exports/SubwayDoor_Wide_OpenHoldClose_Spritesheet.json
Exports/SubwayDoor_Wide_OpenHoldClose_Preview.gif
Exports/Contact.png 및 Validation/FramePreservation/BlackWindowValidation/GifValidation JSON.

Unity: Sprite, Point, Compression None, Mip Maps Off, Pivot Center, 시트 Max Size 4096 이상. 36프레임 전체를 Samples=12/Loop Time Off로 사용합니다. 대기 프레임 10~27을 삭제하지 마세요. PNG는 게임용, GIF는 팔레트 근사 미리보기입니다.

검증: 첫/마지막 닫힘 기준 동일, 열린 18프레임 동일, 중앙 문 외부 고정 배경 RGBA 차이 0, 노출 창문 88,260픽셀 불투명 검정 확인. 실제 열린 이미지에서 후면 벽/낮아진 바닥 원근을 확인했습니다. Unity 실시간 재생은 이번 파일 수정에서 별도로 검증하지 않았습니다.
