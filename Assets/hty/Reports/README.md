# Character normal lighting test

독립적인 URP 2D **diffuse relighting** 데모입니다. 원본 게임 캐릭터를 새로 제작한 결과가 아닙니다. Febucci Text Animator 3.20.1의 실제 DamageNumbers 샘플 `PlayerSpriteSheett.png`를 사용했습니다. 원본 및 복사본 SHA256은 BuildFacts.txt를 참조하십시오. 샘플 아트의 별도 재배포 권한을 새로 부여하지 않습니다.

## 실행

1. **이 산출물을 가진 체크아웃의** Unity 6000.4.5f1 Editor를 여십시오. 원본 `D:/Project/GameJam_Study_test` Editor를 차용하지 않습니다.
2. `Assets/hty/Scenes/CharacterNormalLighting_Test.unity`를 열고 Play 하십시오. 공유 빌드 목록에 추가할 필요가 없습니다.
3. 방향키: Left/Right/Up/Down 광원 위치. N: 노멀 ON/OFF. O: 느린 궤도. Space: 애니메이션 재생/정지. `.`/`,`: 다음/이전 프레임(자동 재생 정지).

테스트 논리 해상도는 **160×120**, 캡처는 **640×480(4배)** 입니다. 팀 전역 표준이라는 뜻이 아닙니다. PixelPerfectCamera PPU16, UpscaleRenderTexture, Windowbox, Point를 사용합니다. 스프라이트는 원본 그대로 여섯 개 16×16 rect, x=0/16/32/48/64/80, y=0, 중앙 피벗, PPU16입니다. 원본 PNG 바이트는 복사본과 동일합니다.

## 비교 조건

같은 Sprite-Lit-Default 전용 머티리얼, 같은 스프라이트/색/광원/카메라를 유지하고 **Light2D NormalMapQuality만 Accurate ↔ Disabled**로 바꿉니다. OFF도 Lit 조명을 받습니다. ON은 `_NormalMap` secondary texture를 사용합니다. 알베도보다 어두워지는 것 자체는 정확한 노멀 방향과 광원 입사각에 의한 diffuse 반응입니다. 일반 URP Sprite-Lit은 여기서 금속성/거칠기 기반 specular 하이라이트를 구현하지 않습니다. 커스텀 셰이더는 만들지 않았습니다.

로컬 Point Light: 거리1.4, 위치 반경1.35, inner radius2.4, outer4.5, intensity0.65, 색(1,.96,.9). ambient0.25. 기존 공유 2D Renderer의 light texture scale0.5는 변경하지 않았습니다. 소스 눈의 흰색 자체는 클리핑 오류로 판정하지 않습니다. 최종 캡처의 allWhitePixels=0은 참고 수치이며 검증 전체를 대체하지 않습니다.

노멀은 **복원된 지오메트리가 아닌 스타일화한 근사**입니다. 밝기/색을 높이로 해석하지 않습니다. 프레임별 동일 좌표 영역에 머리/머리 윗면, 몸통/옷 가장자리, 팔다리의 약한 방향을 직접 지정합니다. 중립 평면(128,128,255 부근)을 유지하고 기울기를 제한했습니다. 모든 프레임에 동일한 아트 방향 규칙을 적용했으나 프레임별 실제 3D 형태를 추정한 것은 아닙니다. PNG 원본 알파를 그대로 보존했습니다. Unity NormalMap 임포터는 런타임 채널을 재패킹할 수 있습니다.

## 실제 렌더 증거 및 재현

- `Captures/F{0..5}_{Left,Right,Up,Down}_{ON,OFF}.png`: **실제 Unity SRP 카메라** 출력 48장. CPU 조명 합성이 아닙니다.
- `Captures/Orbit_00..11.png`: 고정 프레임0, 30도 간격 광원 궤도 12장.
- `CaptureEvents.csv`: 각 출력의 Unity frameCount와 광원 위치. 캡처마다 세 렌더 프레임 안정화 후 기록합니다. 캡처 중 rig Update를 잠시 끄므로 updates 열이 일정한 것은 의도된 동작입니다. 카메라와 Unity frameCount는 진행합니다.
- `Comparison_Frame0.png`: 위 실제 F0 캡처 중앙96×96 영역만 최근접2배 확대. 상단 ON/하단 OFF, 열 L/R/U/D. 글자만 픽셀 폰트로 덧붙였으며 조명은 재계산하지 않았습니다.
- `Validation.txt`: 알파/임포트/24개 ON/OFF 픽셀 차이. `Controls.txt`: 임시 InputSystem Keyboard 이벤트가 실제 runtime Update 경로에서 처리된 결과.

동결 전 재현 메뉴: `HTY/Character Lighting/Build isolated test` → Play → `Capture 48 comparisons and 12 orbit views` → 완료 로그 → `Validate source, imports and captured pairs` → `Create actual-render comparison sheet`. CLI는 반드시 해당 체크아웃의 정확한 `--project-path` 및 `--caller plugin --skill unity-cli`를 사용합니다. 키 검증은 `Hty.EditorTools.CharacterLightingCapture.ValidateKeyboardControls()`입니다. 생성/캡처/검증 도구는 `Reports/FROZEN.txt`가 있으면 쓰기를 차단합니다. QA가 자기 복사본에서 재캡처하려면 코디네이터와 조율해 **자기 복사본의** 동결 마커만 별도로 처리해야 하며 개발 원본은 수정하지 않습니다.

## 비용·범위·한계

96×16 RGBA32는 텍스처당 비압축 원시6144바이트이며 GPU 실제 normal 포맷/렌더타깃/패스 비용과 같다는 뜻은 아닙니다. 실제 GPU는 Validation.txt에 기록했습니다. 노멀 렌더 패스와 광원 오버드로우는 추가 비용이며 프레임 시간/메모리 벤치마크는 수행하지 않았습니다.

직접 작성한 파일은 `Assets/hty/**` 및 최초 `Assets/hty.meta`뿐입니다. 첫 Editor import가 범위 밖 ProjectSettings 및 .vsconfig 등을 자동 생성/정규화한 사실은 별도 Audit.md에 기록하며 무단 복구하지 않습니다. 공유 설정/소스/원본 .meta·GUID를 제작 도구에서 수정하지 않습니다. Git commit/merge/push는 하지 않습니다.
