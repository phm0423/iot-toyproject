using System.Windows;
using System.Windows.Input;

namespace WpfMinipuzzleEditor.Views
{
    /// <summary>
    /// GameWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GameWindow : Window
    {
        // 맵 재시작을 위한 초기상태 기억
        private WpfMinipuzzleEditor.Models.Tile[,]? _initialSnapshot;

        public GameWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => Keyboard.Focus(this);
            KeyDown += GameWindow_KeyDown;
        }

        // 생성 직후, 초기 상태를 전달
        public void SetInitialSnapshot(WpfMinipuzzleEditor.Models.Tile[,] snapshot)
        {
            int w = snapshot.GetLength(0), h = snapshot.GetLength(1);
            _initialSnapshot = new WpfMinipuzzleEditor.Models.Tile[w, h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    _initialSnapshot[x, y] = new WpfMinipuzzleEditor.Models.Tile(snapshot[x, y].X, snapshot[x, y].Y, snapshot[x, y].Type);
        }

        private void GameWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            if (DataContext is ViewModels.GameViewModel vm)
            {
                bool cleared = false;

                switch (e.Key)
                {
                    case Key.Up:
                    case Key.W:
                        cleared = vm.MovePlayer(0, -1);
                        break;
                    case Key.Down:
                    case Key.S:
                        cleared = vm.MovePlayer(0, 1);
                        break;
                    case Key.Left:
                    case Key.A:
                        cleared = vm.MovePlayer(-1, 0);
                        break;
                    case Key.Right:
                    case Key.D:
                        cleared = vm.MovePlayer(1, 0);
                        break;

                    case Key.R:
                        RestartFromSnapshot();
                        return; // 수동 재시작

                    case Key.Escape:
                        this.Close();
                        return;
                }

                if (cleared)
                {
                    var result = MessageBox.Show("클리어! 다시 시작할까요?", "성공", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        RestartFromSnapshot();
                    }
                    else 
                    {
                        this.Close();
                    }
                }
            }
        }
        private void RestartFromSnapshot()
        {
            if (_initialSnapshot != null)
            {
                var w = _initialSnapshot.GetLength(0);
                var h = _initialSnapshot.GetLength(1);
                var fresh = new WpfMinipuzzleEditor.Models.Tile[w, h];
                for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                        fresh[x, y] = new WpfMinipuzzleEditor.Models.Tile(_initialSnapshot[x, y].X, _initialSnapshot[x, y].Y, _initialSnapshot[x, y].Type);
                DataContext = new ViewModels.GameViewModel(fresh);
                Keyboard.Focus(this);
            }
        }
    }
}
