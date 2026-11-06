# 금속 마찰 스파크 (PixelFX)

완성 시트: Exports/impacts/rail-metal-sparks/rail-metal-sparks.png
프레임 좌표/시간: 같은 위치 rail-metal-sparks.json
전체 프레임 보기: contact.png (검토용 배경 포함, 게임 텍스처로 쓰지 마세요)
애니메이션 보기: Preview.html

투명 RGBA 시트, 384×384px, 128×96px 셀, 가로 3칸 × 세로 4칸. 위쪽 행부터 왼쪽→오른쪽 순으로 첫 10칸만 사용합니다. 마지막 두 칸은 여분입니다. 0~8번에 점화→확산→잔광이 있고 9번은 완전 투명 종료 프레임입니다. 30fps 권장(10프레임 약 0.333초), Loop OFF. 더 날카로운 느낌은 36fps(약 0.278초). 기본 방향은 오른쪽 위로 분사되며 SpriteRenderer.flipX 또는 오브젝트 회전으로 접촉 방향에 맞춥니다. 화염·연기 없이 흰색/밝은 노랑의 짧은 핵과 노랑·주황 직선 파편으로 구성했습니다.

Unity: Texture Type=Sprite, Sprite Mode=Multiple, Filter=Point, Compression=None, Mip Maps OFF, Alpha Source=Input Texture Alpha. Sprite Editor에서 Grid By Cell Size 128×96으로 slice하고 10개 프레임만 선택합니다. 모든 프레임 pivot을 동일한 Custom (0.1875, 0.2916667)로 지정하세요(원점 24,68은 이미지 상단 기준). PPU는 프로젝트 스프라이트와 동일하게 맞추고 Unlit/Sprite 소재의 일반 알파 블렌딩을 사용합니다. Animator clip Samples=30, Loop Time OFF; 재생 종료 후 비활성화하거나 풀에 반환합니다. JSON은 좌표 참고용으로 Unity가 기본 자동 변환하지 않습니다. 카메라/씬/프로젝트 설정은 이번 작업에서 변경하지 않았고 Unity 내 재생은 미검증입니다.

재생성: Source/pixelfx에서 Node 18+로
node tools/render.mjs impacts/rail-metal-sparks --sheet --all --no-actor --out ../../Exports
검증: node tools/check.mjs impacts/rail-metal-sparks
최종 검사 1 pass, 0 fail, 0 warnings. 렌더→육안 검토→수정 3회 진행, 파편별 난수 순서 고정과 경계 여유 조정, 마지막 확산 밀도 조정. library MIT 라이선스 및 원본 출처는 Source/pixelfx에 포함. 별도 에이전트 없이 제작했습니다.
