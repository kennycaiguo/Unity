using System;
using System.Text.RegularExpressions;

public struct TheVersion : IComparable<TheVersion>
{
    private const string versionRegex = @"^(?<major>\d+)(\.?(?<minor>\d+))?(\.?(?<patch>\d+))?(\.?(?<build>\d+))?(\.?(?<special>.+))?";
    private const int PART_COUNT = 4;
    private static TheVersion empty;
    public static TheVersion Default {
        get {
            if (empty.Version == null)
                empty = default(TheVersion).Initialize(null);
            return empty;
        }
    }

    private int major;
    public int Major { get { Initialize(Version); return major; } }
    private int minor;
    public int Minor { get { Initialize(Version); return minor; } }
    public bool HasMinor { get; private set; }
    private int patch;
    public int Patch { get { Initialize(Version); return patch; } }
    public bool HasPatch { get; private set; }
    private int build;
    public int Build { get { Initialize(Version); return build; } }
    public bool HasBuild { get; private set; }
    private string special;
    public string Special { get { Initialize(Version); return special; } }
    private bool isAlpha;
    public bool IsAlpha { get { Initialize(Version); return isAlpha; } }
    private bool isBeta;
    public bool IsBeta { get { Initialize(Version); return isBeta; } }
    private bool isUnstable;
    public bool IsUnstable { get { Initialize(Version); return isUnstable; } }
    public bool IsValid { get; private set; }

    private int[] intParts;
    private string[] stringParts;
    private int currentPart;
    private string version;

    public string Version { get { return version ?? (version = string.Empty); } set { version = value; } }

    private static readonly Regex regex = new Regex(versionRegex);

    public static TheVersion Parse(string version)
    {
        return default(TheVersion).Initialize(version);
    }

    public static bool TryParse(string version, out TheVersion parsed)
    {
        parsed = Parse(version);
        return parsed.IsValid;
    }

    public TheVersion BumpMajor()
    {
        return ResetFromMinor(string.Format("{0}", Major + 1));
    }

    public TheVersion BumpMinor()
    {
        return ResetFromPatch(string.Format("{0}.{1}", Major, Minor + 1));
    }

    public TheVersion BumpPatch()
    {
        return ResetFromBuild(string.Format("{0}.{1}.{2}", Major, Minor, Patch + 1));
    }

    public TheVersion BumpBuild()
    {
        return ResetFromSpecial(string.Format("{0}.{1}.{2}.{3}", Major, Minor, Patch, Build + 1));
    }

    public TheVersion BumpLastPart()
    {
        if (HasBuild) return BumpBuild();
        if (HasPatch) return BumpPatch();
        if (HasMinor) return BumpMinor();
        return BumpMajor();
    }

    public TheVersion SetMajor(int newValue)
    {
        return ResetFromMinor(string.Format("{0}", newValue));
    }

    public TheVersion SetMinor(int newValue)
    {
        return ResetFromPatch(string.Format("{0}.{1}", Major, newValue));
    }

    public TheVersion SetPatch(int newValue)
    {
        return ResetFromBuild(string.Format("{0}.{1}.{2}", Major, Minor, newValue));
    }

    public TheVersion SetBuild(int newValue)
    {
        return ResetFromSpecial(string.Format("{0}.{1}.{2}.{3}", Major, Minor, Patch, newValue));
    }

    private TheVersion ResetFromMinor(string ret)
    {
        if (HasMinor) ret += ".0";
        return ResetFromPatch(ret);
    }

    private TheVersion ResetFromPatch(string ret)
    {
        if (HasPatch) ret += ".0";
        return ResetFromBuild(ret);
    }

    private TheVersion ResetFromBuild(string ret)
    {
        if (HasBuild) ret += ".0";
        return ResetFromSpecial(ret);
    }

    private TheVersion ResetFromSpecial(string ret)
    {
        if (IsUnstable) ret += Special;
        return Parse(ret);
    }

    private TheVersion Initialize(string theVersion)
    {
        if (IsValid)
            return this;

        Version = String.Empty;
        if (theVersion != null)
            Version = theVersion.Trim();

        isAlpha = false;
        isBeta = false;
        major = 0;
        minor = 0;
        patch = 0;
        build = 0;
        special = null;
        currentPart = 0;

        intParts = new int[PART_COUNT];
        stringParts = new string[PART_COUNT];
        for (var i = 0; i < PART_COUNT; i++)
            stringParts[i] = intParts[i].ToString();

        if (string.IsNullOrEmpty(theVersion))
            return this;

        var match = regex.Match(theVersion);
        if (!match.Success)
        {
            return this;
        }

        major = int.Parse(match.Groups["major"].Value);
        intParts[currentPart] = major;
        stringParts[currentPart] = major.ToString();

        var minorMatch = match.Groups["minor"];
        var patchMatch = match.Groups["patch"];
        var buildMatch = match.Groups["build"];
        var specialMatch = match.Groups["special"];

        if (minorMatch.Success)
        {
            currentPart++;
            minor = int.Parse(minorMatch.Value);
            HasMinor = true;
            intParts[currentPart] = minor;
            stringParts[currentPart] = minor.ToString();

            if (patchMatch.Success)
            {
                currentPart++;
                patch = int.Parse(patchMatch.Value);
                HasPatch = true;
                intParts[currentPart] = patch;
                stringParts[currentPart] = patch.ToString();

                if (buildMatch.Success)
                {
                    currentPart++;
                    build = int.Parse(buildMatch.Value);
                    HasBuild = true;
                    intParts[currentPart] = build;
                    stringParts[currentPart] = build.ToString();
                }
            }
        }

        if (specialMatch.Success)
        {
            special = specialMatch.Value;
            stringParts[currentPart] = stringParts[currentPart] + special;
        }

        isUnstable = special != null;
        if (isUnstable)
        {
            isAlpha = special.IndexOf("alpha", StringComparison.Ordinal) >= 0;
            isBeta = special.IndexOf("beta", StringComparison.Ordinal) >= 0;
        }
        IsValid = true;
        return this;
    }

