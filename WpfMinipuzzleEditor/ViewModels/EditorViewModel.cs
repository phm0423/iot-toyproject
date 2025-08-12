using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using WpfMinipuzzleEditor.Models;
using WpfMinipuzzleEditor.Helpers;
using WpfMinipuzzleEditor.Views;
using Microsoft.Win32;
using System.Linq;
using System.Collections.Generic;

namespace WpfMinipuzzleEditor.ViewModels
{
    public class EditorViewModel : ViewModelBase
    {
        public ObservableCollection<Tile> TileCollection { get; set; }

        private TileType _selectedTileType = TileType.Wall;

        private RelayCommand _undoCommand;
        private RelayCommand _redoCommand;

        // 드래그
        private bool _isDragging;
        public bool IsDragging
        {
            get => _isDragging;
            private set
            {
                if (_isDragging == value) return;
                _isDragging = value;
                OnPropertyChanged();
            }
        }

        // Record for undo/redo 
        private record Change(int x, int y, TileType before, TileType after);

        private class StrokeAction
        {
            public List<Change> Changes { get; } = new();
        }

        private readonly Stack<object> _undo = new();
        private readonly Stack<object> _redo = new();

        private StrokeAction? _currentStroke;

        public void BeginDrag()
        {
            IsDragging = true;
            _currentStroke = new StrokeAction();
        }
        public void EndDrag()
        {
            IsDragging = false;
            if (_currentStroke != null && _currentStroke.Changes.Count > 0)
            {
                _undo.Push(_currentStroke);
                _currentStroke = null;
                _redo.Clear();
                _undoCommand?.RaiseCanExecuteChanged();
                _redoCommand?.RaiseCanExecuteChanged();
            }
        }

        public  void Paint(Tile tile)
        {
            if (tile == null) return;

            var after = SelectedTileType;
            if (tile.Type == after) return;

            if (after == TileType.Player || after == TileType.Goal)
            {
                var prev = TileCollection.FirstOrDefault(t => t.Type == after);
                if (prev != null && prev != tile)
                {
                    var beforePrev = prev.Type;     // Player or Goal
                    prev.Type = TileType.Empty;

                    if (IsDragging && _currentStroke != null)
                    {
                        var exist = _currentStroke.Changes.FindIndex(c => c.x == prev.X && c.y == prev.Y);
                        if (exist < 0)
                            _currentStroke.Changes.Add(new Change(prev.X, prev.Y, beforePrev, TileType.Empty));
                        else
                            _currentStroke.Changes[exist] = _currentStroke.Changes[exist] with { after = TileType.Empty };
                    }
                    else
                    {
                        _undo.Push(new Change(prev.X, prev.Y, beforePrev, TileType.Empty));
                        _redo.Clear();
                    }
                }
            }

            var before = tile.Type;
            tile.Type = after;

            if (IsDragging && _currentStroke != null)
            {
                var idx = _currentStroke.Changes.FindIndex(c => c.x == tile.X && c.y == tile.Y);
                if (idx < 0)
                    _currentStroke.Changes.Add(new Change(tile.X, tile.Y, before, after));
                else
                    _currentStroke.Changes[idx] = _currentStroke.Changes[idx] with { after = after };
            }
            else
            {
                _undo.Push(new Change(tile.X, tile.Y, before, after));
                _redo.Clear(); // 새 작업 후 리도 스택 초기화
                _undoCommand?.RaiseCanExecuteChanged();
                _redoCommand?.RaiseCanExecuteChanged();
            }
        }

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
        public ICommand UndoCommand => _undoCommand;
        public ICommand RedoCommand => _redoCommand;
        public ICommand NewMapCommand { get; }

        //private const int GridSize = 10;  // 초기 맵 크기 고정
        private int _gridSize = 10;
        public int GridSize
        {
            get => _gridSize;
            set
            {                
                if (_gridSize == value) return;
                _gridSize = value;
                OnPropertyChanged();
            }
        }

