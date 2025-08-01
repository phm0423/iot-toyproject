﻿using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using WpfMinipuzzleEditor.Models;
using WpfMinipuzzleEditor.Helpers;
using WpfMinipuzzleEditor.Views;

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
            PlayCommand = new RelayCommand(_ => ExecutePlay()); // 다음 단계에서 구현
        }

        private void ExecutePlay()
        {
            var grid = new Tile[GridSize, GridSize];
            foreach (var tile in TileCollection)
            {
                grid[tile.X, tile.Y] = new Tile(tile.X, tile.Y, tile.Type);
            }

            var viewModel = new GameViewModel(grid);
            var gameWindow = new GameView
            {
                DataContext = viewModel
            };
            gameWindow.Show();
        }

        private void ResetTiles()
        {
            foreach (var tile in TileCollection)
                tile.Type = TileType.Empty;
        }

        private void SaveToFile()
        {
            var map = new Map(GridSize, GridSize);
            foreach (var tile in TileCollection)
                map.Tiles[tile.X, tile.Y] = tile;

            string json = JsonSerializer.Serialize(map);
            File.WriteAllText("map.json", json);
        }

        private void LoadFromFile()
        {
            if (!File.Exists("map.json")) return;

            string json = File.ReadAllText("map.json");
            var map = JsonSerializer.Deserialize<Map>(json);
            if (map == null) return;

            TileCollection.Clear();
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    TileCollection.Add(map.Tiles[x, y]);
        }
    }

}
