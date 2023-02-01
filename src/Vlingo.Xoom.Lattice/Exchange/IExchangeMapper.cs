// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Exchange;

/// <summary>
/// Supports mapping a local type to external type, and a external type to local type.
/// </summary>
/// <typeparam name="TLocal">The local type</typeparam>
/// <typeparam name="TExternal">The external type</typeparam>
public interface IExchangeMapper<TLocal, TExternal>
{
    /// <summary>
    /// Gets the external type given the local type.
    /// </summary>
    /// <param name="local">The <typeparamref name="TLocal"/> local type to map</param>
    /// <returns>The <typeparamref name="TExternal"/> typed message</returns>
    TExternal LocalToExternal(TLocal local);
        
    /// <summary>
    /// Gets the local type given the external type.
    /// </summary>
    /// <param name="external">The <typeparamref name="TExternal"/> external type to map</param>
    /// <returns>The <typeparamref name="TLocal"/> typed message</returns>
    TLocal ExternalToLocal(TExternal external);
}