        public EditorViewModel()
        {
            BuildNewMap();  // 초기 맵 생성            

            TileClickCommand = new RelayCommand(param =>
            {
                if (param is Tile tile)
                {
                    //var before = tile.Type;
                    //var after = SelectedTileType;
                    //if (before == after) return;

                    //tile.Type = after;

                    //_undo.Push((tile.X, tile.Y, before, after));
                    //_redo.Clear(); // 새 작업 후 리도 스택 초기화

                    //_undoCommand?.RaiseCanExecuteChanged();
                    //_redoCommand?.RaiseCanExecuteChanged();
                    Paint(tile);
                }
            });

            ResetCommand = new RelayCommand(_ => ResetTiles());
            SaveCommand = new RelayCommand(_ => SaveToFile());
            LoadCommand = new RelayCommand(_ => LoadFromFile());
            PlayCommand = new RelayCommand(_ => ExecutePlay());

            _undoCommand = new RelayCommand(_ => Undo(), _ => _undo.Count > 0);
            _redoCommand = new RelayCommand(_ => Redo(), _ => _redo.Count > 0);

            NewMapCommand = new RelayCommand(_ => CreateNewMap(GridSize));
        }

        private void CreateNewMap(int size)
        {
            size = Math.Clamp(size, 3, 50);
            TileCollection.Clear();
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    TileCollection.Add(new Tile(x, y, TileType.Empty));

            _undo.Clear();
            _redo.Clear();
            _undoCommand?.RaiseCanExecuteChanged();
            _redoCommand?.RaiseCanExecuteChanged();

            if (GridSize != size)
            {
                _gridSize = size;
                OnPropertyChanged(nameof(GridSize));
            }
        }

        private void BuildNewMap()
        {
            TileCollection = new ObservableCollection<Tile>();
            for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++)
                    TileCollection.Add(new Tile(x, y, TileType.Empty));
            OnPropertyChanged(nameof(TileCollection));
        }

        private void Redo()
        {
            if (_redo.Count == 0) return;
            var action = _redo.Pop();

            switch (action)
            {
                case Change c:
                    ApplyChange(c, reverse: false);
                    _undo.Push(c);
                    break;
                case StrokeAction s:
                    foreach (var c in s.Changes)
                        ApplyChange(c, reverse: false);
                    _undo.Push(s);
                    break;
            }
            _undoCommand?.RaiseCanExecuteChanged();
            _redoCommand?.RaiseCanExecuteChanged();
        }

        private void ApplyChange(Change c, bool reverse)
        {
            var tile = TileCollection.First(t=> t.X == c.x && t.Y == c.y);
            tile.Type = reverse ? c.before : c.after;
        }

        private void Undo()
        {
            if (_undo.Count == 0) return;
            var action = _undo.Pop();

            switch (action)
            {
                case Change c:
                    ApplyChange(c, reverse: true);
                    _redo.Push(c);
                    break;
                case StrokeAction s:
                    for (int i = s.Changes.Count - 1; i >= 0; i--)
                        ApplyChange(s.Changes[i], reverse: true);
                    _redo.Push(s);
                    break;
            }

            _undoCommand?.RaiseCanExecuteChanged();
            _redoCommand?.RaiseCanExecuteChanged();
        }

