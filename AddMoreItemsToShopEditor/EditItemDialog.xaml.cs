using AddMoreItemsToShopEditor.Models;
using System.Windows;
using System.Windows.Controls;

namespace AddMoreItemsToShopEditor
{
    /// <summary>
    /// EditItemDialog.xaml 的互動邏輯
    /// </summary>
    public partial class EditItemDialog : Window
    {
        private readonly Dictionary<int, GameItem> _itemDict;
        private readonly Dictionary<int, GameStore> _storeDict;

        private readonly List<GameItem> _allGameItems;
        private readonly List<GameStore> _allGameStores;

        private int _currentSelectedItemId;
        private int _currentSelectedStoreId;

        public ShopItemDisplayModel ResultItem { get; private set; }

        public EditItemDialog(ShopItemDisplayModel item, Dictionary<int, GameItem> itemDict, Dictionary<int, GameStore> storeDict)
        {
            InitializeComponent();

            _itemDict = itemDict;
            _storeDict = storeDict;

            _allGameItems = _itemDict.Values.OrderBy(x => x.ID).ToList();
            _allGameStores = _storeDict.Values.OrderBy(x => x.StoreId).ToList();

            _currentSelectedItemId = item.ID;
            _currentSelectedStoreId = item.storeId;
            TxtCount.Text = item.count.ToString();
            TxtCurrency.Text = item.currency.ToString();
            TxtRequireMission.Text = item.requireMission.ToString();
            TxtChance.Text = item.chance.ToString();

            var currentRefItem = _allGameItems.FirstOrDefault(x => x.ID == item.ID);
            if (currentRefItem != null)
            {
                DgGameItemsRef.SelectedItem = currentRefItem;
                DgGameItemsRef.ScrollIntoView(currentRefItem);
            }

            var currentRefStore = _allGameStores.FirstOrDefault(x => x.StoreId == item.storeId);
            if (currentRefStore != null)
            {
                DgGameStoresRef.SelectedItem = currentRefStore;
                DgGameStoresRef.ScrollIntoView(currentRefStore);
            }

            UpdateResultNameDisplay();
            UpdateStoreNameDisplay();

            this.Loaded += EditItemDialog_Loaded;
        }

        private void EditItemDialog_Loaded(object sender, RoutedEventArgs e)
        {
            DgGameItemsRef.ItemsSource = _allGameItems;
            DgGameStoresRef.ItemsSource = _allGameStores;

            var currentRefItem = _allGameItems.FirstOrDefault(x => x.ID == _currentSelectedItemId);
            if (currentRefItem != null)
            {
                DgGameItemsRef.SelectedItem = currentRefItem;
                DgGameItemsRef.ScrollIntoView(currentRefItem);
            }

            var currentRefStore = _allGameStores.FirstOrDefault(x => x.StoreId == _currentSelectedStoreId);
            if (currentRefStore != null)
            {
                DgGameStoresRef.SelectedItem = currentRefStore;
                DgGameStoresRef.ScrollIntoView(currentRefStore);
            }
        }

        private void TxtSearchItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = TxtSearchItem.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(keyword))
            {
                DgGameItemsRef.ItemsSource = _allGameItems;
            }
            else
            {
                DgGameItemsRef.ItemsSource = _allGameItems.Where(x =>
                    x.ID.ToString().Contains(keyword) ||
                    (x.TargetLangName != null && x.TargetLangName.ToLower().Contains(keyword)) ||
                    (x.EnglishName != null && x.EnglishName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword))
                ).ToList();
            }
        }

        private void TxtSearchStore_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = TxtSearchStore.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(keyword))
            {
                DgGameStoresRef.ItemsSource = _allGameStores;
            }
            else
            {
                DgGameStoresRef.ItemsSource = _allGameStores.Where(x =>
                    x.StoreId.ToString().Contains(keyword) ||
                    (x.TargetLangName != null && x.TargetLangName.ToLower().Contains(keyword)) ||
                    (x.EnglishName != null && x.EnglishName.ToLower().Contains(keyword))
                ).ToList();
            }
        }

        private void DgGameItemsRef_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgGameItemsRef.SelectedItem is GameItem selectedRefItem)
            {
                _currentSelectedItemId = selectedRefItem.ID;
                UpdateResultNameDisplay();
            }
        }

        private void DgGameStoresRef_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgGameStoresRef.SelectedItem is GameStore selectedRefStore)
            {
                _currentSelectedStoreId = selectedRefStore.StoreId;
                UpdateStoreNameDisplay();
            }
        }

        private void UpdateResultNameDisplay()
        {
            if (_itemDict.TryGetValue(_currentSelectedItemId, out var gameItem))
            {
                TxtSelectedResultName.Text = $"{gameItem.TargetLangName} ({gameItem.EnglishName}) [ID: {gameItem.ID}]";
            }
            else
            {
                TxtSelectedResultName.Text = $"Unknown Item ({_currentSelectedItemId})";
            }
        }

        private void UpdateStoreNameDisplay()
        {
            if (_storeDict.TryGetValue(_currentSelectedStoreId, out var gameStore))
            {
                TxtSelectedStoreName.Text = $"{gameStore.TargetLangName} ({gameStore.EnglishName}) [Open: {gameStore.OpenTime}]";
            }
            else
            {
                TxtSelectedStoreName.Text = $"Unknown Store ({_currentSelectedStoreId})";
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtCount.Text, out int count) ||
                !int.TryParse(TxtCurrency.Text, out int currency) ||
                !int.TryParse(TxtRequireMission.Text, out int reqMission) ||
                !double.TryParse(TxtChance.Text, out double chance))
            {
                MessageBox.Show("Please enter valid numeric values for Count, Currency, Mission, and Chance.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _itemDict.TryGetValue(_currentSelectedItemId, out var gameItem);
            _storeDict.TryGetValue(_currentSelectedStoreId, out var gameStore);

            ResultItem = new ShopItemDisplayModel
            {
                ID = _currentSelectedItemId,
                storeId = _currentSelectedStoreId,
                count = count,
                currency = currency,
                requireMission = reqMission,
                chance = chance,

                EnglishName = gameItem?.EnglishName ?? "Unknown",
                TargetLangName = gameItem?.TargetLangName ?? "Unknown",
                StoreName = gameStore?.TargetLangName ?? $"Unknown Store ({_currentSelectedStoreId})",
                OpenTime = gameStore?.OpenTime ?? "-"
            };

            this.DialogResult = true;
        }
    }
}
