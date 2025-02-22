using System.Collections.Generic;

namespace Xabbo.Core;

/// <summary>
/// Represents a user's offer in a trade.
/// </summary>
public interface ITradeOffer : IReadOnlyList<ITradeItem>
{
    /// <summary>
    /// The ID of the user.
    /// </summary>
    Id UserId { get; }

    /// <summary>
    /// The number of furni in the trade offer.
    /// </summary>
    /// <remarks>
    /// Used on modern clients.
    /// </remarks>
    int FurniCount { get; }

    /// <summary>
    /// The number of credits in the trade offer.
    /// </summary>
    /// <remarks>
    /// Used on modern clients.
    /// </remarks>
    int CreditCount { get; }
}
