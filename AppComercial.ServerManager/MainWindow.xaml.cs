using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AppComercial.ServerManager
{
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon? _notifyIcon;
        private bool _isReallyClosing = false;
        private DispatcherTimer _statusTimer;
        private const string ServiceName = "AppComercialApi";

        // Rutas al appsettings.json. En entorno Productivo el Installer separa \Api y \ServerManager
        private static readonly string AppSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        private static readonly string ApiSettingsPath = Path.Combine(Directory.GetParent(AppContext.BaseDirectory)?.FullName ?? AppContext.BaseDirectory, "Api", "appsettings.json");

        public MainWindow()
        {
            InitializeComponent();
            SetupNotifyIcon();
            Log("Interfaz WPF Server Manager iniciada.");
            Log($"Leyendo config UI desde: {AppSettingsPath}");
            LoadSettings();

            _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _statusTimer.Tick += StatusTimer_Tick;
            _statusTimer.Start();
            
            // Forzar una revisión inicial síncrona visualmente (opcional)
            StatusTimer_Tick(null, null);
        }

        private void StatusTimer_Tick(object? sender, EventArgs? e)
        {
            try
            {
                using var sc = new ServiceController(ServiceName);
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    TxtStatus.Text = "CORRIENDO (SERVICIO)";
                    TxtStatus.Foreground = System.Windows.Media.Brushes.LimeGreen;
                    BtnStart.IsEnabled = false;
                    BtnStop.IsEnabled = true;
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    TxtStatus.Text = "DETENIDO (SERVICIO)";
                    TxtStatus.Foreground = System.Windows.Media.Brushes.Tomato;
                    BtnStart.IsEnabled = true;
                    BtnStop.IsEnabled = false;
                }
                else
                {
                    TxtStatus.Text = sc.Status.ToString().ToUpper();
                    TxtStatus.Foreground = System.Windows.Media.Brushes.Orange;
                    BtnStart.IsEnabled = false;
                    BtnStop.IsEnabled = false;
                }
            }
            catch (InvalidOperationException)
            {
                // El servicio no está instalado en esta PC
                TxtStatus.Text = "SERVICIO NO INSTALADO";
                TxtStatus.Foreground = System.Windows.Media.Brushes.Gray;
                BtnStart.IsEnabled = false;
                BtnStop.IsEnabled = false;
                
                // Detenemos el timer para no ciclar logs de error
                if (sender != null) 
                {
                    _statusTimer.Stop();
                    Log("Servicio de Windows 'AppComercialApi' no detectado. ¿Ya ejecutaste el Instalador?");
                }
            }
            catch (Exception ex) 
            {
                // Ignorar pero registrar otros errores (falta de privilegios)
                if (sender != null)
                {
                   Log($"Error accediendo al Servicio: {ex.Message}. Asegúrate de arrancar como Administrador.");
                   _statusTimer.Stop();
                }
            }
        }

        private void SetupNotifyIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location),
                Visible = true,
                Text = "Server Manager CONTPAQi"
            };

            _notifyIcon.DoubleClick += (s, e) =>
            {
                Show();
                WindowState = WindowState.Normal;
            };

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            var menuAbrir = contextMenu.Items.Add("Abrir Panel de Control");
            menuAbrir.Click += (s, e) =>
            {
                Show();
                WindowState = WindowState.Normal;
            };
            
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            var menuSalir = contextMenu.Items.Add("Detener Servicio y Salir");
            menuSalir.Click += async (s, e) =>
            {
                menuSalir.Enabled = false;
                try
                {
                    await Task.Run(() =>
                    {
                        using var sc = new ServiceController(ServiceName);
                        if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                        {
                            if (sc.CanStop)
                            {
                                sc.Stop();
                                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15));
                            }
                        }
                    });
                }
                catch { /* Silencioso, forzamos cierre después */ }

                _isReallyClosing = true;
                Close();
            };

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                TxtLog.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
                LogScroll.ScrollToEnd();
            });
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnStart.IsEnabled = false;
                SaveSettings(silent: true);
                Log("Enviando orden de INICIO al servicio Windows de API...");

                await Task.Run(() =>
                {
                    using var sc = new ServiceController(ServiceName);
                    if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15));
                    }
                });

                Log("El servicio de API se ha iniciado correctamente.");
                _statusTimer.Start();
                StatusTimer_Tick(null, null); // Refrescar UI inmeditamente
            }
            catch (Exception ex)
            {
                Log($"Error al iniciar servicio: {ex.Message} (Requiere arrancar como Administrador)");
                BtnStart.IsEnabled = true;
            }
        }

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnStop.IsEnabled = false;
                Log("Enviando orden de DETENCIÓN al servicio Windows de API...");

                await Task.Run(() =>
                {
                    using var sc = new ServiceController(ServiceName);
                    if (sc.CanStop && sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                });

                Log("El servicio de API se ha detenido correctamente.");
                StatusTimer_Tick(null, null);
            }
            catch (Exception ex)
            {
                Log($"Error al detener servicio: {ex.Message} (Requiere arrancar como Administrador)");
                BtnStop.IsEnabled = true;
            }
        }

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
                    ? @"C:\Compac\Empresas"
                    : TxtDirectorioEmpresa.Text
            };

            if (dialog.ShowDialog() == true)
            {
                TxtDirectorioEmpresa.Text = dialog.FolderName;
                Log($"Empresa seleccionada: {dialog.FolderName}");
            }
        }

        private void SaveSettings(bool silent)
        {
            try
            {
                string jsonContent = File.Exists(AppSettingsPath)
                    ? File.ReadAllText(AppSettingsPath) : "{}";

                var root = JsonNode.Parse(jsonContent)?.AsObject() ?? new JsonObject();

                if (root["Contpaqi"] == null) root["Contpaqi"] = new JsonObject();
                root["Contpaqi"]!["Sistema"]           = GetSistemaSeleccionado();
                root["Contpaqi"]!["DirectorioEmpresa"] = TxtDirectorioEmpresa.Text.Trim();
                root["Contpaqi"]!["Usuario"]           = TxtUsuario.Text.Trim();
                root["Contpaqi"]!["Contrasena"]        = TxtPassword.Password;

                if (root["ApiSettings"] == null) root["ApiSettings"] = new JsonObject();
                root["ApiSettings"]!["ListenUrl"] = TxtUrl.Text.Trim();

                string finalJson = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

                // Update UI AppSettings
                File.WriteAllText(AppSettingsPath, finalJson);

                // Update API AppSettings if it exists in a parallel directory structure (from Installer)
                if (File.Exists(ApiSettingsPath))
                {
                    File.WriteAllText(ApiSettingsPath, finalJson);
                    Log("Se sincronizó appsettings con la carpeta de la API Windows Service.");
                }

                Log($"Config guardada → Empresa: [{TxtDirectorioEmpresa.Text.Trim()}] | Usuario: [{TxtUsuario.Text.Trim()}]");

                if (!silent)
                    System.Windows.MessageBox.Show(
                        "Configuración guardada.\nSi el servicio está corriendo, debes Reiniciarlo (Detener e Iniciar) para que apliquen los cambios en la API.",
                        "Configuración guardada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log($"Error al guardar: {ex.Message}");
                if (!silent) System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

                TxtDirectorioEmpresa.Text = node?["Contpaqi"]?["DirectorioEmpresa"]?.GetValue<string>()
                                            ?? @"C:\Compac\Empresas\adEMPRESA_DE_PRUEBA";
                TxtUsuario.Text           = node?["Contpaqi"]?["Usuario"]?.GetValue<string>() ?? "SUPERVISOR";
                TxtPassword.Password      = node?["Contpaqi"]?["Contrasena"]?.GetValue<string>() ?? "";
                TxtUrl.Text               = node?["ApiSettings"]?["ListenUrl"]?.GetValue<string>()
                                            ?? "http://localhost:5271";

                var sistemaGuardado = node?["Contpaqi"]?["Sistema"]?.GetValue<string>() ?? "Comercial";
                SetSistemaSeleccionado(sistemaGuardado);

                Log("Configuración cargada desde appsettings.json.");
            }
            catch (Exception ex) { Log($"Error al leer config: {ex.Message}"); }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_isReallyClosing)
            {
                e.Cancel = true;
                Hide();
                
                try
                {
                    using var sc = new ServiceController(ServiceName);
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        _notifyIcon?.ShowBalloonTip(3000, "AppComercial Server", "El panel se ocultó. La API sigue activa en segundo plano.", System.Windows.Forms.ToolTipIcon.Info);
                    }
                }
                catch { /* Ignorar si no hay permisos o no está instalado */ }

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

            base.OnClosed(e);
            System.Windows.Application.Current.Shutdown();
            Process.GetCurrentProcess().Kill(); 
        }
    }
}