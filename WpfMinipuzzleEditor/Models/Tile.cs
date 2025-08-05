using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace WpfMinipuzzleEditor.Models
{
    public class Tile : INotifyPropertyChanged
    {
        public int X { get; }
        public int Y { get; }

        private TileType _type;
        public TileType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Color));

            }
        }

        public Brush Color =>
            Type switch
            {
                TileType.Empty => Brushes.White,
                TileType.Wall => Brushes.Black,
                TileType.Player => Brushes.Blue,
                TileType.Goal => Brushes.Green,
                _ => Brushes.Transparent
            };

        public Tile(int x, int y, TileType type)
        {
            X = x;
            Y = y;
            Type = type;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));       
    }
}
