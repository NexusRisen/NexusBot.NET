using System;
using System.Collections.Generic;

namespace SysBot.Pokemon;

public class TrackedUserLog
{
    private const int Capacity = 1000;
    private readonly List<TrackedUser> Users = new(Capacity);
    private readonly Dictionary<ulong, TrackedUser> NetworkCache = new(Capacity);
    private readonly Dictionary<ulong, TrackedUser> RemoteCache = new(Capacity);
    private readonly object _sync = new();
    private int ReplaceIndex;

    public TrackedUser? TryRegister(ulong networkID, string name, ulong remoteID)
    {
        if (remoteID == 0)
            return null;

        lock (_sync)
            return InsertReplace(networkID, name, remoteID);
    }

    public TrackedUser? TryRegister(ulong networkID, string name)
    {
        lock (_sync)
            return InsertReplace(networkID, name);
    }

    private TrackedUser? InsertReplace(ulong networkID, string name, ulong remoteID = 0)
    {
        if (NetworkCache.TryGetValue(networkID, out var match))
        {
            var index = Users.FindIndex(z => z.NetworkID == networkID);
            if (index >= 0)
            {
                var user = new TrackedUser(networkID, name, remoteID);
                Users[index] = user;
                NetworkCache[networkID] = user;
                if (match.RemoteID != 0) RemoteCache.Remove(match.RemoteID);
                if (remoteID != 0) RemoteCache[remoteID] = user;
                return match;
            }
        }

        Insert(networkID, name, remoteID);
        return null;
    }

    private void Insert(ulong id, string name, ulong remoteID)
    {
        var user = new TrackedUser(id, name, remoteID);
        if (Users.Count != Capacity)
        {
            Users.Add(user);
        }
        else
        {
            var old = Users[ReplaceIndex];
            NetworkCache.Remove(old.NetworkID);
            if (old.RemoteID != 0) RemoteCache.Remove(old.RemoteID);

            Users[ReplaceIndex] = user;
            ReplaceIndex = (ReplaceIndex + 1) % Capacity;
        }

        NetworkCache[id] = user;
        if (remoteID != 0) RemoteCache[remoteID] = user;
    }

    public void RemoveAllNID(ulong networkID)
    {
        lock (_sync)
        {
            Users.RemoveAll(z =>
            {
                if (z.NetworkID == networkID)
                {
                    NetworkCache.Remove(z.NetworkID);
                    if (z.RemoteID != 0) RemoteCache.Remove(z.RemoteID);
                    return true;
                }
                return false;
            });
        }
    }

    public void RemoveAllRemoteID(ulong remoteID)
    {
        lock (_sync)
        {
            Users.RemoveAll(z =>
            {
                if (z.RemoteID == remoteID)
                {
                    NetworkCache.Remove(z.NetworkID);
                    RemoteCache.Remove(z.RemoteID);
                    return true;
                }
                return false;
            });
        }
    }

    public TrackedUser? TryGetPreviousNID(ulong trainerNid)
    {
        lock (_sync)
            return NetworkCache.GetValueOrDefault(trainerNid);
    }

    public TrackedUser? TryGetPreviousRemoteID(ulong remoteNid)
    {
        lock (_sync)
            return RemoteCache.GetValueOrDefault(remoteNid);
    }

    public IEnumerable<string> Summarize()
    {
        lock (_sync)
            return Users.FindAll(z => z.NetworkID != 0).ConvertAll(z => $"{z.Name}, ID: {z.NetworkID}, Remote ID: {z.RemoteID}");
    }
}

public sealed record TrackedUser
{
    public readonly string Name;
    public readonly ulong RemoteID;
    public readonly ulong NetworkID;
    public readonly DateTime Time;

    public TrackedUser(ulong NetworkID, string name, ulong remoteID)
    {
        this.NetworkID = NetworkID;
        Name = name;
        RemoteID = remoteID;
        Time = DateTime.UtcNow;
    }
}
