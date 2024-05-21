using Blazor.Diagrams;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;
using Blazor.Diagrams.Options;
using LiveSheet.Parts.Nodes;
using LiveSheet.Parts.Serialization;

namespace LiveSheet;

public class LiveSheetDiagram : BlazorDiagram
{
    private readonly LiveSheetLogic _logic;


    private LiveSheetState _state = LiveSheetState.Unloaded;

    public LiveSheetDiagram() : base(DefaultOptions)
    {
        _logic = new LiveSheetLogic(this);
    }

    public LiveSheetDiagram(BlazorDiagramOptions? options = null) : base(options ?? DefaultOptions)
    {
        _logic = new LiveSheetLogic(this);
    }

    public Action<LiveSheetState>? LiveSheetStateChange { get; set; }
    public Action<LiveSheetDiagram>? LiveSheetUpdated { get; set; }

    [LiveSerialize] public string Guid { get; set; } = System.Guid.NewGuid().ToString();

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
            EnableSnapping = false,
            RequireTarget = true
        }
    };


    [LiveSerialize] public string Name { get; set; } = string.Empty;

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

    public List<LiveNode> GetLiveNodes()
    {
        return Nodes.Cast<LiveNode>().ToList();
    }

    public bool IsReadOnly { get; private set; } = false;

    public void ReadOnlyEnable(bool readOnly)
    {
        if (IsReadOnly == readOnly) return;
        if (readOnly)
        {
            this.Nodes.ToList().ForEach(x => x.Locked = true);
        }
        else
        {
            this.Nodes.ToList().ForEach(x => x.Locked = true);
        }
    }


    public virtual void Unload()
    {
        if (State == LiveSheetState.Loaded)
        {
            this.Clear();
            if (_logic.Enabled) _logic.DisableLogic();

            State = LiveSheetState.Unloaded;
        }
    }

    public void UpdateSaveData()
    {
        Version = Version + 1;
        Data = this.SaveLiveSheetData();
        LiveSheetUpdated?.Invoke(this);
    }

    public virtual void Load()
    {
        if (State == LiveSheetState.Unloaded)
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