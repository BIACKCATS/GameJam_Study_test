# 진엔딩 축하 폭죽 2종

PixelFX 원본과 렌더러로 제작했습니다. 각 160×160px, 16프레임, 16 FPS, 1초 1회 재생입니다.

- PurpleBlue: 보라/파랑/하늘색 80개 입자의 3겹 원형 확산, 작은 반짝임과 잔광.
- RedGreen: 빨강/초록 10개 긴 불꽃, 끝에서 양갈래로 분리되는 비대칭 길이의 가지.
- 중심 좌표: 두 종류 모두 (80,80), Unity 피벗 Center (0.5,0.5).

## 파일

Exports/Firework_PurpleBlue_01.png ~ Firework_PurpleBlue_16.png
Exports/Firework_PurpleBlue_Spritesheet.png 및 .json
Exports/Firework_PurpleBlue_Preview.gif
Exports/Firework_RedGreen_01.png ~ Firework_RedGreen_16.png
Exports/Firework_RedGreen_Spritesheet.png 및 .json
Exports/Firework_RedGreen_Preview.gif
Exports/Fireworks_Comparison.png: 두 폭죽 6번째 프레임 비교(투명 배경)
Exports/Compare.html: 두 GIF를 나란히 보는 반복 미리보기
Exports/*_Contact.png: 프레임 검토용 배경 포함 이미지
Exports/Validation.json: 검증 수치
Source/: 효과 원본, 출력, 검증 코드. 기존 MetalSparks/Source/pixelfx 및 Node 필요.

## Unity

ZIP은 먼저 압축 해제하고 PNG를 Assets로 가져옵니다. 각 시트는 640×640px, 4열×4행입니다. Sprite (2D and UI), Multiple, Grid by Cell Size 160×160으로 분할합니다. 왼쪽부터 오른쪽, 위에서 아래 순서로 16장을 재생합니다. 16 FPS, Loop Time Off. Point 필터, Compression None, Mip Maps Off 권장. 마지막 투명 프레임까지 재생 후 오브젝트를 비활성화하거나 풀에 반환합니다. GIF는 검토 편의를 위해 반복하며 게임용 애니메이션은 1회 재생입니다. 프레임 샘플 시각은 0~0.9375초이며 마지막 프레임을 포함한 전체 재생 길이는 1초입니다.

엔딩 배치는 같은 중심 피벗으로 위치만 바꾸고 두 종류의 시작 시각을 0.2~0.4초 정도 엇갈리게 하면 됩니다. JSON은 좌표 자료이며 Unity 자동 임포터를 포함하지 않습니다. PNG 배경은 투명하고 일반 알파 블렌딩으로 사용할 수 있습니다.

## 검증 범위

두 종류 각각 16장, 160×160, GIF 1초를 디코딩 확인했습니다. 모든 프레임의 외곽 여백이 투명하고 최종 프레임이 완전 투명합니다. 세 차례 이상 컨택트 시트를 확인하며 형태와 잔광을 수정했습니다. Unity Editor 안에서의 실제 재생은 검증하지 않았습니다. 기존 프로젝트 설정과 다른 자산은 변경하지 않았습니다.
