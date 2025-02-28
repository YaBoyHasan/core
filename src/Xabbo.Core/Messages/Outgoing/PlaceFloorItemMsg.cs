using System;

using Xabbo.Messages;
using Xabbo.Messages.Flash;

namespace Xabbo.Core.Messages.Outgoing;

/// <summary>
/// Sent when placing a floor item in a room.
/// <para/>
/// Supported clients: <see cref="ClientType.Flash"/>, <see cref="ClientType.Shockwave"/>
/// <para/>
/// Identifiers:
/// <list type="bullet">
/// <item>Flash: <see cref="Out.PlaceObject"/></item>
/// <item>Shockwave: <see cref="Xabbo.Messages.Shockwave.Out.PLACESTUFF"/></item>
/// </list>
/// </summary>
/// <param name="ItemId">The ID of the floor item to place.</param>
/// <param name="Location">The location to place the item at.</param>
/// <param name="Direction">The direction to place the item in.</param>
public sealed record PlaceFloorItemMsg(Id ItemId, Point Location, int Direction)
    : IMessage<PlaceFloorItemMsg>
{
    static Identifier IMessage<PlaceFloorItemMsg>.Identifier => Out.PlaceObject;

    static bool IMessage<PlaceFloorItemMsg>.Match(in PacketReader p)
    {
        if (p.Client is ClientType.Shockwave)
        {
            return true;
        }

        string content = p.ReadString();
        int index = content.IndexOf(' ');
        if (index < 0 || (index + 1) >= content.Length) return false;
        return content[index + 1] != ':';
    }

    /// <summary>
    /// Gets the X coordinate.
    /// </summary>
    public int X => Location.X;

    /// <summary>
    /// Gets the Y coordinate.
    /// </summary>
    public int Y => Location.Y;

    public PlaceFloorItemMsg(Id itemId, int X, int Y, int direction)
        : this(itemId, (X, Y), direction)
    { }

    static PlaceFloorItemMsg IParser<PlaceFloorItemMsg>.Parse(in PacketReader p)
    {
        Id itemId;
        int x, y, direction;

        if (p.Client is ClientType.Shockwave)
        {
            itemId = p.ReadId();
            x = p.ReadInt();
            y = p.ReadInt();
            direction = p.ReadInt();
        }
        else
        {
            string content = p.ReadString();
            string[] fields = content.Split();

            if (!Id.TryParse(fields[0], out itemId))
                throw new Exception($"Failed to parse ItemId in PlaceFloorItemMsg: '{fields[0]}'.");
            if (!int.TryParse(fields[1], out x))
                throw new Exception($"Failed to parse X in PlaceFloorItemMsg: '{fields[1]}'.");
            if (!int.TryParse(fields[2], out y))
                throw new Exception($"Failed to parse Y in PlaceFloorItemMsg: '{fields[2]}'.");
            if (!int.TryParse(fields[3], out direction))
                throw new Exception($"Failed to parse Direction in PlaceFloorItemMsg: '{fields[3]}'.");
        }

        return new PlaceFloorItemMsg(itemId, x, y, direction);
    }

    void IComposer.Compose(in PacketWriter p)
    {
        switch (p.Client)
        {
            case ClientType.Flash:
                p.WriteString($"{ItemId} {X} {Y} {Direction}");
                break;
            case ClientType.Shockwave:
                p.WriteId(ItemId);
                p.WriteInt(X);
                p.WriteInt(Y);
                p.WriteInt(Direction);
                break;
            default:
                throw new UnsupportedClientException(p.Client);
        }
    }
}