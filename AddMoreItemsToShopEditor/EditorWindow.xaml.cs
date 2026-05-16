using AddMoreItemsToShopEditor.Models;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Text.Json;
using System.Windows;
using Dapper;

namespace AddMoreItemsToShopEditor
{
    /// <summary>
    /// EditorWindow.xaml 的互動邏輯
    /// </summary>
    public partial class EditorWindow : Window
    {
        private string _gamePath;
        private string _jsonPath;
        private string _languageColumn;

        private List<ShopItem>? _shopItemsList = new List<ShopItem>();

        private Dictionary<int, GameItem> _gameItemsDict = new Dictionary<int, GameItem>();
        private Dictionary<int, GameStore> _gameStoresDict = new Dictionary<int, GameStore>();

        public EditorWindow(string gamePath, string jsonPath, string languageColumn)
        {
            InitializeComponent();

            _gamePath = gamePath;
            _jsonPath = jsonPath;
            _languageColumn = languageColumn;

            TxtPassedJsonPath.Text = _jsonPath;

            this.Loaded += EditorWindow_Loaded;
        }

        private async void EditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGameDatabase();

            await LoadJsonConfigAsyn();
        }

        private void LoadGameDatabase()
        {
            string dbPath = Path.Combine(_gamePath, "Portia_Data", "StreamingAssets", "CccData", "LocalDb.bytes");
            if (!File.Exists(dbPath)) return;

            string connectionString = $"Data Source={dbPath};";

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    string itemSqlQuery = $@"
                        SELECT 
                            p.Props_Id AS ID, 
                            t1.English AS EnglishName, 
                            t1.{_languageColumn} AS TargetLangName,
                            t2.{_languageColumn} AS Description,
                            p.Buy_Price, 
                            p.Sell_Price, 
                            p.Stack_Number
                        FROM Props_total_table p
                        LEFT JOIN Translation_hint t1 ON p.Props_Name = t1.ID
                        LEFT JOIN Translation_hint t2 ON p.Props_Explain_One = t2.ID";

                    var items = connection.Query<GameItem>(itemSqlQuery).ToList();
                    _gameItemsDict = items.ToDictionary(x => x.ID, x => x);

                    string storeSqlQuery = $@"
                        SELECT 
                            s.store_id AS StoreId,
                            t1.English AS EnglishName,
                            t1.{_languageColumn} AS TargetLangName,
                            s.open_time AS OpenTime
                        FROM Store_4_0 s
                        LEFT JOIN Translation_hint t1 ON s.name_id = t1.ID";

                    var stores = connection.Query<GameStore>(storeSqlQuery).ToList();

                    _gameStoresDict = stores.ToDictionary(x => x.StoreId, x => x);


                    MessageBox.Show($"Database loaded successfully!\n\n" +
                                    $"Items: {_gameItemsDict.Count} loaded.\n" +
                                    $"Shops: {_gameStoresDict.Count} loaded.",
                                    "Database Connected", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SQL Dynamic Query Error:\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadJsonConfigAsyn()
        {
            try
            {
                if (File.Exists(_jsonPath))
                {
                    string jsonContent = await File.ReadAllTextAsync(_jsonPath);

                    _shopItemsList = JsonSerializer.Deserialize<List<ShopItem>>(jsonContent);

                    if (_shopItemsList != null && _shopItemsList.Count > 0)
                    {
                        var displayList = _shopItemsList.Select(shopItem =>
                        {
                            _gameItemsDict.TryGetValue(shopItem.ID, out GameItem? gameItem);
                            _gameStoresDict.TryGetValue(shopItem.storeId, out GameStore? gameStore);

                            return new
                            {
                                ID = shopItem.ID,
                                Count = shopItem.count,
                                StoreId = shopItem.storeId,
                                Chance = shopItem.chance,
                                EnglishName = gameItem?.EnglishName ?? "Unknown",
                                TargetLangName = gameItem?.TargetLangName ?? "Unknown",
                                Description = gameItem?.Description ?? "No description available.",
                                StoreName = gameStore?.TargetLangName ?? $"Unknown Store ({shopItem.storeId})",
                                OpenTime = gameStore?.OpenTime ?? "-"
                            };
                        }).ToList();

                        DgModItems.ItemsSource = displayList;
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
