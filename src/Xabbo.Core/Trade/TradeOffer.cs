using System;
using System.Collections.Generic;

using Xabbo.Messages;

namespace Xabbo.Core;

/// <inheritdoc cref="ITradeOffer"/>
public sealed class TradeOffer : List<TradeItem>, ITradeOffer, IParserComposer<TradeOffer>
{
    public Id UserId { get; set; }
    /// <remarks>
    /// This appears to be an unused field in the Origins packet structure as users will
    /// automatically unaccept the trade whenever a trade offer updates.
    /// </remarks>
    public bool Accepted { get; set; }
    public int FurniCount { get; set; }
    public int CreditCount { get; set; }

    ITradeItem IReadOnlyList<ITradeItem>.this[int index] => this[index];
    IEnumerator<ITradeItem> IEnumerable<ITradeItem>.GetEnumerator() => GetEnumerator();

    public TradeOffer() { }

    private TradeOffer(in PacketReader p) : this()
    {
        UserId = p.ReadId();

        AddRange(p.ParseArray<TradeItem>());

        if (p.Client is not ClientType.Shockwave)
        {
            FurniCount = p.ReadInt();
            CreditCount = p.ReadInt();
        }
    }

    void IComposer.Compose(in PacketWriter p)
    {
        p.WriteId(UserId);
        p.ComposeArray<TradeItem>(this);

        if (p.Client is not ClientType.Shockwave)
        {
            p.WriteInt(FurniCount);
            p.WriteInt(CreditCount);
        }
    }

    static TradeOffer IParser<TradeOffer>.Parse(in PacketReader p) => new(in p);
}
