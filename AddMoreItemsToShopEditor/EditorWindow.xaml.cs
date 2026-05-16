using AddMoreItemsToShopEditor.Models;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace AddMoreItemsToShopEditor
{
    /// <summary>
    /// EditorWindow.xaml 的互動邏輯
    /// </summary>
    public partial class EditorWindow : Window
    {
        private string _gamePath;
        private string _jsonPath;

        private List<ShopItem>? _shopItemsList = new List<ShopItem>();

        public EditorWindow(string gamePath, string jsonPath)
        {
            InitializeComponent();

            _gamePath = gamePath;
            _jsonPath = jsonPath;

            TxtPassedJsonPath.Text = _jsonPath;

            this.Loaded += EditorWindow_Loaded;
        }

        private async void EditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(_jsonPath))
                {
                    string jsonContent = await File.ReadAllTextAsync(_jsonPath);

                    _shopItemsList = JsonSerializer.Deserialize<List<ShopItem>>(jsonContent);

                    if (_shopItemsList != null && _shopItemsList.Count > 0)
                    {
                        DgModItems.ItemsSource = _shopItemsList;
                    }
                    else
                    {
                        MessageBox.Show("The JSON file is empty or contains no items.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load JSON data in Editor Workspace:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
