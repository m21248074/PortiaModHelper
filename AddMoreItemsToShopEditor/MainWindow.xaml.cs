using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;


namespace AddMoreItemsToShopEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string GameFolderName = "My Time at Portia";
        private readonly string ModJsonRelativePath = Path.Combine("Mods", "AddMoreItemsToShop", "assets", "Add More Items to Shops.json");
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string? detectedPath = AutoDetectPortiaPath();

            if (!string.IsNullOrEmpty(detectedPath))
            {
                TxtGamePath.Text = detectedPath;
                TxtStatus.Text = "Steam game directory detected automatically!";
                TxtStatus.Foreground = System.Windows.Media.Brushes.Green;
                BtnLoadConfig.IsEnabled = true;
            }
            else
            {
                TxtStatus.Text = "Could not locate the game. Please click 'Browse...' to select it manually.";
                TxtStatus.Foreground = System.Windows.Media.Brushes.Red;
                BtnLoadConfig.IsEnabled = false;
            }
        }

        /// <summary>
        /// Automatically detects the installation path of My Time at Portia from Steam Registry.
        /// </summary>
        private string? AutoDetectPortiaPath()
        {
            try
            {
                // 1. Find Steam installation path
                string registryKey = @"SOFTWARE\WOW6432Node\Valve\Steam";
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryKey))
                {
                    if (key != null)
                    {
                        string? steamPath = key.GetValue("InstallPath")?.ToString();
                        if (!string.IsNullOrEmpty(steamPath))
                        {
                            // 2. Check the default SteamApps location
                            string defaultLibraryPath = Path.Combine(steamPath, "steamapps", "common", GameFolderName);
                            if (Directory.Exists(defaultLibraryPath))
                            {
                                return defaultLibraryPath;
                            }
                        }
                    }
                }
            }
            catch (Exception) { }

            return null;
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            bool currentBtnLoadConfigIsEnabled = BtnLoadConfig.IsEnabled;
            var folderDialog = new OpenFolderDialog
            {
                Title = "Select 'My Time at Portia' Installation Folder (containing Portia.exe)",
                InitialDirectory = @"C:\Program Files (x86)\Steam\steamapps\common"
            };

            if (folderDialog.ShowDialog() == true)
            {
                string selectedPath = folderDialog.FolderName;

                if (File.Exists(Path.Combine(selectedPath, "Portia.exe")))
                {
                    TxtGamePath.Text = selectedPath;
                    TxtStatus.Text = "Game directory updated successfully.";
                    TxtStatus.Foreground = System.Windows.Media.Brushes.Green;
                    BtnLoadConfig.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Could not find 'Portia.exe' in the selected folder.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    BtnLoadConfig.IsEnabled = currentBtnLoadConfigIsEnabled;
                }
            }
        }

        private async void BtnLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            string gamePath = TxtGamePath.Text;
            string fullJsonPath = Path.Combine(gamePath, ModJsonRelativePath);

            if (!File.Exists(fullJsonPath))
            {
                MessageBox.Show($"Could not find the Mod configuration file at:\n{fullJsonPath}\n\nPlease check if the 'AddMoreItemsToShop' Mod is installed correctly.",
                                "Missing JSON File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                TxtStatus.Text = "Validating JSON format...";
                TxtStatus.Foreground = System.Windows.Media.Brushes.Orange;

                string jsonContent = await File.ReadAllTextAsync(fullJsonPath);

                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    EditorWindow editorWorkspace = new EditorWindow(gamePath, fullJsonPath);

                    editorWorkspace.Show();

                    this.Close();
                }
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"The JSON file format is corrupted or invalid!\n\nError details:\n{ex.Message}",
                                "JSON Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Validation failed. Bad JSON format.";
                TxtStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while reading the file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}