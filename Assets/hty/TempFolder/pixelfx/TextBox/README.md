# 메시지 입력 중 애니메이션

PixelFX로 제작한 청회색 말풍선과 파란 점 3개의 순차 바운스 루프입니다.

- 프레임: 64×40px, 36장, 24 FPS, 1.5초 반복.
- 말풍선은 점 3개에 대응하는 세 영역으로 움직입니다. 각 점과 해당 영역이 함께 최대 4px 올라갔다 내려옵니다. 영역 사이는 연결된 곡선형 픽셀 경계로 이어지며 꼬리도 왼쪽 영역을 따라 움직입니다. 활성 점은 파란색으로 강조됩니다.
- PNG 배경은 투명하며 말풍선 내부는 불투명합니다.
- Exports/Typing_01.png ~ Typing_36.png: 개별 프레임.
- Exports/Typing_Spritesheet.png: 320×320px, 5열×8행. 왼쪽→오른쪽, 위→아래 순서로 앞 36칸만 사용합니다.
- Exports/Typing_Spritesheet.json: 프레임 좌표와 PixelFX 메타데이터.
- Exports/Typing_Preview.gif: 무한 반복 미리보기. GIF 시간 단위 특성상 40/50ms를 배분하며 총 길이는 1.5초입니다.
- Exports/Contact.png: 프레임 확인용 배경 포함 이미지이며 게임용 자산이 아닙니다.
- Exports/Validation.json: 실제 픽셀 검증 결과.

Unity 적용: Sprite (2D and UI), Multiple, Grid by Cell Size 64×40으로 슬라이스합니다. 피벗 Center, Filter Mode Point, Compression None, Mip Maps Off를 권장합니다. 36개 스프라이트를 순서대로 24 FPS로 재생하고 Loop Time을 켜십시오. UI Image에서 사용 시 Preserve Aspect를 유지하고 정수 배율로 표시하면 선명합니다. JSON은 프레임 정보용이며 Unity 자동 임포트 플러그인을 포함하지 않습니다.

검증: 36프레임과 1.5초 GIF를 디코딩 확인했습니다. 한 주기 뒤 동일 시점의 모든 RGBA 픽셀이 일치하며, 각 열의 국소 이동을 보정한 말풍선 실루엣은 일정하고 외곽 여백은 투명합니다. 점은 10/22/34번째 프레임에서 순차적으로 최고점에 도달합니다. Unity Editor 재생 검증은 수행하지 않았습니다.

Source/typing.mjs는 PixelFX 효과 원본, render.mjs는 PNG/시트/GIF 출력, validate.mjs는 루프 검증 코드입니다. 실행에는 기존 MetalSparks/Source/pixelfx 체크아웃과 Node가 필요합니다.


