using OpenTK.Core.Platform;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Numerics;

namespace WaffleEngine;

public static class Input
{
    private interface IButtonBind
    {
        public bool IsDown(); 

        public bool IsPressed();
    }

    private interface IVectorBind
    {
        public Vector2 GetValue();
    }

    private struct KeyboardButtonBind : IButtonBind
    {
        public int Key;

        public bool IsDown()
        {
            return Keyboard.IsDown(Key);
        }

        public bool IsPressed()
        {
            return Keyboard.IsPressed(Key);
        }
    }

    private struct KeyVectorBind : IVectorBind
    {
        public int UpKey;
        public int DownKey;
        public int LeftKey;
        public int RightKey;

        public Vector2 GetValue()
        {
            float up = Keyboard.IsDown(UpKey) ? 1f : 0f;
            float down = Keyboard.IsDown(DownKey) ? 1f : 0f;
            float left = Keyboard.IsDown(LeftKey) ? 1f : 0f;
            float right = Keyboard.IsDown(RightKey) ? 1f : 0f;

            float vertical = up - down;
            float horizontal = right - left;

            return new Vector2(horizontal, vertical);
        }
    }

    private static Dictionary<string, List<IVectorBind>> _vector_binds = new Dictionary<string, List<IVectorBind>>();

    public static Vector2 GetVector(string bind_name)
    {
        Vector2 vector = Vector2.Zero;

        foreach (var bind in _vector_binds[bind_name])
        {
            Vector2 vector2 = bind.GetValue();
            if (vector2 == vector)
                continue;

            vector = vector2;
            break;
        }

        return vector;
    }

    public static void BindKeyVectors(string bind_name, params (int up, int down, int left, int right)[] keys)
    {
        foreach (var key in keys)
        {
            if (!_vector_binds.ContainsKey(bind_name))
                _vector_binds[bind_name] = new List<IVectorBind>();

            _vector_binds[bind_name].Add(new KeyVectorBind { UpKey = key.up, DownKey = key.down, LeftKey = key.left, RightKey = key.right });
        }
    }

    public static void SetKeyVector(string bind_name, int bind_index, int up, int down, int left, int right)
    {
        if (!_vector_binds.ContainsKey(bind_name))
            _vector_binds[bind_name] = new List<IVectorBind>(bind_index);

        while (_vector_binds[bind_name].Count <= bind_index)
        {
            _vector_binds[bind_name].Add(default);
        }

        _vector_binds[bind_name][bind_index] = new KeyVectorBind { UpKey = up, DownKey = down, LeftKey = left, RightKey = right };
    }

    private static Dictionary<string, List<IButtonBind>> _button_binds = new Dictionary<string, List<IButtonBind>>();

    public static bool ButtonIsDown(string bind_name)
    {
        foreach (var bind in _button_binds[bind_name])
        {
            if (bind.IsDown())
                return true;
        }

        return false;
    }

    public static bool ButtonIsPressed(string bind_name)
    {
        foreach (var bind in _button_binds[bind_name])
        {
            if (bind.IsPressed())
                return true;
        }

        return false;
    }

    public static void BindButtonKeys(string bind_name, params int[] keys)
    {
        foreach (var key in keys)
        {
            if (!_button_binds.ContainsKey(bind_name))
                _button_binds[bind_name] = new List<IButtonBind>();

            _button_binds[bind_name].Add(
                new KeyboardButtonBind { Key = key }
            );
        }
    }

    public static void SetButtonBind(string bind_name, int bind_index, int key)
    {
        if (!_button_binds.ContainsKey(bind_name))
            _button_binds[bind_name] = new List<IButtonBind>(bind_index);

        _button_binds[bind_name][bind_index] = new KeyboardButtonBind { Key = key };
    }

    public static void ClearButtonBind(string bind_name)
    {
        _button_binds[bind_name].Clear();
    }

    public static void ClearAllButtonBinds()
    {
        _button_binds.Clear();
    }
}