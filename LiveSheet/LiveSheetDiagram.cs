using Blazor.Diagrams;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;
using Blazor.Diagrams.Options;
using LiveSheet.Parts.Nodes;
using LiveSheet.Parts.Serialization;

namespace LiveSheet;

public class LiveSheetDiagram : BlazorDiagram
{
    public Action<LiveSheetState>? LiveSheetStateChange { get; set; }
    public Action<LiveSheetDiagram>? LiveSheetUpdated { get; set; }

    [LiveSerialize] public string Guid { get; set; } = System.Guid.NewGuid().ToString();

    public LiveSheetDiagram() : base(options: DefaultOptions)
    {
        _logic = new(this);
    }

    public LiveSheetDiagram(BlazorDiagramOptions? options = null) : base(options: options ?? DefaultOptions)
    {
        _logic = new(this);
    }

    public static BlazorDiagramOptions DefaultOptions => new()
    {
        AllowMultiSelection = true,
        Zoom =
        {
            Enabled = true,
            Inverse = true
        },
        Links =
        {
            DefaultRouter = new NormalRouter(),
            DefaultPathGenerator = new SmoothPathGenerator(),
            SnappingRadius = 4,
            EnableSnapping = true
        },
    };


    [LiveSerialize] public string Name { get; set; } = string.Empty;


    private LiveSheetState _state = LiveSheetState.Unloaded;
    private LiveSheetLogic _logic;

    public LiveSheetState State
    {
        get => _state;
        private set
        {
            if (value == _state) return;
            _state = value;
            LiveSheetStateChange?.Invoke(_state);
            LiveSheetUpdated?.Invoke(this);
        }
    }

    [LiveSerialize] public string Data { get; set; } = string.Empty;
    [LiveSerialize] public int Version { get; private set; } = 1;

    public List<LiveNode> GetLiveNodes() => this.Nodes.Cast<LiveNode>().ToList();


    public void Unload()
    {
        if (this.State == LiveSheetState.Loaded)
        {
            this.Clear();
            if (_logic.Enabled)
            {
                _logic.DisableLogic();
            }

            State = LiveSheetState.Unloaded;
        }
    }

    public void UpdateSaveData()
    {
        Version = Version + 1;
        Data = this.SaveLiveSheetData();
        LiveSheetUpdated?.Invoke(this);
    }

    public void Load()
    {
        if (this.State == LiveSheetState.Unloaded)
        {
            if (!_logic.Enabled)
            {
                _logic.EnableLogic();
                this.LoadLiveSheetData(Data);
            }

            State = LiveSheetState.Loaded;
        }
    }
}