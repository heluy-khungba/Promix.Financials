using Promix.Financials.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Promix.Financials.UI.Security;

public sealed class LocalSettingsSessionStore : ISessionStore
{
    private const string ActiveUserIdKey = "ActiveUserId";
    private const string SessionsKey = "UserSessions";      // Dictionary<Guid,string(json)>
    private const string LastUsernameKey = "LastUsername";

    private static ApplicationDataContainer Settings => ApplicationData.Current.LocalSettings;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General);

    // ✅ Runtime-only session (cleared automatically when app closes)
    private AppSession? _currentSession;

    public Task<AppSession?> LoadAsync(CancellationToken ct = default)
    {
        // 1) runtime session first
        if (_currentSession is not null)
            return Task.FromResult<AppSession?>(_currentSession);

        // 2) try restore from persisted (RememberMe)
        var activeUserId = LoadActiveUserId();
        if (activeUserId is null)
            return Task.FromResult<AppSession?>(null);

        var session = LoadPersistedSession(activeUserId.Value);
        if (session is null)
            return Task.FromResult<AppSession?>(null);

        // Validate expiry (extra safety)
        if (session.IsExpired(DateTimeOffset.UtcNow))
        {
            RemovePersistedSession(activeUserId.Value);
            Settings.Values.Remove(ActiveUserIdKey);
            return Task.FromResult<AppSession?>(null);
        }

        _currentSession = session;
        return Task.FromResult<AppSession?>(_currentSession);
    }

    public Task SaveAsync(AppSession session, bool persistent, CancellationToken ct = default)
    {
        _currentSession = session;

        if (persistent)
        {
            SavePersistedSession(session.UserId, session);
            Settings.Values[ActiveUserIdKey] = session.UserId.ToString();
        }

        // If not persistent => do NOT write anything persistent, so restart requires login.
        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken ct = default)
    {
        // Clear runtime session always
        var activeUserId = LoadActiveUserId();
        _currentSession = null;

        // Clear only the active user's persisted session (professional behavior)
        if (activeUserId is not null)
            RemovePersistedSession(activeUserId.Value);

        Settings.Values.Remove(ActiveUserIdKey);
        return Task.CompletedTask;
    }

    public Task<Guid?> LoadActiveUserIdAsync(CancellationToken ct = default)
        => Task.FromResult<Guid?>(LoadActiveUserId());

    public Task SetActiveUserIdAsync(Guid? userId, CancellationToken ct = default)
    {
        if (userId is null)
            Settings.Values.Remove(ActiveUserIdKey);
        else
            Settings.Values[ActiveUserIdKey] = userId.Value.ToString();

        return Task.CompletedTask;
    }

    public Task<string?> LoadLastUsernameAsync(CancellationToken ct = default)
    {
        if (Settings.Values.TryGetValue(LastUsernameKey, out var raw) && raw is string s)
            return Task.FromResult<string?>(s);

        return Task.FromResult<string?>(null);
    }

    public Task SaveLastUsernameAsync(string? username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            Settings.Values.Remove(LastUsernameKey);
        else
            Settings.Values[LastUsernameKey] = username.Trim();

        return Task.CompletedTask;
    }

    // ----------------- helpers -----------------

    private static Guid? LoadActiveUserId()
    {
        if (Settings.Values.TryGetValue(ActiveUserIdKey, out var raw) && raw is string s && Guid.TryParse(s, out var id))
            return id;

        return null;
    }

    private static Dictionary<string, string> LoadSessionsDictionary()
    {
        if (Settings.Values.TryGetValue(SessionsKey, out var raw) && raw is string json && !string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions);
                return dict ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        return new Dictionary<string, string>();
    }

    private static void SaveSessionsDictionary(Dictionary<string, string> dict)
    {
        Settings.Values[SessionsKey] = JsonSerializer.Serialize(dict, JsonOptions);
    }

    private static AppSession? LoadPersistedSession(Guid userId)
    {
        var dict = LoadSessionsDictionary();
        if (!dict.TryGetValue(userId.ToString(), out var sessionJson) || string.IsNullOrWhiteSpace(sessionJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<AppSession>(sessionJson, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static void SavePersistedSession(Guid userId, AppSession session)
    {
        var dict = LoadSessionsDictionary();
        dict[userId.ToString()] = JsonSerializer.Serialize(session, JsonOptions);
        SaveSessionsDictionary(dict);
    }

    private static void RemovePersistedSession(Guid userId)
    {
        var dict = LoadSessionsDictionary();
        if (dict.Remove(userId.ToString()))
            SaveSessionsDictionary(dict);
    }
}