using System.Collections.ObjectModel;
using System.Windows.Input;
using WpfMinipuzzleEditor.Models;

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
                OnPropertyChanged(nameof(SelectedTileType));
            }
        }

        public ICommand TileClickCommand(AttributeTargets;)

        public EditorViewModel()
        {
        }
    }
}
