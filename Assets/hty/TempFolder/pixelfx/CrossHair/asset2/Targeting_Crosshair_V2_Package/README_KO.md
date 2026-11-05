# 대형 타겟팅 크로스헤어 V2

이전 결과물의 단조로움을 줄이고 주변의 독립된 도트 장식을 제거한 개정판입니다. 기존 버전은 보존했습니다.

## 변경
- 두꺼운 주 링 + 얇은 안쪽/바깥쪽 링, 링 자체의 넓은 색상 구간.
- 축과 연결된 좌우 슬리브, 테이퍼 접합부, 상하 접합부.
- 주 구조 코랄 레드 #e94a65, 보조 링/슬리브 청록 #43bfc6, 접합부/중앙 프레임 황금색 #e6b84b.
- 색상 자체를 나눈 디자인입니다. 광택, 블룸, 그라디언트 또는 명암 단계 추가로 다양성을 대신하지 않았습니다. 기존 가독성용 외곽선은 중립색 #272635입니다.
- 주변 점/파티클/별 모양 없음. 최종 불투명 구조 전체는 8방향 연결 기준 하나로 연결됩니다.

## 사양 및 파일
14프레임, 16 FPS, 0.875초, 1회 재생, 마지막 4프레임 0.25초 고정.
각 PNG 320×180 투명 RGBA. 주 링 지름 112px, 보조 링 지름 128px. 긴 수평선 297px, 수직선 161px.
Exports/Targeting_Crosshair_01.png ~ Targeting_Crosshair_14.png
Exports/Targeting_Crosshair_Spritesheet.png: 960×900, 3열×5행. 320×180 셀, 좌상단부터 행 순서의 첫 14칸. 마지막 빈 칸 제외.
Exports/Targeting_Crosshair_Spritesheet.json: 좌표/시간 정보.
Exports/Targeting_Crosshair_Preview.gif: 투명, 14프레임, 비반복, GIF 시간 양자화로 0.88초.
Exports/Contact.png: 검토용 배경과 번호 포함. 게임용 이미지가 아닙니다.
Exports/Validation.json, NoDotsValidation.json: 실제 생성 검증.
Source/targeting-crosshair.mjs, render.mjs: PixelFX 생성 소스.

## Unity
Sprite(2D and UI), Point, Compression None, Mip Maps Off, Pivot Center. Samples=16, Loop Time=Off. 화면 중앙/표적 위치에 큰 오버레이로 배치합니다. 16:9 정수배 확대 권장. 표적 추적/조준 완료 상태 유지 및 해제는 게임 로직에서 연결합니다. JSON은 프레임 좌표 참고용입니다.

## 검증
PixelFX painter/renderFrames/spriteSheet를 사용했고 반복 렌더/시각 검토했습니다. 14프레임, 알파 0/255, 최종 4프레임 RGBA 동일 확인. 모든 프레임에서 8방향 연결 기준 1~3픽셀 독립 덩어리 0개, 마지막 프레임의 연결 구조 1개 확인. GIF 실제 디코딩 14프레임/0.88초. Unity 실행 검증은 이번 이미지 작업에서 수행하지 않았습니다. 기존 에셋/씬/설정은 수정하지 않았습니다.
Node로 Source/render.mjs 실행 시 기존 MetalSparks/Source/pixelfx 라이브러리를 참조합니다. PNG 사용에는 별도 설치가 필요 없습니다.
