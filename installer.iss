; ─────────────────────────────────────────────────────────────────────────────
; Box Breathing Tray — Inno Setup installer script
; Compile with Inno Setup 6+ (https://jrsoftware.org/isinfo.php)
; ─────────────────────────────────────────────────────────────────────────────

#define AppName      "Box Breathing Tray"
#define AppVersion   "1.0.0"
#define AppPublisher "BoxBreathing"
#define AppExeName   "BoxBreathingTray.exe"
; Path to the self-contained publish output — adjust if needed
#define SourceDir    "publish"

[Setup]
AppId={{A7C3E2F1-4D58-4B91-9E2A-1F8C7D3B5A06}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=installer
OutputBaseFilename=BoxBreathingTray_Setup_{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
SetupIconFile=assets\icon.ico
UninstallDisplayIcon={app}\{#AppExeName}
PrivilegesRequired=lowest
; Minimum Windows 10 (build 10.0.17763)
MinVersion=10.0.17763

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; Single self-contained executable produced by dotnet publish
Source: "{#SourceDir}\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Start menu shortcut
Name: "{group}\{#AppName}";   Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"

[Run]
; Launch after install
Filename: "{app}\{#AppExeName}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent

[Registry]
; Auto-start with Windows (current user)
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; \
  ValueType: string; ValueName: "{#AppName}"; \
  ValueData: """{app}\{#AppExeName}"""; \
  Flags: uninsdeletevalue

[UninstallRun]
; Kill the tray app before uninstalling
Filename: "taskkill"; Parameters: "/f /im {#AppExeName}"; Flags: runhidden waituntilterminated; RunOnceId: "KillApp"
