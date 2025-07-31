using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfMinipuzzleEditor.Models;
using WpfMinipuzzleEditor.Helpers;

namespace WpfMinipuzzleEditor.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        // 2차원 배열 (로직용)
        public Tile[,] Tiles { get; set; }
        public int Width => Tiles.GetLength(0);
        public int Height => Tiles.GetLength(1);

        // View 바인딩용 1차원 컬렉션
        public ObservableCollection<Tile> GameTiles { get; set; }

        private Tile _playerTile;

        // 방향 이동 커맨드
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand MoveLeftCommand { get; }
        public ICommand MoveRightCommand { get; }

        public GameViewModel(Tile[,] tiles)
        {
            Tiles = tiles;
            _playerTile = FindPlayerTile();

            // GameTiles 초기화
            GameTiles = new ObservableCollection<Tile>();
            foreach (var tile in tiles)
                GameTiles.Add(tile);

            // 커맨드 초기화
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

        public void MovePlayer(int dx, int dy)
        {
            if (_playerTile == null) return;

            int newX = _playerTile.X + dx;
            int newY = _playerTile.Y + dy;

            if (newX < 0 || newX >= Width || newY < 0 || newY >= Height)
                return;

            var targetTile = Tiles[newX, newY];

            if (targetTile.Type == TileType.Wall)
                return;

            // 이동 처리
            _playerTile.Type = TileType.Empty;
            targetTile.Type = TileType.Player;
            _playerTile = targetTile;

            OnPropertyChanged(nameof(GameTiles));

            if (targetTile.Type == TileType.Goal)
            {
                MessageBox.Show("클리어!", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
