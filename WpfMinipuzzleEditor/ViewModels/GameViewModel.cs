using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfMinipuzzleEditor.Models;

namespace WpfMinipuzzleEditor.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        public Tile[,] Tiles { get; set; }
        public int Width => Tiles.GetLength(0);
        public int Height => Tiles.GetLength(1);

        private Tile _playerTile;
        
        public GameViewModel(Tile[,] tiles)
        {
            Tiles = tiles;
            _playerTile = FindPlayerTile();
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

        public void MovePlyaer(int dx, int dy)
        {
            if (_playerTile == null) return;

            int newX = _playerTile.X + dx;
            int newY = _playerTile.Y + dy;

            if (newX < 0 || newX >= Width || newY < 0 || newY >= Height)
                return; 

            var targetTile = Tiles[newX, newY];

            if (targetTile.Type == TileType.Wall)
                return;

            // 이동
            _playerTile.Type = TileType.Empty;
            targetTile.Type = TileType.Player;
            _playerTile = targetTile;

            OnPropertyChanged(nameof(Tiles));

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
