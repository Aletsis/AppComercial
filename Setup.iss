[Setup]
AppName=AppComercial POS System
AppVersion=1.0.0
AppPublisher=AppComercial
DefaultDirName={pf}\AppComercial
DefaultGroupName=AppComercial
OutputDir=.\Releases
OutputBaseFilename=AppComercial_Instalador_v1.0
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin

[Files]
; IMPORTANTE: Antes de compilar este instalador, debes hacer un dotnet publish de ambos proyectos
; Ejemplo: dotnet publish AppComercial.Api -c Release -o AppComercial.Api\bin\publish
; Binarios en Program Files (solo lectura en runtime — correcta práctica Windows)
Source: "AppComercial.Api\bin\publish\*"; DestDir: "{app}\Api"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "AppComercial.ServerManager\bin\publish\*"; DestDir: "{app}\ServerManager"; Flags: ignoreversion recursesubdirs createallsubdirs

; Configuración mutable en ProgramData (escribible por usuarios sin UAC)
; onlyifdoesntexist: preserva config del usuario en upgrades; el PSCommand post-install actualiza los campos SQL
Source: "AppComercial.Api\bin\publish\appsettings.json"; DestDir: "{commonappdata}\AppComercial\Api"; Flags: ignoreversion onlyifdoesntexist
Source: "AppComercial.ServerManager\bin\publish\appsettings.json"; DestDir: "{commonappdata}\AppComercial\ServerManager"; Flags: ignoreversion onlyifdoesntexist

[Dirs]
; Crear carpetas ProgramData con permisos de escritura para usuarios estándar (sin UAC)
Name: "{commonappdata}\AppComercial\Api"; Permissions: users-modify
Name: "{commonappdata}\AppComercial\ServerManager"; Permissions: users-modify

[Icons]
Name: "{group}\AppComercial Server Manager"; Filename: "{app}\ServerManager\AppComercial.ServerManager.exe"
Name: "{commondesktop}\AppComercial Server Manager"; Filename: "{app}\ServerManager\AppComercial.ServerManager.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
; Lanza el UI del Server Manager para que el usuario pueda ver que todo está corriendo
Filename: "{app}\ServerManager\AppComercial.ServerManager.exe"; Description: "Lanzar Server Manager ahora"; Flags: nowait postinstall skipifsilent

[Registry]
; Agregar el ServerManager al arranque de Windows para que inicie la API automáticamente cuando el usuario inicie sesión
Root: HKCU; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "AppComercialServerManager"; ValueData: """{app}\ServerManager\AppComercial.ServerManager.exe"""

[UninstallRun]
; Matar procesos durante desinstalación
Filename: "taskkill.exe"; Parameters: "/F /IM AppComercial.ServerManager.exe"; Flags: runhidden skipifdoesntexist
Filename: "taskkill.exe"; Parameters: "/F /IM AppComercial.Api.exe"; Flags: runhidden skipifdoesntexist

[UninstallDelete]
Type: filesandordirs; Name: "C:\AppComercialLogs"
Type: filesandordirs; Name: "{commonappdata}\AppComercial"

[Code]
var
  DbPage: TInputQueryWizardPage;
  SystemPage: TInputOptionWizardPage;

{ 1. VALIDACIÓN DEL SDK: Revisamos si CONTPAQi existe en el Registro }
function InitializeSetup(): Boolean;
var
  HasComercial: Boolean;
  HasFactura: Boolean;
