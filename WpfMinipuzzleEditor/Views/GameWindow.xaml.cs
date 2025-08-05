using System.Windows;
using System.Windows.Input;

namespace WpfMinipuzzleEditor.Views
{
    /// <summary>
    /// GameWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GameWindow : Window
    {
        public GameWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => Keyboard.Focus(this);
            KeyDown += GameWindow_KeyDown;
        }

        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is ViewModels.GameViewModel vm)
            {
                switch (e.Key)
                {
                    case Key.Up:
                        vm.MovePlayer(0, -1);
                        break;
                    case Key.Down:
                        vm.MovePlayer(0, 1);
                        break;
                    case Key.Left:
                        vm.MovePlayer(-1, 0);
                        break;
                    case Key.Right:
                        vm.MovePlayer(1, 0);
                        break;
                }
            }
        }
    }
}
