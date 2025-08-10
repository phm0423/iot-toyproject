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
                switch (e.Key)
                {
                    case Key.Up:
                    case Key.W:
                        if(vm.MovePlayer(0, -1)) this.Close();
                        break;
                    case Key.Down:
                    case Key.S:
                        if(vm.MovePlayer(0, 1)) this.Close();
                        break;
                    case Key.Left:
                    case Key.A:
                        if(vm.MovePlayer(-1, 0)) this.Close();
                        break;
                    case Key.Right:
                    case Key.D:
                        if(vm.MovePlayer(1, 0)) this.Close();
                        break;

                    // 재시작
                    case Key.R:
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
                        break;

                    case Key.Escape:
                        this.Close();
                        break;
                }
            }
        }
    }
}