begin
  { Checar Registro en nodos de 32 bits WOW6432Node o nativos }
  HasComercial := RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\Computación en Acción, SA CV\CONTPAQ I COMERCIAL');
  if not HasComercial then
    HasComercial := RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\Computación en Acción, SA CV\CONTPAQ I COMERCIAL');
    
  HasFactura := RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\Computación en Acción, SA CV\CONTPAQ I FACTURACION');
  if not HasFactura then
    HasFactura := RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\Computación en Acción, SA CV\CONTPAQ I FACTURACION');

  if (not HasComercial) and (not HasFactura) then begin
    MsgBox('No se detectó CONTPAQi Comercial Premium ni Factura Electrónica instalado en este equipo.' + #13#10#13#10 + 'Por favor instale CONTPAQi antes de intentar instalar AppComercial.', mbCriticalError, MB_OK);
    Result := False;
  end else begin
    Result := True;
  end;
end;

{ 2. PANTALLAS CUSTOMIZADAS (Wizard) }
procedure InitializeWizard;
begin
  SystemPage := CreateInputOptionPage(wpWelcome,
    'Sistema CONTPAQi', 'Seleccione su sistema CONTPAQi',
    'Seleccione el sistema CONTPAQi principal al cual se conectará el POS AppComercial.',
    True, False);
  SystemPage.Add('CONTPAQi Comercial Premium');
  SystemPage.Add('CONTPAQi Factura Electrónica');
  SystemPage.Values[0] := True; { Default Comercial }

  DbPage := CreateInputQueryPage(SystemPage.ID,
    'Configuración de Base de Datos SQL Server', 'Ingrese sus credenciales de SQL Desktop/Server',
    'Ingrese los datos de conexión para vincular AppComercial con la base de datos (SQL) de su Empresa.');
  DbPage.Add('Servidor SQL (Instancia, Ej. localhost\COMPAC2019):', False);
  DbPage.Add('Base de Datos Empresa (Ej. adEMPRESA):', False);
  DbPage.Add('Usuario SQL (Ej. sa):', False);
  DbPage.Add('Contraseña SQL:', True);
  DbPage.Add('Directorio de Empresa CONTPAQi (Ej. C:\Compac\Empresas\adEMPRESA):', False);

  { Valores por defecto }
  DbPage.Values[0] := 'localhost\COMPAC2019';
  DbPage.Values[1] := 'adEMPRESA';
  DbPage.Values[2] := 'sa';
  DbPage.Values[3] := '';
  DbPage.Values[4] := 'C:\Compac\Empresas\';
end;

{ 3. PRUEBA DE CONEXIÓN UTILIZANDO ADODB COM OBJECT }
function TestDbConnection(Server, Db, User, Pass: String): Boolean;
var
  ADOConn: Variant;
  ConnString: String;
begin
  Result := False;
  try
    ADOConn := CreateOleObject('ADODB.Connection');
    { Provider nativo de SQL Server OLE DB }
    ConnString := 'Provider=SQLOLEDB;Data Source=' + Server + ';Initial Catalog=' + Db + ';User ID=' + User + ';Password=' + Pass + ';';
    ADOConn.ConnectionString := ConnString;
    ADOConn.ConnectionTimeout := 5; { Segundos }
    ADOConn.Open;
    ADOConn.Close;
    
    MsgBox('Conexión exitosa a la base de datos SQL Server.', mbInformation, MB_OK);
    Result := True;
  except
    MsgBox('Error conectando a SQL Server.' + #13#10 + 'Verifique si la instancia existe, el usuario está habilitado, o si la contraseña es correcta.' + #13#10#13#10 + 'Servidor probado: ' + Server, mbCriticalError, MB_OK);
  end;
end;

{ Validamos justo cuando le dan a Siguiente en la ventana de BD }
function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = DbPage.ID then begin
    Result := TestDbConnection(DbPage.Values[0], DbPage.Values[1], DbPage.Values[2], DbPage.Values[3]);
  end;
end;

{ 4. INYECCIÓN DEL APPSETTINGS.JSON }
{ Los archivos se escriben en ProgramData, no en Program Files (que es solo lectura) }
procedure UpdateAppSettingsViaPS(AppPath, Server, Db, User, Pass, SistemaSTR, DirEmpresa: String);
var
  PSCommand: String;
  ConnStrA, ConnStrB, ConnStrC: String;
  ProgramDataPath: String;
  ResultCode: Integer;
begin
  { TrustServerCertificate=True previene errores de SSL en SQLExpress con certificados auto-firmados }
  ConnStrA := 'Server=' + Server + ';Database=' + Db + ';User Id=' + User + ';Password=' + Pass + ';Encrypt=False;TrustServerCertificate=True;';
  ConnStrB := 'Server=' + Server + ';Database=CompacWAdmin;User Id=' + User + ';Password=' + Pass + ';Encrypt=False;TrustServerCertificate=True;';
  ConnStrC := 'Server=' + Server + ';Database=RepositorioAdminPAQ;User Id=' + User + ';Password=' + Pass + ';Encrypt=False;TrustServerCertificate=True;';

  { Ruta de configuración mutable: ProgramData\AppComercial (escribible sin UAC) }
  ProgramDataPath := ExpandConstant('{commonappdata}') + '\AppComercial';

  { Script de PowerShell que actualiza AMBOS appsettings.json (Api y ServerManager) en ProgramData }
  PSCommand :=
    '$pathApi = ''' + ProgramDataPath + '\Api\appsettings.json''; ' +
    '$pathSm = ''' + ProgramDataPath + '\ServerManager\appsettings.json''; ' +
    'foreach ($path in @($pathApi, $pathSm)) { ' +
    '  if (Test-Path $path) { ' +
    '    $j = Get-Content -Raw $path | ConvertFrom-Json; ' +
    '    if (-not $j.ConnectionStrings) { $j | Add-Member -Type NoteProperty -Name ConnectionStrings -Value (New-Object PSObject) -Force }; ' +
    '    $j.ConnectionStrings | Add-Member -NotePropertyName ContpaqiComercial -NotePropertyValue ''' + ConnStrA + ''' -Force; ' +
    '    $j.ConnectionStrings | Add-Member -NotePropertyName CompacWAdmin -NotePropertyValue ''' + ConnStrB + ''' -Force; ' +
    '    $j.ConnectionStrings | Add-Member -NotePropertyName RepositorioAdminPAQ -NotePropertyValue ''' + ConnStrC + ''' -Force; ' +
    '    if (-not $j.Contpaqi) { $j | Add-Member -Type NoteProperty -Name Contpaqi -Value (New-Object PSObject) -Force }; ' +
    '    $j.Contpaqi | Add-Member -NotePropertyName Sistema -NotePropertyValue ''' + SistemaSTR + ''' -Force; ' +
    '    $j.Contpaqi | Add-Member -NotePropertyName DirectorioEmpresa -NotePropertyValue ''' + DirEmpresa + ''' -Force; ' +
    '    $j.Contpaqi | Add-Member -NotePropertyName Usuario -NotePropertyValue ''SUPERVISOR'' -Force; ' +
    '    $j.Contpaqi | Add-Member -NotePropertyName Contrasena -NotePropertyValue '''' -Force; ' +
    '    if (-not $j.ApiSettings) { $j | Add-Member -Type NoteProperty -Name ApiSettings -Value (New-Object PSObject) -Force }; ' +
    '    $j.ApiSettings | Add-Member -NotePropertyName ListenUrl -NotePropertyValue ''http://*:5271'' -Force; ' +
    '    $j | ConvertTo-Json -Depth 10 | Set-Content $path -Encoding UTF8; ' +
    '  } ' +
    '}';

  Exec('powershell.exe', '-NoProfile -NonInteractive -ExecutionPolicy Bypass -WindowStyle Hidden -Command "' + PSCommand + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  SistemaSTR: String;
begin
  if CurStep = ssPostInstall then begin
    if SystemPage.Values[0] then SistemaSTR := 'Comercial' else SistemaSTR := 'FacturaElectronica';

    { Values[0]=Server, [1]=DB, [2]=User, [3]=Pass, [4]=DirEmpresa }
    UpdateAppSettingsViaPS(
      ExpandConstant('{app}'),
      DbPage.Values[0],
      DbPage.Values[1],
      DbPage.Values[2],
      DbPage.Values[3],
      SistemaSTR,
      DbPage.Values[4]);
  end;
end;
