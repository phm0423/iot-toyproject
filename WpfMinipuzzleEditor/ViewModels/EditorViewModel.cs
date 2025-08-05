using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using WpfMinipuzzleEditor.Models;
using WpfMinipuzzleEditor.Helpers;
using WpfMinipuzzleEditor.Views;
using Microsoft.Win32;

namespace WpfMinipuzzleEditor.ViewModels
{
    public class EditorViewModel : ViewModelBase
    {
        public ObservableCollection<Tile> TileCollection { get; set; }

        private TileType _selectedTileType = TileType.Wall;
        public TileType SelectedTileType
        {
            get => _selectedTileType;
            set
            {
                _selectedTileType = value;
                OnPropertyChanged();
            }
        }

        public ICommand TileClickCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand PlayCommand { get; }

        private const int GridSize = 10;

        public EditorViewModel()
        {
            TileCollection = new ObservableCollection<Tile>();
            for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++)
                    TileCollection.Add(new Tile(x, y, TileType.Empty));

            TileClickCommand = new RelayCommand(param =>
            {
                if (param is Tile tile)
                    tile.Type = SelectedTileType;
            });

            ResetCommand = new RelayCommand(_ => ResetTiles());
            SaveCommand = new RelayCommand(_ => SaveToFile());
            LoadCommand = new RelayCommand(_ => LoadFromFile());
            PlayCommand = new RelayCommand(_ => ExecutePlay());
        }

        private void ExecutePlay()
        {
            var grid = new Tile[GridSize, GridSize];
            foreach (var tile in TileCollection)
            {
                grid[tile.X, tile.Y] = new Tile(tile.X, tile.Y, tile.Type);
            }

            var viewModel = new GameViewModel(grid);
            var gameWindow = new GameWindow
            {
                DataContext = viewModel
            };
            gameWindow.Show();
        }

        private void LoadFromFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON 파일 (*.json)|*.json"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(dialog.FileName);
                    var map = JsonSerializer.Deserialize<Map>(json);
                    if (map == null) return;

                    TileCollection.Clear();
                    for (int y = 0; y < map.Height; y++)
                        for (int x = 0; x < map.Width; x++)
                            TileCollection.Add(new Tile(x, y, map.Tiles[x, y].Type));
                }
                catch
                {
                    MessageBox.Show("파일을 읽는 중 오류가 발생했습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveToFile()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON 파일 (*.json)|*.json",
                FileName = "map.json"
            };
            if (dialog.ShowDialog() == true)
            {


                try
                {
                    var map = new Map(GridSize, GridSize);
                    foreach (var tile in TileCollection)
                        map.Tiles[tile.X, tile.Y] = new Tile(tile.X, tile.Y, tile.Type);

                    string json = JsonSerializer.Serialize(map);
                    File.WriteAllText(dialog.FileName, json);
                }
                catch
                {
                    MessageBox.Show("파일 저장 중 오류가 발생했습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResetTiles()
        {
            foreach (var tile in TileCollection)
                tile.Type = TileType.Empty;
        }
    }
}
