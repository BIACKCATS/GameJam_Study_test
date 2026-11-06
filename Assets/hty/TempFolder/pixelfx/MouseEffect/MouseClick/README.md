# 마우스 왼쪽 클릭 안내 — PixelFX

128×128px, 24프레임, 16 FPS, 1.5초 무한 반복. 고정된 밝은 회색 마우스의 왼쪽 버튼만 노란색으로 강조되고 2px 눌린 뒤 원상 복귀합니다. 클릭 직후 버튼 주변 충격파와 4개의 짧은 반짝임이 나타납니다.

## 실제 파일
- Exports/Mouse_Click_01.png ~ Mouse_Click_24.png: 투명 RGBA 개별 프레임
- Exports/Mouse_Click_Spritesheet.png: 768×512, 6열×4행 완성형 시트
- Exports/Mouse_Click_Preview.gif: 투명 배경 반복 미리보기
- Exports/Mouse_Click_Base.png: 기본 마우스
- Exports/Mouse_Click_Pressed.png: 이펙트 없는 눌림 상태
- Exports/Mouse_Click_Body_Spritesheet.png: 본체/버튼 동작만 분리
- Exports/Mouse_Click_Effect_Spritesheet.png: 충격파/반짝임만 분리
- Exports/Mouse_Click_Frames.json: 프레임 좌표와 타이밍
- Exports/Contact.png: 검토용 배경 포함 컨택트 시트
- Exports/Validation.json: 실제 검증 결과
- Source/: PixelFX 원본/출력/검증 코드. 기존 MetalSparks/Source/pixelfx 및 Node 필요.

## 타임라인
1~4 대기 / 5~6 왼쪽 버튼 강조 / 7~8 눌림 / 9~10 눌림 유지 및 충격파·반짝임 / 11~12 복귀 / 13~15 이펙트 소멸 / 16~24 기본 상태.

## Unity
ZIP은 먼저 압축 해제하고 필요한 PNG를 Assets에 넣습니다. ZIP 자체는 애니메이션으로 임포트되지 않습니다. Sprite (2D and UI), Multiple, 128×128 셀 크기로 분할하고 왼쪽→오른쪽, 위→아래 순서로 24프레임을 사용합니다. 16 FPS, Loop Time On. 피벗은 모든 프레임 Center로 통일하고 Point 필터/Compression None/Mipmap Off를 권장합니다. 클릭 입력이 확인되면 게임 쪽에서 안내 오브젝트를 숨깁니다. 입력 처리 스크립트나 Unity 프리팹은 포함하지 않습니다.

분리 시트는 동일한 위치/피벗/프레임 인덱스로 합성합니다. GIF는 10ms 시간 단위 때문에 60/70ms를 배분하며 총 1.5초를 유지합니다. GIF의 반투명 제한 때문에 충격파는 투명 픽셀 디더링으로 근사하고, PNG에는 실제 알파 감소가 유지됩니다.

## 검증
첫/마지막 프레임 RGBA 일치, 한 주기 뒤 전체 프레임 일치, 본체 외곽 알파 고정, 오른쪽 버튼 영역 불변, 12프레임까지 본체 원상 복귀, 투명 여백을 확인했습니다. GIF는 128×128/24프레임/1.5초로 디코딩 확인했습니다. Unity Editor 실제 재생은 검증하지 않았습니다.
