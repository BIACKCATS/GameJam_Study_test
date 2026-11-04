# Build and scope audit

Project: `D:/orca/workspaces/GameJam_Study_test/ai-development`, branch `po2zn/ai-development`. Start Git status was clean. No branch/commit/merge/push operations were performed. Original `D:/Project/GameJam_Study_test` Editor was not driven.

The requested Unity CLI at `C:/Users/muste/AppData/Local/Unity/bin/unity.exe` was beta13; version check reported beta16 available. The assigned CLI was retained rather than changing the user's tool installation. Every Editor operation explicitly pinned the full development checkout. Dedicated Unity6000.4.5f1 PID46396 was identified; Pipeline ports7801/7803 changed during reloads. Initial import settling caused a30-second EDITOR_NOT_READY timeout; a subsequent normal connection succeeded. A request immediately after Play domain reload returned a network error; confirmed ready/ticking status preceded the next request.

## Compilation and Console

- Initial own code used deprecated PixelPerfectCamera members and FindFirstObjectByType, then an unavailable public filterMode setter caused CS1061. Replaced with current gridSnapping/cropFrame enums, serialized m_FilterMode, FindAnyObjectByType. Final compilationFailed=false, compiling=false.
- Early capture cleanup released an active RenderTexture and warned. Cleanup now occurs on an Editor delayed callback after unbinding the camera; final recapture did not repeat that warning.
- A temporary PowerShell contact-sheet script was blocked by system execution policy. No policy was changed. That newly created script was removed and replaced with the Unity C# actual-capture crop tool. Unity subsequently removed its orphan meta during refresh; the final deliverable has no such script/meta.
- ConsoleHistory.json intentionally preserves historical errors/warnings. At2026-11-04T12:43:22Z its ground truth reported Console errors1/warnings1, **not a clean Console**. Entries include UnityEditor.Search.SearchDatabase ArgumentOutOfRangeException and Unity AI Account API timeouts unrelated to hty code. No shared/package fix was attempted. Final render/alpha/import/24-pair checks and runtime keyboard checks passed despite these Editor issues.

## Unexpected writes from Editor initialization

After first Editor launch/import Git reported:

| Path | Observed change |
|---|---|
| ProjectSettings/ProjectSettings.asset | Editor serialized default Android platform icon entries (+93/-1 semantic diff) |
| ProjectSettings/EditorBuildSettings.asset | Byte/newline normalization; git diff showed no semantic change |
| ProjectSettings/ShaderGraphSettings.asset | Byte/newline normalization; git diff showed no semantic change |
| .vsconfig | Newly generated |
| ProjectSettings/SceneTemplateSettings.json | Newly generated |

These were escalated to the coordinator and were **not restored/deleted by this worker**, because the original authorization only permits hty writes. Coordinator said cleanup approval was pending and authorized continued hty-only captures. Exact final Git status is recorded separately; cleanup ownership stays with coordinator. All intentional implementation and report writes are under Assets/hty plus its initially generated folder meta. No original asset/GUID, Packages, shared scene or renderer was intentionally edited.

## Final evidence

Actual GPU: Intel Arc140T, Direct3D12.60 actual Unity renders, including48 frame/direction/normal combinations and12 fixed-F0 orbit views. Latest capture Unity frame3575 through3752 (three frames per saved view). Diffuse byte hash equals original. Source-alpha mismatch0;114 flat opaque pixels and290 directional opaque pixels.24/24 pairs have nonzero measured differences. All final captures report0 all-white pixels; this is a secondary metric rather than a universal clipping test. Captures were visually inspected, including the nearest-neighbor comparison sheet. No CPU lighting approximation was used.

Runtime controls were tested with a temporary Input System Keyboard, real queued key state events and actual component Update invocation, then device removal and restoration of the prior keyboard. Play Mode was stopped and temporary auto-tick disabled after verification. Runtime animation/orbit advancement was also seen in the active Editor; automated captures use deterministic states and pause rig updates intentionally. Physical human keypresses and frame-time/GPU memory benchmarking were not performed.

Frozen delivery includes all files and meta in SHA256 manifest, excluding the manifest itself to avoid a recursive hash. The manifest's own hash is sent via Orca. No files should be regenerated in this development checkout after freeze without coordination. QA copies only hty and hty.meta to its own checkout; any re-run should use a separate coordinated copy and not mutate this frozen source.