        private void ExecutePlay()
        {
            var (ok, error) = ValidateMap(checkPath: true);
            if (!ok)
            {
                MessageBox.Show(error, "게임 실행 불가", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var grid = new Tile[GridSize, GridSize];
            foreach (var tile in TileCollection)
            {
                grid[tile.X, tile.Y] = new Tile(tile.X, tile.Y, tile.Type);
            }

            var viewModel = new GameViewModel(grid);
            var gameWindow = new GameWindow { DataContext = viewModel };

            gameWindow.SetInitialSnapshot(grid);

            gameWindow.Show();
        }

        private void LoadFromFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON 파일 (*.json)|*.json"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var json = File.ReadAllText(dialog.FileName);
                    var dto = JsonSerializer.Deserialize<MapDto>(json);
                    if (dto == null || dto.Types == null || dto.Types.Length == 0)
                        throw new InvalidDataException("맵 데이터가 비어있습니다.");

                    if (dto.Height != dto.Types.Length || dto.Width != dto.Types[0].Length)
                        throw new InvalidDataException("맵 크기 정보가 올바르지 않습니다.");

                    GridSize = dto.Width; // 저장된 맵크기 설정

                    TileCollection.Clear();
                    for (int y = 0; y < dto.Height; y++)
                    {
                        for (int x = 0; x < dto.Width; x++)
                        {
                            var type = dto.Types[y][x];
                            TileCollection.Add(new Tile(x, y, type));
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("파일을 읽는 중 오류가 발생했습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveToFile()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON 파일 (*.json)|*.json",
                FileName = "map.json"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var (ok, error) = ValidateMap(checkPath: true);
                    if (!ok)
                    {
                        MessageBox.Show(error, "저장 불가", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var dto = new MapDto
                    {
                        Width = GridSize,
                        Height = GridSize,
                        Types = new TileType[GridSize][]
                    };

                    for(int y = 0; y < GridSize; y++)
                    dto.Types[y] = new TileType[GridSize];

                    foreach (var tile in TileCollection)
                    {
                        dto.Types[tile.Y][tile.X] = tile.Type;
                    }

                    var options = new JsonSerializerOptions { WriteIndented = true };

                    string json = JsonSerializer.Serialize(dto, options);
                    File.WriteAllText(dialog.FileName, json);
                    MessageBox.Show("저장 완료!", "정보", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 저장 중 오류가 발생했습니다.\n\n{ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResetTiles()
        {
            foreach (var tile in TileCollection)
                tile.Type = TileType.Empty;
        }


        // 맵검증(Player, Goal 검증)

        private (bool ok, string error) ValidateMap(bool checkPath = true)
        {
            int playerCount = 0;
            int goalCount = 0;

            foreach (var t in TileCollection)
            {
                if (t.Type == TileType.Player) playerCount++;
                if (t.Type == TileType.Goal) goalCount++;
            }

            if (playerCount != 1) return (false, $"플레이어 개수는 정확히 1개여야 합니다. (현재: {playerCount})");
            if (goalCount != 1) return (false, "목표(Goal) 타일이 정확히 1개여야 합니다.");

            if (!checkPath) return (true, "");

            // 경로 존재 검사 (BFS)
            return (HasPathFromPlayerToAnyGoal() ? (true, "") : (false, "플레이어에서 목표까지 갈 수 있는 경로가 없습니다."));
        }

        private bool HasPathFromPlayerToAnyGoal()
        {
            int n = (int)Math.Sqrt(TileCollection.Count);
            if (n * n != TileCollection.Count) n = 10; // 기본 크기

            var grid = new Tile[n, n];
            foreach (var t in TileCollection)
            {
                if (t.X >= 0 && t.X < n && t.Y >= 0 && t.Y < n)
                    grid[t.X, t.Y] = t;
            }

            // player/goal 위치 찾기
            (int x, int y)? start = null;
            var goals = new HashSet<(int x, int y)>();
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    var tile = grid[x, y];
                    if (tile == null) continue;
                    if (tile.Type == TileType.Player) start = (x, y);
                    if (tile.Type == TileType.Goal) goals.Add((x, y));
                }
            }
            if (start == null || goals.Count == 0) return false;

            // BFS로 경로 탐색
            var q = new Queue<(int x, int y)>();
            var visited = new bool[n, n];
            q.Enqueue(start.Value);
            visited[start.Value.x, start.Value.y] = true;

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            while (q.Count > 0)
            {
                var (cx, cy) = q.Dequeue();
                if (goals.Contains((cx, cy))) return true;
                
                for (int dir = 0; dir < 4; dir++)
                {
                    int nx = cx + dx[dir], ny = cy + dy[dir];
                    if (nx < 0 || nx >= n || ny < 0 || ny >= n) continue;
                    if (visited[nx, ny]) continue;

                    var nt = grid[nx, ny];
                    if (nt == null) continue;
                    if (nt.Type == TileType.Wall) continue;

                    visited[nx, ny] = true;
                    q.Enqueue((nx, ny));
                }
            }

            return false;
        }
    }
}
