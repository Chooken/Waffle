namespace WaffleEngine;

public struct Modifier
{
    internal readonly HashSet<Keycode> ModifierKeys;

    public Modifier(params HashSet<Keycode> keys)
    {
        ModifierKeys = keys;
    }
}

public class ModifierComparer : IEqualityComparer<Modifier>, IAlternateEqualityComparer<HashSet<Keycode>, Modifier>
{
    private IEqualityComparer<HashSet<Keycode>> _comparer = HashSet<Keycode>.CreateSetComparer();
    
    public bool Equals(Modifier x, Modifier y)
    {
        return _comparer.GetHashCode(x.ModifierKeys) == _comparer.GetHashCode(y.ModifierKeys);
    }

    public int GetHashCode(Modifier obj)
    {
        return _comparer.GetHashCode(obj.ModifierKeys);
    }

    public bool Equals(HashSet<Keycode> alternate, Modifier other)
    {
        return _comparer.GetHashCode(alternate) == _comparer.GetHashCode(other.ModifierKeys);
    }

    public int GetHashCode(HashSet<Keycode> alternate)
    {
        return _comparer.GetHashCode(alternate);
    }

    public Modifier Create(HashSet<Keycode> alternate)
    {
        return new Modifier(alternate);
    }
}
