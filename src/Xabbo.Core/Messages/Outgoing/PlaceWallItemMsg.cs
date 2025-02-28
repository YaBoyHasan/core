using System;

using Xabbo.Messages;
using Xabbo.Messages.Flash;

namespace Xabbo.Core.Messages.Outgoing;

/// <summary>
/// Sent when placing a wall item in a room.
/// <para/>
/// Supported clients: <see cref="ClientType.Flash"/>, <see cref="ClientType.Shockwave"/>
/// <para/>
/// Identifiers:
/// <list type="bullet">
/// <item>Flash: <see cref="Out.PlaceObject"/></item>
/// <item>Shockwave: <see cref="Xabbo.Messages.Shockwave.Out.PLACEITEM"/></item>
/// </list>
/// </summary>
/// <param name="ItemId">The ID of the wall item to place.</param>
/// <param name="Location">The location to place the wall item at.</param>
public sealed record PlaceWallItemMsg(Id ItemId, WallLocation Location) : IMessage<PlaceWallItemMsg>
{
    static Identifier IMessage<PlaceWallItemMsg>.Identifier => Identifier.Unknown;

    Identifier IMessage.GetIdentifier(ClientType client) => client switch
    {
        ClientType.Shockwave => Xabbo.Messages.Shockwave.Out.PLACEITEM,
        not ClientType.Shockwave => Out.PlaceObject,
    };

    static bool IMessage<PlaceWallItemMsg>.UseTargetedIdentifiers => true;

    static bool IMessage<PlaceWallItemMsg>.Match(in PacketReader p)
    {
        if (p.Client is ClientType.Shockwave)
        {
            return true;
        }

        string content = p.ReadString();
        int index = content.IndexOf(' ');
        if (index < 0 || (index + 1) >= content.Length) return false;
        return content[index + 1] == ':';
    }

    static PlaceWallItemMsg IParser<PlaceWallItemMsg>.Parse(in PacketReader p)
    {
        Id itemId;
        WallLocation location;

        if (p.Client is ClientType.Shockwave)
        {
            itemId = p.ReadInt();

            string locationStr = p.ReadString();
            if (!WallLocation.TryParse(locationStr, out location))
                throw new Exception($"Failed to parse {nameof(Location)} in {nameof(PlaceWallItemMsg)}: '{locationStr}'.");
        }
        else
        {
            string content = p.ReadString();

            int i = content.IndexOf(' ');
            if (i < 0)
                throw new Exception("No separator in PlaceWallItemMsg.");

            if (!Id.TryParse(content[..i], out itemId))
                throw new Exception($"Failed to parse {nameof(ItemId)} in {nameof(PlaceWallItemMsg)}: '{content[..i]}'.");

            if (!WallLocation.TryParse(content[(i + 1)..], out location))
                throw new Exception($"Failed to parse {nameof(Location)} in {nameof(PlaceWallItemMsg)}: '{content[(i + 1)..]}'.");
        }

        return new PlaceWallItemMsg(itemId, location);
    }

    void IComposer.Compose(in PacketWriter p)
    {
        switch (p.Client)
        {
            case ClientType.Flash:
                p.WriteString($"{ItemId} {Location}");
                break;
            case ClientType.Shockwave:
                p.WriteId(ItemId);
                p.WriteString(Location.ToString());
                break;
            default:
                throw new UnsupportedClientException(p.Client);
        }
    }
}