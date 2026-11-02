# HANDOFF

## 2026-11-03 Claude — JSH 템플릿 코드 (Core / Juice / DebugTools)
**Done**
- `Assets/Scripts/JSH/Core`: Singleton, GameManager(상태·timeScale), EventBus, SceneLoader(페이드), AudioManager, SaveSystem, ComponentPool/PoolReturn/AutoRelease
- `Assets/Scripts/JSH/Juice/HitStop.cs`(일시정지와 충돌 없음), `Assets/Scripts/JSH/DebugTools`: DebugCheats(F1~F6, F12), DebugFlags. 컴파일 에러 0
- asmdef 없음(Assembly-CSharp): Feel·DOTween을 직접 참조하려고 의도적으로 뺌

**Next (Antigravity)**
- `Assets/Prefabs/JSH/[Managers].prefab` 만들기: GameManager, SceneLoader, AudioManager, HitStop, DebugCheats를 한 오브젝트에 붙이기
- [Managers] 자식으로 Fade Canvas 추가(Sort Order 999, 전체 화면 검정 Image와 CanvasGroup, alpha 0) → SceneLoader `_fadeGroup`에 연결
- `Assets/Scenes/JSH/` 테스트 씬에 [Managers]를 배치하고 Play로 확인(ESC 일시정지, F2 배속, F3 리로드)

**Warnings**
- Singleton을 상속한 클래스는 `Awake` 대신 `OnSingletonAwake`를 override할 것. 씬마다 [Managers]를 두면 중복분은 자동으로 삭제됨
- 일시정지 상태에서 Reload하면 상태가 Paused로 남음 → 재시작 버튼은 `GameManager.StartGame()` 호출 후 `SceneLoader.Reload()`
- 카메라 흔들림·플래시·플로팅 텍스트는 Feel MMF_Player 사용(따로 만들지 않음)
