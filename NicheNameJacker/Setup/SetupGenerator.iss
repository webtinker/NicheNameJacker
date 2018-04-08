[Setup]
AppId=e4c97b20-9a1c-4849-aab5-aee0d2be734f
AppName=Niche Name Jacker
AppVersion=1.0.0.53
AppPublisher=NicheJacker Software LLC
DefaultDirName={pf}\Niche Name Jacker
DefaultGroupName=NicheJacker Software LLC
UninstallDisplayIcon={app}\NicheNameJacker.exe
OutputBaseFilename=NNJ
LicenseFile=license.txt
SetupIconFile=..\NicheNameJacker.ico

[Icons]
Name: "{group}\Niche Name Jacker"; Filename: "{app}\NicheNameJacker.exe"; IconFilename: "{app}\NicheNameJacker.ico";
Name: "{commondesktop}\Niche Name Jacker"; Filename: "{app}\NicheNameJacker.exe";

[Run]
Filename: {app}\NicheNameJacker.exe; Description: Run the application; Flags: shellexec skipifsilent nowait postinstall;

[Files]
Source: "../NicheNameJacker.exe"; DestDir: {app}
Source: "../NicheNameJacker.ico"; DestDir: {app}
Source: "../BouncyCastle.Crypto.dll"; DestDir: {app}
Source: "../Google.Apis.Auth.dll"; DestDir: {app}
Source: "../Google.Apis.Auth.PlatformServices.dll"; DestDir: {app}
Source: "../Google.Apis.Core.dll"; DestDir: {app}
Source: "../Google.Apis.dll"; DestDir: {app}
Source: "../Google.Apis.PlatformServices.dll"; DestDir: {app}
Source: "../Google.Apis.YouTube.v3.dll"; DestDir: {app}
Source: "../HtmlAgilityPack.dll"; DestDir: {app}
Source: "../log4net.dll"; DestDir: {app}
Source: "../MaterialDesignColors.dll"; DestDir: {app}
Source: "../MaterialDesignThemes.Wpf.dll"; DestDir: {app}
Source: "../Microsoft.Azure.KeyVault.Core.dll"; DestDir: {app}
Source: "../Microsoft.Data.Edm.dll"; DestDir: {app}
Source: "../Microsoft.Data.OData.dll"; DestDir: {app}
Source: "../Microsoft.Data.Services.Client.dll"; DestDir: {app}
Source: "../Microsoft.Expression.Interactions.dll"; DestDir: {app}
Source: "../Microsoft.WindowsAzure.Storage.dll"; DestDir: {app}
Source: "../Newtonsoft.Json.dll"; DestDir: {app}
Source: "../ServiceStack.Text.dll"; DestDir: {app}
Source: "../System.Reactive.Core.dll"; DestDir: {app}
Source: "../System.Reactive.Interfaces.dll"; DestDir: {app}
Source: "../System.Reactive.Linq.dll"; DestDir: {app}
Source: "../System.Reactive.PlatformServices.dll"; DestDir: {app}
Source: "../System.Reactive.Windows.Threading.dll"; DestDir: {app}
Source: "../System.Spatial.dll"; DestDir: {app}
Source: "../System.Windows.Interactivity.dll"; DestDir: {app}
Source: "../YoutubeExtractor.dll"; DestDir: {app}
Source: "../Zlib.Portable.dll"; DestDir: {app}
Source: "../Assets/Docs/tutorial.pdf"; DestDir: {app}/Assets/Docs
Source: "../NicheNameJacker.exe.config"; DestDir: {app}

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\NicheNameJacker"

[Code]
function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1'          .NET Framework 1.1
//    'v2.0'          .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.5'          .NET Framework 4.5
//    'v4.5.1'        .NET Framework 4.5.1
//    'v4.5.2'        .NET Framework 4.5.2
//    'v4.6'          .NET Framework 4.6
//    'v4.6.1'        .NET Framework 4.6.1
//    'v4.6.2'        .NET Framework 4.6.2
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key, versionKey: string;
    install, release, serviceCount, versionRelease: cardinal;
    success: boolean;
begin
    versionKey := version;
    versionRelease := 0;

    // .NET 1.1 and 2.0 embed release number in version key
    if version = 'v1.1' then begin
        versionKey := 'v1.1.4322';
    end else if version = 'v2.0' then begin
        versionKey := 'v2.0.50727';
    end

    // .NET 4.5 and newer install as update to .NET 4.0 Full
    else if Pos('v4.', version) = 1 then begin
        versionKey := 'v4\Full';
        case version of
          'v4.5':   versionRelease := 378389;
          'v4.5.1': versionRelease := 378675; // 378758 on Windows 8 and older
          'v4.5.2': versionRelease := 379893;
          'v4.6':   versionRelease := 393295; // 393297 on Windows 8.1 and older
          'v4.6.1': versionRelease := 394254; // 394271 on Windows 8.1 and older
          'v4.6.2': versionRelease := 394802; // 394806 on Windows 8.1 and older
        end;
    end;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + versionKey;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0 and newer use value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 and newer use additional value Release
    if versionRelease > 0 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= versionRelease);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;


function InitializeSetup(): Boolean;
begin
    if not IsDotNetDetected('v4.5', 0) then begin
        MsgBox('The application requires Microsoft .NET Framework 4.5.'#13#13
            'Please install it first'#13
            'and then re-run the setup program.', mbInformation, MB_OK);
        result := false;
    end else
        result := true;
end;