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
        
        public EditorWindow(string gamePath, string jsonPath)
        {
            InitializeComponent();

            _gamePath = gamePath;
            _jsonPath = jsonPath;

            TxtPassedGamePath.Text = _gamePath;
            TxtPassedJsonPath.Text = _jsonPath;
        }
    }
}