    public override string ToString()
    {
        return Version;
    }

    public int CompareTo(TheVersion other)
    {
        if (this > other)
            return 1;
        if (this == other)
            return 0;
        return -1;
    }

    public override int GetHashCode()
    {
        var hash = 17;
        hash = hash * 23 + Major.GetHashCode();
        hash = hash * 23 + Minor.GetHashCode();
        hash = hash * 23 + Patch.GetHashCode();
        hash = hash * 23 + Build.GetHashCode();
        hash = hash * 23 + (Special != null ? Special.GetHashCode() : 0);
        return hash;
    }

    public override bool Equals(object obj)
    {
        if (obj is TheVersion)
            return Equals((TheVersion)obj);
        return false;
    }

    public bool Equals(TheVersion other)
    {
        return this == other;
    }

    public static bool operator==(TheVersion lhs, TheVersion rhs)
    {
        if (lhs.Version == rhs.Version)
            return true;
        return
            (lhs.Major == rhs.Major) &&
                (lhs.Minor == rhs.Minor) &&
                (lhs.Patch == rhs.Patch) &&
                (lhs.Build == rhs.Build) &&
                (lhs.Special == rhs.Special);
    }

    public static bool operator!=(TheVersion lhs, TheVersion rhs)
    {
        return !(lhs == rhs);
    }

    public static bool operator>(TheVersion lhs, TheVersion rhs)
    {
        if (lhs.Version == rhs.Version)
            return false;
        if (!lhs.IsValid)
            return false;
        if (!rhs.IsValid)
            return true;

        for (var i = 0; i < lhs.currentPart && i < rhs.currentPart; i++)
        {
            if (lhs.intParts[i] != rhs.intParts[i])
                return lhs.intParts[i] > rhs.intParts[i];
        }

        for (var i = 1; i < PART_COUNT; i++)
        {
            var ret = CompareVersionStrings(lhs.stringParts[i], rhs.stringParts[i]);
            if (ret != 0)
                return ret > 0;
        }

        return false;
    }

    public static bool operator<(TheVersion lhs, TheVersion rhs)
    {
        return !(lhs > rhs);
    }

    public static bool operator>=(TheVersion lhs, TheVersion rhs)
    {
        return lhs > rhs || lhs == rhs;
    }

    public static bool operator<=(TheVersion lhs, TheVersion rhs)
    {
        return lhs < rhs || lhs == rhs;
    }

    private static int CompareVersionStrings(string lhs, string rhs)
    {
        int lhsNonDigitPos = 0;
        var lhsNumber = GetNumberFromVersionString(lhs, out lhsNonDigitPos);

        int rhsNonDigitPos = 0;
        var rhsNumber = GetNumberFromVersionString(rhs, out rhsNonDigitPos);

        if (lhsNumber != rhsNumber)
            return lhsNumber.CompareTo(rhsNumber);

        if (lhsNonDigitPos < 0 && rhsNonDigitPos < 0)
            return 0;

        // versions with alphanumeric characters are always lower than ones without
        // i.e. 1.1alpha is lower than 1.1
        if (lhsNonDigitPos < 0)
            return 1;
        if (rhsNonDigitPos < 0)
            return -1;
        return string.Compare(lhs.Substring(lhsNonDigitPos), rhs.Substring(rhsNonDigitPos), StringComparison.Ordinal);
    }

    private static int GetNumberFromVersionString(string lhs, out int nonDigitPos)
    {
        nonDigitPos = IndexOfFirstNonDigit(lhs);
        int number = -1;
        if (nonDigitPos > -1)
        {
            int.TryParse(lhs.Substring(0, nonDigitPos), out number);
        }
        else
        {
            int.TryParse(lhs, out number);
        }
        return number;
    }

    private static int IndexOfFirstNonDigit(string str)
    {
        for (var i = 0; i < str.Length; i++)
        {
            if (!char.IsDigit(str[i]))
            {
                return i;
            }
        }
        return -1;
    }
}
