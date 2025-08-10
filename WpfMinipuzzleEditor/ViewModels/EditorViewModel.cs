using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using WpfMinipuzzleEditor.Models;
using WpfMinipuzzleEditor.Helpers;
using WpfMinipuzzleEditor.Views;
using Microsoft.Win32;

namespace WpfMinipuzzleEditor.ViewModels
{
    public class EditorViewModel : ViewModelBase
    {
        public ObservableCollection<Tile> TileCollection { get; set; }

        private TileType _selectedTileType = TileType.Wall;
        private readonly Stack<(int x, int y, TileType before, TileType after)> _undo = new();
        private readonly Stack<(int x, int y, TileType before, TileType after)> _redo = new();

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
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        private const int GridSize = 10;

        public EditorViewModel()
        {
            TileCollection = new ObservableCollection<Tile>();
            for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++)
                    TileCollection.Add(new Tile(x, y, TileType.Empty));

            TileClickCommand = new RelayCommand(param =>
            {
                if (param is Tile tile)
                {
                    var before = tile.Type;
                    var after = SelectedTileType;
                    if (before == after) return;

                    tile.Type = after;
                    _undo.Push((tile.X, tile.Y, before, after));
                    _redo.Clear(); // 새 작업 후 리도 스택 초기화
                }
            });

            ResetCommand = new RelayCommand(_ => ResetTiles());
            SaveCommand = new RelayCommand(_ => SaveToFile());
            LoadCommand = new RelayCommand(_ => LoadFromFile());
            PlayCommand = new RelayCommand(_ => ExecutePlay());
            UndoCommand = new RelayCommand(_ => Undo(), _ => _undo.Count > 0);
            UndoCommand = new RelayCommand(_ => Redo(), _ => _undo.Count > 0);
        }

        private void Redo()
        {
            if (_redo.Count == 0) return;
            var (x, y, before, after) = _redo.Pop();

            var tile = TileCollection.First(t => t.X == x && t.Y == y);
            tile.Type = after;
            _undo.Push((x, y, before, after));
            OnPropertyChanged(nameof(TileCollection));
        }

        private void Undo()
        {
            if (_undo.Count == 0) return;
            var (x, y, before, after) = _undo.Pop();

            var tile = TileCollection.First(t => t.X == x && t.Y == y);
            tile.Type = before;
            _redo.Push((x, y, before, after));
            OnPropertyChanged(nameof(TileCollection));
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
            if (goalCount < 1) return (false, "목표(Goal) 타일이 최소 1개 이상 필요합니다.");

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
