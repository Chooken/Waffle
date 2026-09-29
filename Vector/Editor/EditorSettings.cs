using VYaml.Emitter;
using VYaml.Parser;
using Vector.Editor.UI;
using WaffleEngine;
using WaffleEngine.Serializer;

namespace Vector.Editor;

// Persisted editor preferences (not undoable). Lives next to the asset
// files; applied to Theme/live state on load and on every change.
public sealed class EditorSettings : ISerializable, IDeserializable<EditorSettings>
{
    public const string SettingsPath = "Assets/editor-settings.yaml";
    public const int CurrentVersion = 1;

    public bool DarkTheme = true;
    public bool SnapEnabled = true;
    public bool ShowControls = true;
    // 0 = small, 1 = normal, 2 = large.
    public int HandleSize;
    // Canvas texels per transparency-checker square.
    public int GridSquarePixels = 4;

    public EditorSettings()
    {
        HandleSize = 1;
    }

    public void Apply()
    {
        if (DarkTheme)
            Theme.ApplyDark();
        else
            Theme.ApplyLight();
        Theme.HandleScale = HandleSize switch
        {
            0 => 0.75f,
            2 => 1.3f,
            _ => 1f,
        };
    }

    public void Reset()
    {
        DarkTheme = true;
        SnapEnabled = true;
        ShowControls = true;
        HandleSize = 1;
        GridSquarePixels = 4;
    }

    public static EditorSettings LoadOrDefault()
    {
        var settings = new EditorSettings();
        if (File.Exists(SettingsPath)
            && Yaml.TryDeserialize(SettingsPath, out EditorSettings? loaded)
            && loaded is not null)
        {
            settings = loaded;
        }
        settings.Apply();
        return settings;
    }

    public void Save() => Yaml.TrySerialize(SettingsPath, this);

    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping();
        emitter.WriteString("version");
        emitter.WriteInt32(CurrentVersion);
        emitter.WriteString("dark");
        emitter.WriteBool(DarkTheme);
        emitter.WriteString("snap");
        emitter.WriteBool(SnapEnabled);
        emitter.WriteString("controls");
        emitter.WriteBool(ShowControls);
        emitter.WriteString("handle");
        emitter.WriteInt32(HandleSize);
        emitter.WriteString("grid");
        emitter.WriteInt32(GridSquarePixels);
        emitter.EndMapping();
    }

    public static bool TryDeserialize(ref YamlParser parser, out EditorSettings settings)
    {
        settings = new EditorSettings();
        try
        {
            parser.SkipAfter(ParseEventType.DocumentStart);
            if (parser.CurrentEventType != ParseEventType.MappingStart)
                return false;
            parser.Read();
            while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
            {
                if (parser.CurrentEventType != ParseEventType.Scalar)
                {
                    parser.SkipCurrentNode();
                    continue;
                }
                string key = parser.ReadScalarAsString() ?? string.Empty;
                switch (key)
                {
                    case "dark":
                        settings.DarkTheme = ReadBool(ref parser, true);
                        break;
                    case "snap":
                        settings.SnapEnabled = ReadBool(ref parser, true);
                        break;
                    case "controls":
                        settings.ShowControls = ReadBool(ref parser, true);
                        break;
                    case "handle":
                        settings.HandleSize = ReadInt(ref parser, 1);
                        break;
                    case "grid":
                        settings.GridSquarePixels = ReadInt(ref parser, 4);
                        break;
                    default:
                        parser.SkipCurrentNode();
                        break;
                }
            }
            settings.HandleSize = Math.Clamp(settings.HandleSize, 0, 2);
            settings.GridSquarePixels = Math.Clamp(settings.GridSquarePixels, 1, 32);
            return true;
        }
        catch
        {
            settings = new EditorSettings();
            return false;
        }
    }

    private static bool ReadBool(ref YamlParser parser, bool fallback)
    {
        if (parser.CurrentEventType != ParseEventType.Scalar)
        {
            parser.SkipCurrentNode();
            return fallback;
        }
        if (parser.TryGetScalarAsBool(out bool v))
        {
            parser.Read();
            return v;
        }
        parser.Read();
        return fallback;
    }

    private static int ReadInt(ref YamlParser parser, int fallback)
    {
        if (parser.CurrentEventType != ParseEventType.Scalar)
        {
            parser.SkipCurrentNode();
            return fallback;
        }
        if (parser.TryGetScalarAsInt32(out int v))
        {
            parser.Read();
            return v;
        }
        parser.Read();
        return fallback;
    }
}
