using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfMinipuzzleEditor.Helpers;
using WpfMinipuzzleEditor.Models;

namespace WpfMinipuzzleEditor.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        public Tile[,] Tiles { get; set; }

        private Tile _playerTile;

        public int Width => Tiles.GetLength(0);
        public int Height => Tiles.GetLength(1);

        public ObservableCollection<Tile> GameTiles { get; set; }

        private Tile? _selectedTile;

        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand MoveLeftCommand { get; }
        public ICommand MoveRightCommand { get; }

        public GameViewModel(Tile[,] tiles)
        {
            Tiles = tiles;
            _playerTile = FindPlayerTile();

            GameTiles = new ObservableCollection<Tile>();
            for (int y = 0; y < Height; y++)
                for(int x = 0; x < Width; x++)
                    GameTiles.Add(Tiles[x, y]);

            MoveUpCommand = new RelayCommand(_ => MovePlayer(0, -1));
            MoveDownCommand = new RelayCommand(_ => MovePlayer(0, 1));
            MoveLeftCommand = new RelayCommand(_ => MovePlayer(-1, 0));
            MoveRightCommand = new RelayCommand(_ => MovePlayer(1, 0));
        }

        private Tile? FindPlayerTile()
        {
            foreach (var tile in Tiles)
            {
                if (tile.Type == TileType.Player)
                    return tile;
            }
            return null;
        }

        public bool MovePlayer(int dx, int dy)
        {
            if (_playerTile == null) return false;

            int newX = _playerTile.X + dx;
            int newY = _playerTile.Y + dy;

            if (newX < 0 || newX >= Width || newY < 0 || newY >= Height)
                return false;

            var targetTile = Tiles[newX, newY];
            if (targetTile == null || targetTile.Type == TileType.Wall)
                return false;

            if (targetTile.Type == TileType.Goal)
                return true;    // goal 도달
            
            _playerTile.Type = TileType.Empty;
            targetTile.Type = TileType.Player;
            _playerTile = targetTile;

            OnPropertyChanged(nameof(GameTiles));
            return false; // 게임 진행 중

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
