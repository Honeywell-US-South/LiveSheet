using System.Globalization;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LiveSheet;

public struct LiveSheetTime
{
    private bool _isNull = false;
    private DateTime? _dateTime;

    public LiveSheetTime()
    {
    }

    public LiveSheetTime(DateTime dateTime)
    {
        _dateTime = dateTime;
    }

    public LiveSheetTime(string liveSheetTimeAsString)
    {
        _dateTime = null;
        if (!string.IsNullOrEmpty(liveSheetTimeAsString))
        {
            var parts = liveSheetTimeAsString.Split("|");
            if (parts.Length == 1)
            {
                if (parts[0].Equals(typeof(LiveSheetTime).Name, StringComparison.OrdinalIgnoreCase))
                {
                    _dateTime = null;
                }
                else
                {
                    if (DateTime.TryParse(parts[0], out var dt))
                        _dateTime = dt;
                    else _dateTime = null;
                }
            }
            else if (parts.Length == 2)
            {
                if (parts[0].Equals(typeof(LiveSheetTime).Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (DateTime.TryParse(parts[1], null, DateTimeStyles.RoundtripKind, out var dt))
                        _dateTime = dt;
                }
                else _dateTime = null;
            }
        }
    }

    public bool IsNullBson
    {
        get => _isNull && _dateTime == null;
        private set => _isNull = value;
    }

    public static bool IsLiveSheetTime(string liveSheetTimeAsString)
    {
        if (!string.IsNullOrEmpty(liveSheetTimeAsString))
        {
            var parts = liveSheetTimeAsString.Split("|");
            if (parts.Length == 2)
            {
                if (parts[0].Equals(typeof(LiveSheetTime).Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static LiveSheetTime Now => new();

    public string AsString => $"{typeof(LiveSheetTime).Name}|{_dateTime?.ToString("O") ?? "null"}";

    // Override ToString() for simple string conversion
    public override string ToString()
    {
        return _dateTime.ToString() ?? DateTime.Now.ToString();
    }

    // Implement IFormattable ToString method for custom formatting
    public string ToString(string format, IFormatProvider? formatProvider = null)
    {
        if (string.IsNullOrEmpty(format)) format = "G"; // Default format
        if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;

        return _dateTime?.ToString(format, formatProvider) ?? DateTime.Now.ToString(format, formatProvider);
    }

    public long AsMilliseconds => _dateTime?.Ticks ?? DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    public long Difference(LiveSheetTime? timeB)
    {
        if (timeB == null || (timeB?.IsNullBson ?? false)) return AsMilliseconds;
        if (_dateTime == null && timeB?._dateTime == null) return 0;
        return AsMilliseconds - timeB?.AsMilliseconds ?? 0;
    }


    // conversion to BsonValue
    public static implicit operator BsonValue(LiveSheetTime st)
    {
        return new BsonValue($"{typeof(LiveSheetTime).Name}|{st._dateTime?.ToString("O") ?? "null"}");
    }

    // conversion from BsonValue to LiveSheetTime
    public static implicit operator LiveSheetTime(BsonValue dt)
    {
        var st = new LiveSheetTime(dt?.AsString ?? "");
        st.IsNullBson = dt == null;
        return st;
    }

    // conversion to DateTime
    public static implicit operator DateTime(LiveSheetTime st)
    {
        return st._dateTime ?? DateTime.Now;
    }

    // conversion from DateTime to LiveSheetTime
    public static implicit operator LiveSheetTime(DateTime dt)
    {
        return new LiveSheetTime(dt);
    }

        
}
