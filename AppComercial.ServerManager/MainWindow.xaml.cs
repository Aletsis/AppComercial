using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

// Resolver ambigüedades entre System.Windows y System.Windows.Forms
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using DoubleAnimation = System.Windows.Media.Animation.DoubleAnimation;
using RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;
using Clipboard = System.Windows.Clipboard;

namespace AppComercial.ServerManager
{
    public partial class MainWindow : Window
    {
        // ── Fields ──────────────────────────────────────────────────────────────
        private System.Windows.Forms.NotifyIcon? _notifyIcon;
        private bool _isReallyClosing = false;
        private DispatcherTimer _statusTimer = null!;
        private Process? _apiProcess;
        private DateTime? _apiStartTime;
        private bool _isPulsing = false;

        private readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(6)
        };

        // ── Paths ────────────────────────────────────────────────────────────────
        // Configuración mutable en ProgramData (escribible sin UAC, incluso en Program Files).
        // Los binarios siguen en AppContext.BaseDirectory (Program Files, solo lectura).
        private static readonly string ProgramDataDir =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "AppComercial");

        private static readonly string AppSettingsPath =
            Path.Combine(ProgramDataDir, "ServerManager", "appsettings.json");

        private static readonly string LogFilePath =
            @"C:\AppComercialLogs\ServerManager.log";

        private static readonly string ApiLogFilePath =
            @"C:\AppComercialLogs\ApiServer.log";

        // El exe de la API sigue en Program Files (solo se lee, no se escribe)
        private static readonly string ParentDir =
            Directory.GetParent(AppContext.BaseDirectory.TrimEnd('\\', '/'))?.FullName
            ?? AppContext.BaseDirectory;

        private static readonly string ApiSettingsPath =
            Path.Combine(ProgramDataDir, "Api", "appsettings.json");

        private static readonly string ApiExePath =
            Path.Combine(ParentDir, "Api", "AppComercial.Api.exe");

        private const string AutoStartRegKey =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AutoStartRegValue = "AppComercialServerManager";

        // ── Static constructor ──────────────────────────────────────────────────────
        static MainWindow()
        {
            // Directorio de logs del sistema
            if (!Directory.Exists(@"C:\AppComercialLogs"))
                Directory.CreateDirectory(@"C:\AppComercialLogs");

            // Crear carpetas de configuración en ProgramData si no existen
            var smConfigDir = Path.GetDirectoryName(AppSettingsPath)!;
            var apiConfigDir = Path.GetDirectoryName(ApiSettingsPath)!;
            try { Directory.CreateDirectory(smConfigDir); } catch { }
            try { Directory.CreateDirectory(apiConfigDir); } catch { }

            // Primera ejecución sin instalador (entorno de desarrollo):
            // copiar el template del directorio de instalación a ProgramData
            if (!File.Exists(AppSettingsPath))
            {
                var templatePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (File.Exists(templatePath))
                    try { File.Copy(templatePath, AppSettingsPath); } catch { }
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════════════════
        public MainWindow()
        {
            InitializeComponent();
            SetupNotifyIcon();

            Log("Interfaz WPF Server Manager iniciada.");
            Log($"Config: {AppSettingsPath}");

            LoadSettings();
            UpdateUrlDisplay();
            TryLoadApiVersion();

            _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _statusTimer.Tick += StatusTimer_Tick;
            _statusTimer.Start();

            // Revisión inicial inmediata
            StatusTimer_Tick(null, null);

            // Auto-start API si está configurado
            if (GetSettingBool("ApiSettings:AutoStartApi"))
                _ = StartApiAsync();
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  TIMER DE ESTADO
        // ══════════════════════════════════════════════════════════════════════════
        private void StatusTimer_Tick(object? sender, EventArgs? e)
        {
            try
            {
                bool isRunning = _apiProcess != null && !_apiProcess.HasExited;

                if (isRunning)
                {
                    TxtStatus.Text = "CORRIENDO";
                    TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                    StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                    BtnStart.IsEnabled = false;
                    BtnStop.IsEnabled = true;
                    BtnRestart.IsEnabled = true;

                    // Uptime
                    if (_apiStartTime.HasValue)
                        TxtUptime.Text = (DateTime.Now - _apiStartTime.Value).ToString(@"hh\:mm\:ss");

                    // PID
                    try { TxtPid.Text = $"PID: {_apiProcess!.Id}"; } catch { }

                    StartPulseAnimation();
                }
                else
                {
                    TxtStatus.Text = "DETENIDO";
                    TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
                    StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
                    BtnStart.IsEnabled = true;
                    BtnStop.IsEnabled = false;
                    BtnRestart.IsEnabled = false;
                    TxtUptime.Text = "--:--:--";
                    TxtPid.Text = "";
                    StopPulseAnimation();

                    // Limpiar proceso muerto
                    if (_apiProcess != null && _apiProcess.HasExited)
                    {
                        var exitCode = _apiProcess.ExitCode;
                        _apiProcess.Dispose();
                        _apiProcess = null;
                        _apiStartTime = null;
                        Log($"[!] La API se detuvo inesperadamente (ExitCode: {exitCode}). Revisa ApiServer.log.");
                    }
                }
            }
            catch (Exception ex)
            {
                if (sender != null) Log($"Error monitoreando proceso: {ex.Message}");
            }
        }

        // ── Pulse animation ────────────────────────────────────────────────────
        private void StartPulseAnimation()
        {
            if (_isPulsing) return;
            _isPulsing = true;
            var anim = new DoubleAnimation(1.0, 0.2,
                new Duration(TimeSpan.FromSeconds(0.85)))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            StatusDot.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        private void StopPulseAnimation()
        {
            if (!_isPulsing) return;
            _isPulsing = false;
            StatusDot.BeginAnimation(UIElement.OpacityProperty, null);
            StatusDot.Opacity = 1.0;
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  NOTIFY ICON (SYSTRAY)
        // ══════════════════════════════════════════════════════════════════════════
        private void SetupNotifyIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                    System.Reflection.Assembly.GetExecutingAssembly().Location),
                Visible = true,
                Text = "AppComercial Server Manager"
            };

            _notifyIcon.DoubleClick += (s, e) =>
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            };

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();

            var menuAbrir = contextMenu.Items.Add("Abrir Panel de Control");
            menuAbrir.Click += (s, e) => { Show(); WindowState = WindowState.Normal; Activate(); };

            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

            var menuSalir = contextMenu.Items.Add("Detener API y Salir");
            menuSalir.Click += async (s, e) =>
            {
                menuSalir.Enabled = false;
                try
                {
                    await Task.Run(() =>
                    {
                        if (_apiProcess != null && !_apiProcess.HasExited)
                        {
                            _apiProcess.Kill();
                            _apiProcess.WaitForExit(5000);
                        }
                    });
                }
                catch { /* ignorar */ }
                _isReallyClosing = true;
                Close();
            };

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  LOGGING
        // ══════════════════════════════════════════════════════════════════════════
        private void Log(string message)
        {
            var timeStamp = DateTime.Now.ToString("HH:mm:ss");
            var fullMessage = $"[{timeStamp}] {message}\n";

            try { File.AppendAllText(LogFilePath, fullMessage); } catch { }

            Dispatcher.Invoke(() =>
            {
                TxtLog.Text += fullMessage;
                LogScroll.ScrollToEnd();
            });
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  CONTROL DE LA API
        // ══════════════════════════════════════════════════════════════════════════
        private async void BtnStart_Click(object sender, RoutedEventArgs e)
            => await StartApiAsync();

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
            => await StopApiAsync();

        private async void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            Log("Reiniciando la API...");
            await StopApiAsync();
            await Task.Delay(1200);
            await StartApiAsync();
        }

        private async Task StartApiAsync()
        {
            try
            {
                BtnStart.IsEnabled = false;
                BtnRestart.IsEnabled = false;
                SaveSettings(silent: true);
                Log("Iniciando API en segundo plano...");

                if (!File.Exists(ApiExePath))
                {
                    Log($"[ERROR] No se encontró el ejecutable: {ApiExePath}");
                    BtnStart.IsEnabled = true;
                    return;
                }

                await Task.Run(() =>
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = ApiExePath,
                        UseShellExecute = true,
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Minimized,
                        WorkingDirectory = Path.GetDirectoryName(ApiExePath) ?? AppContext.BaseDirectory
                    };
                    _apiProcess = Process.Start(psi);
                    _apiStartTime = DateTime.Now;
                });

                Log("✓ API iniciada correctamente.");
                StatusTimer_Tick(null, null);
            }
            catch (Exception ex)
            {
                Log($"[ERROR] Al iniciar la API: {ex.Message}");
                BtnStart.IsEnabled = true;
            }
        }

        private async Task StopApiAsync()
        {
            try
            {
                BtnStop.IsEnabled = false;
                BtnRestart.IsEnabled = false;
                Log("Deteniendo API...");

                await Task.Run(() =>
                {
                    if (_apiProcess != null && !_apiProcess.HasExited)
                    {
                        _apiProcess.Kill();
                        _apiProcess.WaitForExit(5000);
                        _apiProcess.Dispose();
                        _apiProcess = null;
                        _apiStartTime = null;
                    }
                });

                Log("✓ API detenida correctamente.");
                StatusTimer_Tick(null, null);
            }
            catch (Exception ex)
            {
                Log($"[ERROR] Al detener API: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  TAB 1 — CONSOLA
        // ══════════════════════════════════════════════════════════════════════════
        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            TxtLog.Text = "";
            Log("Log limpiado.");
        }

        private void BtnOpenLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(LogFilePath))
                    Process.Start(new ProcessStartInfo { FileName = LogFilePath, UseShellExecute = true });
                else
                    MessageBox.Show("No se encontró el archivo de log.", "Log no encontrado",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { Log($"Error al abrir log: {ex.Message}"); }
        }

        private void TxtUrlDisplay_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
            => OpenSwagger();

        private void SidebarSwaggerLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
            => OpenSwagger();

        private void OpenSwagger()
        {
            try
            {
                var url = (TxtUrl?.Text ?? "http://localhost:5271").Replace("*", "localhost");
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex) { Log($"Error al abrir Swagger: {ex.Message}"); }
        }

        private void UpdateUrlDisplay()
        {
            var url = (TxtUrl?.Text ?? "http://*:5271").Replace("*", "localhost");
            if (TxtUrlDisplay != null) TxtUrlDisplay.Text = url;
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  TAB 2 — CONFIGURACIÓN
        // ══════════════════════════════════════════════════════════════════════════

        // ── Sistema selector ──────────────────────────────────────────────────────
        private string GetSistemaSeleccionado()
        {
            if (CmbSistema.SelectedItem is System.Windows.Controls.ComboBoxItem item)
                return item.Tag?.ToString() ?? "Comercial";
            return "Comercial";
        }

        private void SetSistemaSeleccionado(string tag)
        {
            foreach (System.Windows.Controls.ComboBoxItem item in CmbSistema.Items)
                if (item.Tag?.ToString() == tag) { CmbSistema.SelectedItem = item; return; }
        }

        private void BtnSaveConfig_Click(object sender, RoutedEventArgs e)
            => SaveSettings(silent: false);

        private void BtnBrowseEmpresa_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Seleccionar carpeta de empresa CONTPAQi",
                InitialDirectory = string.IsNullOrWhiteSpace(TxtDirectorioEmpresa.Text)
                    ? @"C:\Compac\Empresas" : TxtDirectorioEmpresa.Text
            };
            if (dialog.ShowDialog() == true)
            {
                TxtDirectorioEmpresa.Text = dialog.FolderName;
                Log($"Empresa seleccionada: {dialog.FolderName}");
            }
        }

        // ── Show/Hide passwords ───────────────────────────────────────────────────
        private void BtnShowContpaqiPass_Click(object sender, RoutedEventArgs e)
        {
            if (TxtPassword.Visibility == Visibility.Visible)
            {
                TxtPasswordVisible.Text = TxtPassword.Password;
                TxtPassword.Visibility = Visibility.Collapsed;
                TxtPasswordVisible.Visibility = Visibility.Visible;
                IconShowContpaqiPass.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;
            }
            else
            {
                TxtPassword.Password = TxtPasswordVisible.Text;
                TxtPasswordVisible.Visibility = Visibility.Collapsed;
                TxtPassword.Visibility = Visibility.Visible;
                IconShowContpaqiPass.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;
            }
        }

        private void BtnShowCsdPassword_Click(object sender, RoutedEventArgs e)
        {
            if (TxtCsdPassword.Visibility == Visibility.Visible)
            {
                TxtCsdPasswordVisible.Text = TxtCsdPassword.Password;
                TxtCsdPassword.Visibility = Visibility.Collapsed;
                TxtCsdPasswordVisible.Visibility = Visibility.Visible;
                IconShowCsdPassword.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;
            }
            else
            {
                TxtCsdPassword.Password = TxtCsdPasswordVisible.Text;
                TxtCsdPasswordVisible.Visibility = Visibility.Collapsed;
                TxtCsdPassword.Visibility = Visibility.Visible;
                IconShowCsdPassword.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;
            }
        }

        private void BtnShowSqlPass_Click(object sender, RoutedEventArgs e)
        {
            if (TxtSqlPass.Visibility == Visibility.Visible)
            {
                TxtSqlPassVisible.Text = TxtSqlPass.Password;
                TxtSqlPass.Visibility = Visibility.Collapsed;
                TxtSqlPassVisible.Visibility = Visibility.Visible;
                IconShowSqlPass.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;
            }
            else
            {
                TxtSqlPass.Password = TxtSqlPassVisible.Text;
                TxtSqlPassVisible.Visibility = Visibility.Collapsed;
                TxtSqlPass.Visibility = Visibility.Visible;
                IconShowSqlPass.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;
            }
        }

        // Helpers para obtener el valor actual de las contraseñas (independiente de si están visibles)
        private string GetContpaqiPassword()
            => TxtPassword.Visibility == Visibility.Visible
                ? TxtPassword.Password
                : TxtPasswordVisible.Text;

        private string GetCsdPassword()
            => TxtCsdPassword.Visibility == Visibility.Visible
                ? TxtCsdPassword.Password
                : TxtCsdPasswordVisible.Text;

        private string GetSqlPassword()
            => TxtSqlPass.Visibility == Visibility.Visible
                ? TxtSqlPass.Password
                : TxtSqlPassVisible.Text;

        // ── Probar conexión SQL ───────────────────────────────────────────────────
        private async void BtnTestSql_Click(object sender, RoutedEventArgs e)
        {
            var server = TxtSqlServer.Text.Trim();
            var db = TxtSqlDb.Text.Trim();
            var user = TxtSqlUser.Text.Trim();
            var pass = GetSqlPassword();

            if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(db))
            {
                TxtSqlTestResult.Text = "⚠ Servidor y BD son requeridos";
                TxtSqlTestResult.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
                return;
            }

            TxtSqlTestResult.Text = "Probando...";
            TxtSqlTestResult.Foreground = Brushes.Gray;
            BtnTestSql.IsEnabled = false;

            bool success = false;
            string errorMsg = "";

            await Task.Run(() =>
            {
                try
                {
                    string connStr = string.IsNullOrWhiteSpace(user)
                        ? $"Server={server};Database={db};Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connect Timeout=8;"
                        : $"Server={server};Database={db};User Id={user};Password={pass};Encrypt=False;TrustServerCertificate=True;Connect Timeout=8;";

                    using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                    conn.Open();
                    success = true;
                }
                catch (Exception ex) { errorMsg = ex.Message; }
            });

            BtnTestSql.IsEnabled = true;

            if (success)
            {
                TxtSqlTestResult.Text = "✓ Conectado";
                TxtSqlTestResult.Foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                Log($"[SQL] ✓ Conexión exitosa a {server}/{db}");
            }
            else
            {
                TxtSqlTestResult.Text = "✗ Sin conexión";
                TxtSqlTestResult.Foreground = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
                Log($"[SQL ERROR] {errorMsg}");
                MessageBox.Show($"No se pudo conectar:\n{errorMsg}", "Error SQL",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ── Exportar config ───────────────────────────────────────────────────────
        private void BtnBackupConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(AppSettingsPath))
                {
                    MessageBox.Show("No existe appsettings.json para exportar.", "Sin configuración",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var backupDir = Path.Combine(AppContext.BaseDirectory, "Backups");
                Directory.CreateDirectory(backupDir);
                var backupFile = Path.Combine(backupDir, $"appsettings_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                File.Copy(AppSettingsPath, backupFile, overwrite: true);

                Log($"✓ Config exportada → {backupFile}");
                MessageBox.Show($"Configuración exportada exitosamente a:\n{backupFile}",
                    "Exportación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { Log($"Error al exportar: {ex.Message}"); }
        }

        // ── SaveSettings (FIXED: merge en lugar de overwrite) ─────────────────────
        private void SaveSettings(bool silent)
        {
            try
            {
                // Validar campos requeridos si no es silent
                if (!silent)
                {
                    if (string.IsNullOrWhiteSpace(TxtSqlServer.Text) || string.IsNullOrWhiteSpace(TxtSqlDb.Text))
                    {
                        MessageBox.Show("El Servidor SQL y la Base de Datos son campos requeridos.",
                            "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                var server = TxtSqlServer.Text.Trim();
                var db = TxtSqlDb.Text.Trim();
                var user = TxtSqlUser.Text.Trim();
                var pass = GetSqlPassword();
                var listenUrl = TxtUrl.Text.Trim();

                // ── Escribir al appsettings del ServerManager ──────────────────────
                string jsonContent = File.Exists(AppSettingsPath)
                    ? File.ReadAllText(AppSettingsPath) : "{}";
                var root = JsonNode.Parse(jsonContent)?.AsObject() ?? new JsonObject();
                PatchCommonSections(root, server, db, user, pass, listenUrl);
                File.WriteAllText(AppSettingsPath,
                    root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

                // ── BUG FIX: Merge (no overwrite) en el appsettings de la API ──────
                // Se preservan ApiKey, JwtSettings, IpRateLimiting, Logging, etc.
                if (File.Exists(ApiSettingsPath))
                {
                    string apiJsonContent = File.ReadAllText(ApiSettingsPath);
                    var apiRoot = JsonNode.Parse(apiJsonContent)?.AsObject() ?? new JsonObject();
                    PatchCommonSections(apiRoot, server, db, user, pass, listenUrl);
                    File.WriteAllText(ApiSettingsPath,
                        apiRoot.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
                    Log("✓ appsettings sincronizado con la carpeta Api (merge).");
                }

                UpdateUrlDisplay();
                Log($"✓ Config guardada → SQL: {server} | BD: {db} | URL: {listenUrl}");

                if (!silent)
                    MessageBox.Show(
                        "Configuración guardada correctamente.\n\n" +
                        "Si la API está corriendo, usa 'REINICIAR API' para que apliquen los cambios.",
                        "Configuración guardada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log($"Error al guardar: {ex.Message}");
                if (!silent) MessageBox.Show($"Error: {ex.Message}", "Error al guardar",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Parchea SOLO las secciones que le pertenecen al ServerManager.
        /// Las demás secciones del JSON (ApiKey, JwtSettings, IpRateLimiting, etc.) NO se tocan.
        /// </summary>
        private void PatchCommonSections(JsonObject root, string server, string db,
            string user, string pass, string listenUrl)
        {
            // Contpaqi
            if (root["Contpaqi"] == null) root["Contpaqi"] = new JsonObject();
            root["Contpaqi"]!["Sistema"] = GetSistemaSeleccionado();
            root["Contpaqi"]!["DirectorioEmpresa"] = TxtDirectorioEmpresa.Text.Trim();
            root["Contpaqi"]!["Usuario"] = TxtUsuario.Text.Trim();
            root["Contpaqi"]!["Contrasena"] = GetContpaqiPassword();
            root["Contpaqi"]!["CsdPassword"] = GetCsdPassword();

            // ConnectionStrings (solo si hay servidor y BD)
            if (!string.IsNullOrWhiteSpace(server) && !string.IsNullOrWhiteSpace(db))
            {
                string BuildConn(string database) =>
                    $"Server={server};Database={database};User Id={user};Password={pass};" +
                    $"Encrypt=False;TrustServerCertificate=True;";

                if (root["ConnectionStrings"] == null) root["ConnectionStrings"] = new JsonObject();
                root["ConnectionStrings"]!["ContpaqiComercial"] = BuildConn(db);
                root["ConnectionStrings"]!["CompacWAdmin"] = BuildConn("CompacWAdmin");
                root["ConnectionStrings"]!["RepositorioAdminPAQ"] = BuildConn("RepositorioAdminPAQ");
            }

            // ApiSettings: solo actualizar ListenUrl. NO tocar ApiKey, RequireApiKeyForLogin ni AutoStartApi.
            if (root["ApiSettings"] == null) root["ApiSettings"] = new JsonObject();
            root["ApiSettings"]!["ListenUrl"] = listenUrl;
        }

        // ── LoadSettings ──────────────────────────────────────────────────────────
        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(AppSettingsPath))
                {
                    Log("appsettings.json no encontrado. Usando valores predeterminados.");
                    return;
                }

                var node = JsonNode.Parse(File.ReadAllText(AppSettingsPath));

                TxtDirectorioEmpresa.Text =
                    node?["Contpaqi"]?["DirectorioEmpresa"]?.GetValue<string>()
                    ?? @"C:\Compac\Empresas\";
                TxtUsuario.Text =
                    node?["Contpaqi"]?["Usuario"]?.GetValue<string>() ?? "SUPERVISOR";
                TxtPassword.Password =
                    node?["Contpaqi"]?["Contrasena"]?.GetValue<string>() ?? "";
                TxtCsdPassword.Password =
                    node?["Contpaqi"]?["CsdPassword"]?.GetValue<string>() ?? "";
                TxtUrl.Text =
                    node?["ApiSettings"]?["ListenUrl"]?.GetValue<string>() ?? "http://*:5271";

                var sistemaGuardado =
                    node?["Contpaqi"]?["Sistema"]?.GetValue<string>() ?? "Comercial";
                SetSistemaSeleccionado(sistemaGuardado);

                // Parsear SQL desde la cadena de conexión guardada
                var connStr =
                    node?["ConnectionStrings"]?["ContpaqiComercial"]?.GetValue<string>() ?? "";
                if (!string.IsNullOrEmpty(connStr))
                {
                    var parts = connStr.Split(';')
                        .Select(p => p.Split('=', 2))
                        .Where(p => p.Length == 2)
                        .ToDictionary(p => p[0].Trim().ToLower(), p => p[1].Trim());

                    TxtSqlServer.Text = parts.GetValueOrDefault("server", "");
                    TxtSqlDb.Text = parts.GetValueOrDefault("database", "");
                    TxtSqlUser.Text = parts.GetValueOrDefault("user id", "");
                    TxtSqlPass.Password = parts.GetValueOrDefault("password", "");
                }

                // Configuración de seguridad: preferir leer desde el appsettings de la API
                // para obtener ApiKey y JWT que son generados/guardados allí.
                var secPath = File.Exists(ApiSettingsPath) ? ApiSettingsPath : AppSettingsPath;
                var secNode = JsonNode.Parse(File.ReadAllText(secPath));

                TxtApiKey.Text =
                    secNode?["ApiSettings"]?["ApiKey"]?.GetValue<string>() ?? "";
                TxtJwtSecret.Text =
                    secNode?["JwtSettings"]?["SecretKey"]?.GetValue<string>() ?? "";
                TglRequireApiKey.IsChecked =
                    secNode?["ApiSettings"]?["RequireApiKeyForLogin"]?.GetValue<bool>() ?? false;
                TglAutoStartApi.IsChecked =
                    secNode?["ApiSettings"]?["AutoStartApi"]?.GetValue<bool>() ?? false;

                // Auto-inicio: leer desde registro de Windows
                bool autoStartEnabled = IsAutoStartEnabled();
                TglAutoStart.IsChecked = autoStartEnabled;
                TglAutoStartApi.IsEnabled = autoStartEnabled;

                Log("✓ Configuración cargada.");
            }
            catch (Exception ex) { Log($"Error al leer config: {ex.Message}"); }
        }

        private bool GetSettingBool(string path)
        {
            try
            {
                if (!File.Exists(AppSettingsPath)) return false;
                var parts = path.Split(':');
                JsonNode? node = JsonNode.Parse(File.ReadAllText(AppSettingsPath));
                foreach (var p in parts) node = node?[p];
                return node?.GetValue<bool>() ?? false;
            }
            catch { return false; }
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  TAB 3 — ESTADO DEL SISTEMA
        // ══════════════════════════════════════════════════════════════════════════
        private async void BtnRefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            BtnRefreshStatus.IsEnabled = false;
            await RefreshSystemStatusAsync();
            BtnRefreshStatus.IsEnabled = true;
        }

        private async Task RefreshSystemStatusAsync()
        {
            // ── Proceso ────────────────────────────────────────────────────────────
            bool isRunning = _apiProcess != null && !_apiProcess.HasExited;
            var green = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
            var red = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
            var orange = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));

            DiagProcessStatus.Text = isRunning ? "✓ Corriendo" : "✗ Detenido";
            DiagProcessStatus.Foreground = isRunning ? green : red;

            if (isRunning && _apiProcess != null)
            {
                try
                {
                    DiagPid.Text = _apiProcess.Id.ToString();
                    _apiProcess.Refresh();
                    var ramMb = _apiProcess.WorkingSet64 / (1024.0 * 1024.0);
                    DiagRam.Text = $"{ramMb:F1} MB";
                }
                catch { DiagPid.Text = "—"; DiagRam.Text = "—"; }
            }
            else
            {
                DiagPid.Text = "—";
                DiagRam.Text = "—";
            }

            DiagUptime.Text = _apiStartTime.HasValue
                ? (DateTime.Now - _apiStartTime.Value).ToString(@"hh\:mm\:ss")
                : "—";

            // ── HTTP ───────────────────────────────────────────────────────────────
            var rawUrl = (TxtUrl?.Text ?? "http://*:5271").Replace("*", "localhost");
            DiagUrl.Text = rawUrl;
            try
            {
                var sw = Stopwatch.StartNew();
                var response = await _httpClient.GetAsync(rawUrl);
                sw.Stop();
                DiagHttp.Text = $"✓ {(int)response.StatusCode} {response.StatusCode}";
                DiagHttp.Foreground = green;
                DiagLatency.Text = $"{sw.ElapsedMilliseconds} ms";
            }
            catch
            {
                DiagHttp.Text = "✗ No accesible";
                DiagHttp.Foreground = red;
                DiagLatency.Text = "—";
            }

            // ── SQL ────────────────────────────────────────────────────────────────
            var server = TxtSqlServer.Text.Trim();
            var db = TxtSqlDb.Text.Trim();
            DiagSqlServerLabel.Text = string.IsNullOrEmpty(server) ? "(no configurado)" : server;
            DiagSqlDb.Text = string.IsNullOrEmpty(db) ? "(no configurada)" : db;

            if (!string.IsNullOrEmpty(server) && !string.IsNullOrEmpty(db))
            {
                bool sqlOk = false;
                await Task.Run(() =>
                {
                    try
                    {
                        var u = TxtSqlUser.Text.Trim();
                        var p = GetSqlPassword();
                        string cs = string.IsNullOrWhiteSpace(u)
                            ? $"Server={server};Database={db};Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connect Timeout=5;"
                            : $"Server={server};Database={db};User Id={u};Password={p};Encrypt=False;TrustServerCertificate=True;Connect Timeout=5;";
                        using var conn = new Microsoft.Data.SqlClient.SqlConnection(cs);
                        conn.Open();
                        sqlOk = true;
                    }
                    catch { }
                });
                DiagSqlStatus.Text = sqlOk ? "✓ Conectado" : "✗ Sin conexión";
                DiagSqlStatus.Foreground = sqlOk ? green : red;
            }
            else
            {
                DiagSqlStatus.Text = "⚠ No configurado";
                DiagSqlStatus.Foreground = orange;
            }

            // ── SDK / Empresa ──────────────────────────────────────────────────────
            var empresaPath = TxtDirectorioEmpresa.Text.Trim();
            DiagEmpresaPath.Text = string.IsNullOrEmpty(empresaPath) ? "(no configurado)" : empresaPath;
            DiagSistema.Text = GetSistemaSeleccionado();

            if (!string.IsNullOrEmpty(empresaPath))
            {
                bool folderOk = Directory.Exists(empresaPath);
                DiagEmpresaStatus.Text = folderOk ? "✓ Accesible" : "✗ No encontrada";
                DiagEmpresaStatus.Foreground = folderOk ? green : red;
            }
            else
            {
                DiagEmpresaStatus.Text = "⚠ No configurado";
                DiagEmpresaStatus.Foreground = orange;
            }

            // ── Log preview (últimas 25 líneas del ApiServer.log) ──────────────────
            var apiLogPath = File.Exists(ApiLogFilePath) ? ApiLogFilePath : LogFilePath;
            try
            {
                if (File.Exists(apiLogPath))
                {
                    var lines = File.ReadLines(apiLogPath).TakeLast(25);
                    TxtApiLogPreview.Text = string.Join("\n", lines);
                    DiagLogScroll.ScrollToEnd();
                }
                else
                    TxtApiLogPreview.Text = "(archivo de log no encontrado)";
            }
            catch { TxtApiLogPreview.Text = "(error al leer el log)"; }
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  TAB 4 — SEGURIDAD
        // ══════════════════════════════════════════════════════════════════════════
        private void BtnCopyApiKey_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtApiKey.Text))
            {
                Clipboard.SetText(TxtApiKey.Text);
                Log("✓ API Key copiada al portapapeles.");
            }
        }

        private void BtnRegenApiKey_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "¿Regenerar la API Key?\n\nTodos los clientes que usen la clave actual dejarán de " +
                "autenticarse hasta que actualicen su configuración.",
                "Regenerar API Key", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var newKey = Convert.ToBase64String(
                    System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
                TxtApiKey.Text = newKey;
                Log("✓ Nueva API Key generada. Presiona 'GUARDAR CONFIGURACIÓN DE SEGURIDAD' para aplicar.");
            }
        }

        private void BtnCopyJwtSecret_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtJwtSecret.Text))
            {
                Clipboard.SetText(TxtJwtSecret.Text);
                Log("✓ JWT Secret copiado al portapapeles.");
            }
        }

        private void BtnRegenJwtSecret_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "¿Regenerar el JWT Secret?\n\nTodos los tokens activos quedarán invalidados " +
                "inmediatamente al reiniciar la API.",
                "Regenerar JWT Secret", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var newSecret = $"{Guid.NewGuid()}_SuperSecretKeyForProductionLongEnough256Bits";
                TxtJwtSecret.Text = newSecret;
                Log("✓ Nuevo JWT Secret generado. Presiona 'GUARDAR CONFIGURACIÓN DE SEGURIDAD' para aplicar.");
            }
        }

        private void BtnSaveSecurityConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSecurityToFile(AppSettingsPath);
                if (File.Exists(ApiSettingsPath))
                    SaveSecurityToFile(ApiSettingsPath);

                Log("✓ Configuración de seguridad guardada.");
                MessageBox.Show(
                    "Configuración de seguridad guardada correctamente.\n\n" +
                    "Reinicia la API para que apliquen los cambios.",
                    "Seguridad guardada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log($"Error al guardar seguridad: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSecurityToFile(string path)
        {
            string jsonContent = File.Exists(path) ? File.ReadAllText(path) : "{}";
            var root = JsonNode.Parse(jsonContent)?.AsObject() ?? new JsonObject();

            if (root["ApiSettings"] == null) root["ApiSettings"] = new JsonObject();
            root["ApiSettings"]!["ApiKey"] = TxtApiKey.Text;
            root["ApiSettings"]!["RequireApiKeyForLogin"] = TglRequireApiKey.IsChecked ?? false;
            root["ApiSettings"]!["AutoStartApi"] = TglAutoStartApi.IsChecked ?? false;

            if (root["JwtSettings"] == null) root["JwtSettings"] = new JsonObject();
            root["JwtSettings"]!["SecretKey"] = TxtJwtSecret.Text;

            File.WriteAllText(path,
                root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }

        // ── Auto-inicio con Windows ───────────────────────────────────────────────
        private bool IsAutoStartEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(AutoStartRegKey);
                return key?.GetValue(AutoStartRegValue) != null;
            }
            catch { return false; }
        }

        private void TglAutoStart_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(AutoStartRegKey, writable: true);
                var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                key?.SetValue(AutoStartRegValue, $"\"{exePath}\"");
                TglAutoStartApi.IsEnabled = true;
                Log("✓ Auto-inicio con Windows activado.");
            }
            catch (Exception ex) { Log($"Error al activar auto-inicio: {ex.Message}"); }
        }

        private void TglAutoStart_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(AutoStartRegKey, writable: true);
                key?.DeleteValue(AutoStartRegValue, throwOnMissingValue: false);
                TglAutoStartApi.IsEnabled = false;
                TglAutoStartApi.IsChecked = false;
                Log("✓ Auto-inicio con Windows desactivado.");
            }
            catch (Exception ex) { Log($"Error al desactivar auto-inicio: {ex.Message}"); }
        }

        // ── Versión de la API ─────────────────────────────────────────────────────
        private void TryLoadApiVersion()
        {
            try
            {
                if (File.Exists(ApiExePath))
                {
                    var v = FileVersionInfo.GetVersionInfo(ApiExePath);
                    TxtApiVersion.Text = $"API v{v.FileVersion ?? "—"}";
                }
                else
                {
                    TxtApiVersion.Text = "API (no encontrada)";
                }
            }
            catch { TxtApiVersion.Text = "API v—"; }
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  WINDOW LIFECYCLE
        // ══════════════════════════════════════════════════════════════════════════
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_isReallyClosing)
            {
                e.Cancel = true;
                Hide();
                try
                {
                    if (_apiProcess != null && !_apiProcess.HasExited)
                        _notifyIcon?.ShowBalloonTip(3000, "AppComercial Server",
                            "El panel se ocultó en la bandeja del sistema. La API sigue activa.",
                            System.Windows.Forms.ToolTipIcon.Info);
                }
                catch { }
                return;
            }
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }

            _statusTimer?.Stop();
            _httpClient?.Dispose();

            try
            {
                if (_apiProcess != null && !_apiProcess.HasExited)
                    _apiProcess.Kill();
            }
            catch { }

            base.OnClosed(e);
            System.Windows.Application.Current.Shutdown(); // BUG FIX: eliminado Process.Kill() agresivo
        }
    }
}