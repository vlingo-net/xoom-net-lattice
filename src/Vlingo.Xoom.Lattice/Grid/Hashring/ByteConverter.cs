// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

// borrowed from https://github.com/ayende/Maybe/blob/00cfe13e980c0914cf88e3520e9462aff09a212f/Maybe/Utilities/ByteConverter.cs
namespace Vlingo.Xoom.Lattice.Grid.Hashring
{
    /// <summary>
    /// Helper class to abstract serializing objects to bytes.
    /// </summary>
    public static class ByteConverter
    {
        /// <summary>
        /// Given a serializable object, returns the binary serialized representation of that object.
        /// </summary>
        /// <param name="item">The input to be serialized</param>
        /// <returns>Binary serialized representation of the input item.</returns>
        public static byte[] ConvertToByteArray(object item)
        {
            if (item == null)
            {
                return null;
            }

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, item);
                return stream.ToArray();
            }
        }
    }
}