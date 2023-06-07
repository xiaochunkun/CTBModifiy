using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CurseTheBeast;


public readonly struct Version : IComparable<Version>, IEqualityComparer<Version>, IEquatable<Version>
{
    public readonly string VersionStr = "";

    readonly IReadOnlyList<int> _versions = Array.Empty<int>();

    public Version(string version)
    {
        VersionStr = version;
        _versions = version.Split('.').Select(int.Parse).ToArray();
    }

    public static implicit operator Version(string version) => new Version(version);
    public static implicit operator string(Version version) => version.VersionStr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare(in Version L, in Version R)
    {
        if (L.VersionStr == R.VersionStr)
            return 0;

        for (var i = 0; ; ++i)
        {
            if (L._versions.Count <= i)
                return -1;
            if (R._versions.Count <= i)
                return 1;

            var l = L._versions[i];
            var r = R._versions[i];
            if (l < r)
                return -1;
            else if (l > r)
                return 1;
        }
    }

    public static bool operator >(in Version L, in Version R)
    {
        return Compare(L, R) > 0;
    }
    public static bool operator <(in Version L, in Version R)
    {
        return Compare(L, R) < 0;
    }
    public static bool operator <=(in Version L, in Version R)
    {
        return Compare(L, R) <= 0;
    }

    public static bool operator >=(in Version L, in Version R)
    {
        return Compare(L, R) >= 0;
    }

    public static bool operator ==(in Version L, in Version R)
    {
        return L.VersionStr == R.VersionStr;
    }

    public static bool operator !=(in Version L, in Version R)
    {
        return L.VersionStr != R.VersionStr;
    }

    public int CompareTo(Version other)
    {
        return Compare(this, other);
    }


    public bool Equals(Version x, Version y)
    {
        return x.VersionStr == y.VersionStr;
    }

    public bool Equals(Version other)
    {
        return VersionStr == other.VersionStr;
    }

    public int GetHashCode([DisallowNull] Version obj)
    {
        return obj.VersionStr.GetHashCode();
    }

    public override int GetHashCode()
    {
        return VersionStr.GetHashCode();
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj == null)
            return false;
        return ((Version)obj).VersionStr == VersionStr;
    }

    public override readonly string ToString()
    {
        return VersionStr;
    }
}
