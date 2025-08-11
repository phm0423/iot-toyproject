using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfMinipuzzleEditor.Models;
using WpfMinipuzzleEditor.ViewModels;

namespace WpfMinipuzzleEditor.Views
{
    /// <summary>
    /// EditorVIew.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EditorView : Window
    {
        public EditorView()
        {
            InitializeComponent();
            DataContext = new EditorViewModel();
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is EditorViewModel vm && vm.IsDragging)
            {
                vm.EndDrag();
                Mouse.Capture(null);
            }
        }

        private void Tile_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is EditorViewModel vm && sender is Button btn && btn.DataContext is Tile tile)
            {
                if (vm.SelectedTileType == TileType.Player || vm.SelectedTileType == TileType.Goal)
                {
                    vm.Paint(tile);
                    return;
                }

                vm.BeginDrag();
                vm.Paint(tile);
                Mouse.Capture(this);
            }
        }
        private void Tile_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is EditorViewModel vm && vm.IsDragging)
            {
                vm.EndDrag();
                Mouse.Capture(null);
            }
        }
        
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (DataContext is not EditorViewModel vm || !vm.IsDragging) return;

            Point p = e.GetPosition(this);
            var hit = InputHitTest(p) as DependencyObject;
            if (hit == null) return;

            var btn = FindParent<Button>(hit);
            if (btn?.DataContext is Tile tile)
            {
                vm.Paint(tile);
            }
        }

        private static T? FindParent<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T t) return t;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
