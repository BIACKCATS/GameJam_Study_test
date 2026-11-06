# 마우스 오른쪽 클릭 안내

기존 왼쪽 클릭 버전을 보존하고 별도 생성했습니다. 오른쪽 버튼만 눌리고 오른쪽에 충격파와 반짝임이 나타납니다.

128×128px / 24프레임 / 16 FPS / 1.5초 반복 / 투명 PNG.

Exports/Mouse_RightClick_01.png ~ Mouse_RightClick_24.png
Exports/Mouse_RightClick_Spritesheet.png (768×512, 6열×4행)
Exports/Mouse_RightClick_Preview.gif
Exports/Mouse_RightClick_Base.png
Exports/Mouse_RightClick_Pressed.png
Exports/Mouse_RightClick_Body_Spritesheet.png
Exports/Mouse_RightClick_Effect_Spritesheet.png
Exports/Mouse_RightClick_Frames.json
Exports/Validation.json

Unity: 압축 해제 후 PNG를 가져오고 128×128 셀로 슬라이스합니다. 16 FPS, Loop Time On, Center 피벗, Point 필터를 사용합니다. GIF의 반투명은 디더링 근사이며 PNG에는 실제 알파가 있습니다. 왼쪽 버튼 불변, 본체 실루엣 고정, 원상 복귀 및 주기 픽셀 일치를 검증했습니다. Unity Editor 재생은 미검증입니다.

Source는 기존 MouseClick/Source/mouse.mjs와 MetalSparks/Source/pixelfx를 참조합니다. 완성 PNG/GIF 사용에는 이 소스 의존성이 필요하지 않습니다.
