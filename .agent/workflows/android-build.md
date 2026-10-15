---
description: Use this workflow for Android build tasks, release preparation, and device testing. Verify build settings first, configure correctly, produce the build, and provide a concrete device test checklist.
---

# Content

## 1. Understand the Build Goal

Before making any changes, determine what type of build is required.

Clarify:

- Is this a **development build** (internal testing, debug symbols, profiler)?
- Is this a **release build** (store submission, performance validation)?
- Is this a **quick device test build** (verify a specific feature on device)?

The build configuration differs between these. Do not apply release settings to
a development build or vice versa.

---

## 2. Inspect Current Build Settings

Before changing any settings, inspect the existing configuration:

- **File > Build Settings** — check selected scenes and platform.
- **Player Settings > Android** — check package name, version, min/target SDK.
- **Project Settings > Quality** — check quality tiers and mobile settings.
- **Project Settings > Audio** — check DSP buffer size (low latency is important).
- **Project Settings > Physics** — check whether physics is needed.

Do not assume settings are correct. Inspect them first.

---

## 3. Required Android Build Settings

Verify these settings before producing any build for device.

### Player Settings

```text
Company Name:         Set appropriately
Product Name:         Chordyceps
Package Name:         com.<company>.chordyceps (stable, never change after release)
Version:              Semantic (e.g., 1.0.0)
Bundle Version Code:  Increments with each store submission
Minimum API Level:    Android 8.0 (API 26) or as determined by store requirements
Target API Level:     Latest stable supported by the store
Scripting Backend:    IL2CPP (required for store release)
Target Architectures: ARM64 required; ARMv7 optional (check store requirements)
```

### Graphics

```text
Rendering Path:       Forward (URP default)
Auto Graphics API:    Off; set explicitly to OpenGLES3 or Vulkan as appropriate
Multithreaded Rendering: Enabled (verify no issues)
```

### Audio (Critical for Chordyceps)

```text
DSP Buffer Size:      Best Latency
```

Low audio latency is a first-class quality requirement for this project.
Always use **Best Latency** DSP buffer size for any build that will be tested
for audio responsiveness.

### Quality

```text
Vsync:                Every V Blank (60 FPS target)
Shadows:              Appropriate for mobile (hard shadows or disabled)
```

---

## 4. Scene Configuration

Confirm the correct scenes are included and in the correct order.

- The first scene in Build Settings is the startup scene.
- Remove any test or temporary scenes before a release build.
- Verify that the main guitar scene is included.

---

## 5. Development Build

Use for internal device testing and profiling.

**Enable:**

- Development Build checkbox.
- Script Debugging (when debugging is needed).
- Autoconnect Profiler (when profiling).

**Do not ship a Development Build to the store.**

A development build provides console output via logcat and Unity Remote,
which is useful for early hardware testing.

---

## 6. Release Build

Use for store submission and final performance testing.

**Confirm before building:**

- Development Build is **unchecked**.
- IL2CPP scripting backend is selected.
- ARM64 architecture is included.
- Package name and version code are correct.
- All test scenes removed.
- All editor-only code is inside `#if UNITY_EDITOR` guards.
- Keystore is correctly configured (ask the developer for keystore details;
  do not assume or modify keystore settings without explicit approval).

**Keystore:**

- The release keystore must be set in Player Settings > Publishing Settings.
- Never commit the keystore password or alias password to Git.
- Confirm keystore configuration with the developer before building.

---

## 7. Build Output

Produce the appropriate output format:

- **APK:** for direct device installation during development.
- **AAB (Android App Bundle):** required for Google Play store submission.

Confirm which format is needed before building.

---

## 8. Device Installation and Testing

After building, install and test on a physical Android device.

### Install APK

```text
adb install -r path/to/build.apk
```

Or use the Unity Build and Run option for a connected device.

### Verify After Install

Provide the developer with this checklist after every device build:

#### Core Functionality

- [ ] App launches without crash.
- [ ] Guitar strings respond to touch.
- [ ] Audio plays promptly after string interaction.
- [ ] Repeated taps produce overlapping sounds correctly.
- [ ] Strumming across multiple strings plays the expected notes.

#### Audio Latency

- [ ] Touch-to-sound latency feels responsive (not noticeably delayed).
- [ ] No audio glitches or dropouts during rapid input.

#### Chord System

- [ ] Chord wheel selection changes the active chord.
- [ ] Key and chord shape changes affect the string audio.
- [ ] Capo controls change the pitch correctly.

#### Song System (when implemented)

- [ ] Song library displays correctly.
- [ ] Song selection loads the correct song.
- [ ] Progression events fire at approximately the correct time.

#### UI

- [ ] All UI is readable and touch-friendly on the test device.
- [ ] No UI elements are clipped or offscreen.
- [ ] Settings screen opens and closes correctly.

#### Performance

- [ ] Frame rate is stable (no consistent drops below 60 FPS).
- [ ] No obvious thermal throttling after extended play.
- [ ] Memory usage is within acceptable range (check with Android Profiler or logcat).

#### Interruptions

- [ ] App handles an incoming phone call correctly (pause/resume).
- [ ] App handles being minimized and restored correctly.
- [ ] App handles screen rotation (if applicable) or locks orientation correctly.

#### Fresh Install

- [ ] App installs and launches correctly with no prior data.
- [ ] No crashes on first launch.

---

## 9. Logcat and Console

After device testing, check logcat for problems.

```text
adb logcat -s Unity
```

Look for:

- Unity errors or exceptions.
- Missing asset or null reference errors.
- Audio system errors.
- Input system errors.
- Out-of-memory warnings.
- ANR (Application Not Responding) events.

Do not ignore logcat errors during device testing.

---

## 10. Performance Profiling on Device

When a performance problem is suspected or before a release build:

1. Enable Autoconnect Profiler in a Development Build.
2. Connect Unity Profiler to the device.
3. Record a session of typical gameplay (strumming, chord changes, song play).
4. Identify CPU, GPU, memory, or audio bottlenecks.
5. Follow the `mobile-performance` skill for the optimization approach.

Do not optimize based on Editor Profiler results alone. Android hardware performance
differs significantly from desktop.

---

## 11. Release Acceptance Checklist

Before submitting a build to the store, confirm all items in the project's Release
Acceptance Checklist (see `Chordyceps_Project_Overview.md` Section 12).

Key items:

- [ ] All gameplay and audio checks passed on a real device.
- [ ] Fresh install tested.
- [ ] App interruption/resume tested.
- [ ] No critical Unity Console errors.
- [ ] Analytics/monetization failure does not break core gameplay.
- [ ] Privacy and store requirements reviewed.
- [ ] Release build tested (not Development Build).
- [ ] Version code incremented from the previous submission.

---

## 12. Git Commit

Only commit build configuration changes when they are intentional and stable.

Do not commit:

- Keystore passwords or credentials.
- Development-only build settings as permanent project settings.
- Temporary test scene inclusion changes.

Before committing, confirm that the build settings reflect the intended state
for the project.

---

## Workflow Summary

```text
Determine Build Goal
    |
    v
Inspect Current Build Settings
    |
    v
Apply Correct Configuration
    |
    v
Confirm Scene List
    |
    v
Build (Development or Release)
    |
    v
Install on Device
    |
    v
Run Device Test Checklist
    |
    v
Check Logcat
    |
    v
Profile if Needed
    |
    v
Confirm Acceptance Checklist (release only)
    |
    v
Commit Configuration (if appropriate)
```

**Core rule:** Always test on a real Android device. Editor and emulator results
are not sufficient for audio latency, performance, or input validation.
