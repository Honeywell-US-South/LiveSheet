namespace LiveSheet.Parts.Ports;

[Flags]
public enum PortType
{
    None = 0,
    Numeric = 1 << 0, // 1
    Logic = 1 << 1, // 2
    String = 1 << 2, // 4
    Time = 1 << 3, //8
    // Open For Other Types Not Multi

    Multi = 1 << 8 // 256, Wild Card
}