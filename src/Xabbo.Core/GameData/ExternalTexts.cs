using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;

namespace Xabbo.Core.GameData;

/// <summary>
/// Defines a dictionary of external texts.
/// </summary>
public sealed class ExternalTexts : IReadOnlyDictionary<string, string>
{
    public static ExternalTexts Load(string path) => new(path);

    private readonly ImmutableDictionary<string, string> _texts;

    public IEnumerable<string> Keys => _texts.Keys;
    public IEnumerable<string> Values => _texts.Values;
    public int Count => _texts.Count;

    public string this[string key] => _texts[key];

    internal ExternalTexts(string path)
    {
        Dictionary<string, string> texts = [];

        foreach (string line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            int index = line.IndexOf('=');
            if (index < 0)
            {
                Debug.Log($"Invalid line in external texts: '{line}'.");
                continue;
            }
            string key = line[..index];
            string value = line[(index + 1)..];
            if (!texts.TryAdd(key, value))
                Debug.Log($"Duplicate key in external texts: '{key}'.");
        }

        _texts = texts.ToImmutableDictionary();
    }

    public bool ContainsKey(string key) => _texts.ContainsKey(key);
    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => _texts.TryGetValue(key, out value);
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _texts.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Attempts to get a poster name by its variant from the external texts.
    /// </summary>
    public bool TryGetPosterName(string variant, [NotNullWhen(true)] out string? name)
        => TryGetValue($"poster_{variant}_name", out name);

    /// <summary>
    /// Attempts to get a poster description by its variant from the external texts.
    /// </summary>
    public bool TryGetPosterDescription(string variant, [NotNullWhen(true)] out string? description)
        => TryGetValue($"poster_{variant}_desc", out description);

    /// <summary>
    /// Attempts to get a badge name by its code from the external texts.
    /// </summary>
    public bool TryGetBadgeName(string code, [NotNullWhen(true)] out string? name)
        => TryGetValue($"badge_name_{code}", out name) || TryGetValue($"{code}_badge_name", out name);

    /// <summary>
    /// Gets a badge name by its code from the external texts. Returns <c>null</c> if it is not found.
    /// </summary>
    public string? GetBadgeName(string code)
        => TryGetBadgeName(code, out string? name) ? name : null;

    /// <summary>
    /// Attempts to get a badge description by its code from the external texts.
    /// </summary>
    public bool TryGetBadgeDescription(string code, [NotNullWhen(true)] out string? description)
        => TryGetValue($"badge_desc_{code}", out description);

    /// <summary>
    /// Gets a badge description by its code from the external texts. Returns <c>null</c> if it is not found.
    /// </summary>
    public string? GetBadgeDescription(string code)
        => TryGetBadgeDescription(code, out string? description) ? description : null;

    /// <summary>
    /// Attempts to get an effect name by its ID from the external texts.
    /// </summary>
    public bool TryGetEffectName(int id, [NotNullWhen(true)] out string? name)
        => TryGetValue($"fx_{id}", out name);

    /// <summary>
    /// Gets an effect name by its ID from the external texts. Returns <c>null</c> if it is not found.
    /// </summary>
    public string? GetEffectName(int id)
        => TryGetEffectName(id, out string? name) ? name : null;

    /// <summary>
    /// Attempts to get an effect description by its ID from the external texts.
    /// </summary>
    public bool TryGetEffectDescription(int id, [NotNullWhen(true)] out string? description)
        => TryGetValue($"fx_{id}_desc", out description);

    /// <summary>
    /// Gets an effect description by its ID from the external texts. Returns <c>null</c> if it is not found.
    /// </summary>
    public string? GetEffectDescription(int id)
        => TryGetEffectDescription(id, out string? description) ? description : null;

    /// <summary>
    /// Attempts to get a hand item name by its ID from the external texts.
    /// </summary>
    public bool TryGetHandItemName(int id, [NotNullWhen(true)] out string? name)
        => TryGetValue($"handitem{id}", out name);

    /// <summary>
    /// Gets a hand item name by its ID from the external texts. Returns <c>null</c> if it is not found.
    /// </summary>
    public string? GetHandItemName(int id)
        => TryGetHandItemName(id, out string? name) ? name : null;

    /// <summary>
    /// Gets all hand item IDs matching the specified name from the external texts.
    /// </summary>
    public IEnumerable<int> GetHandItemIds(string name)
    {
        foreach (var (key, value) in this.Where(x => x.Value.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            if (key.StartsWith("handitem"))
                if (int.TryParse(key[8..], out int id))
                    yield return id;
        }
    }
}
