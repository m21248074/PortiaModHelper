using AddMoreItemsToShopEditor.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

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

        private List<ShopItemDisplayModel> _displayItemsList = new List<ShopItemDisplayModel>();

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
                            p.Buy_Price AS BuyPrice, 
                            p.Sell_Price AS SellPrice, 
                            p.Stack_Number AS StackNumber
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
                        _displayItemsList = _shopItemsList.Select(shopItem =>
                        {
                            _gameItemsDict.TryGetValue(shopItem.ID, out GameItem? gameItem);
                            _gameStoresDict.TryGetValue(shopItem.storeId, out GameStore? gameStore);

                            return new ShopItemDisplayModel
                            {
                                ID = shopItem.ID,
                                count = shopItem.count,
                                storeId = shopItem.storeId,
                                currency = shopItem.currency,
                                requireMission = shopItem.requireMission,
                                chance = shopItem.chance,

                                EnglishName = gameItem?.EnglishName ?? "Unknown",
                                TargetLangName = gameItem?.TargetLangName ?? "Unknown",
                                Description = gameItem?.Description ?? "No description available.",

                                StoreName = gameStore?.TargetLangName ?? $"Unknown Store ({shopItem.storeId})",
                                OpenTime = gameStore?.OpenTime ?? "-"
                            };
                        }).ToList();

                        DgModItems.ItemsSource = _displayItemsList;
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

        /// <summary>
        /// 處理刪除按鈕點擊
        /// </summary>
        private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ShopItemDisplayModel clickedItem)
            {
                var result = MessageBox.Show($"Are you sure you want to remove '{clickedItem.FormattedName}' from this store?",
                                             "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _displayItemsList.Remove(clickedItem);

                    DgModItems.ItemsSource = null;
                    DgModItems.ItemsSource = _displayItemsList;
                }
            }
        }

        /// <summary>
        /// 處理編輯按鈕點擊
        /// </summary>
        private void BtnEditItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ShopItemDisplayModel clickedItem)
            {
                EditItemDialog dialog = new EditItemDialog(clickedItem, _gameItemsDict, _gameStoresDict);

                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    ShopItemDisplayModel updatedItem = dialog.ResultItem;

                    int index = _displayItemsList.IndexOf(clickedItem);
                    if (index != -1)
                    {
                        _displayItemsList[index] = updatedItem;
                    }

                    DgModItems.ItemsSource = null;
                    DgModItems.ItemsSource = _displayItemsList;
                }
            }
        }

        /// <summary>
        /// 處理「新增物品」按鈕點擊
        /// </summary>
        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            int defaultItemId = _gameItemsDict.Keys.FirstOrDefault();
            int defaultStoreId = _gameStoresDict.Keys.FirstOrDefault();

            ShopItemDisplayModel newItemTemplate = new ShopItemDisplayModel
            {
                ID = defaultItemId,
                storeId = defaultStoreId,
                count = 99,
                currency = -1,
                requireMission = -1,
                chance = 1
            };

            EditItemDialog dialog = new EditItemDialog(newItemTemplate, _gameItemsDict, _gameStoresDict);
            dialog.Owner = this;
            dialog.Title = "Add New Item to Shop";

            if (dialog.ShowDialog() == true)
            {
                ShopItemDisplayModel createdItem = dialog.ResultItem;

                _displayItemsList.Add(createdItem);

                DgModItems.ItemsSource = null;
                DgModItems.ItemsSource = _displayItemsList;

                if (DgModItems.Items.Count > 0)
                {
                    DgModItems.ScrollIntoView(DgModItems.Items[DgModItems.Items.Count - 1]);
                }
            }
        }

        private async void BtnSaveJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(_jsonPath))
                {
                    string backupPath = _jsonPath + ".bak";

                    File.Copy(_jsonPath, backupPath, true);
                }

                var backToRawList = _displayItemsList.Select(x => new ShopItem
                {
                    ID = x.ID,
                    count = x.count,
                    storeId = x.storeId,
                    currency = x.currency,
                    requireMission = x.requireMission,
                    chance = x.chance
                }).ToList();

                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                string updatedJsonContent = JsonSerializer.Serialize(backToRawList, jsonOptions);

                await File.WriteAllTextAsync(_jsonPath, updatedJsonContent);

                MessageBox.Show("Mod configuration saved successfully!\n\n" +
                                "💡 A backup of the previous configuration has been created as '.bak'.",
                                "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save JSON data:\n{ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
