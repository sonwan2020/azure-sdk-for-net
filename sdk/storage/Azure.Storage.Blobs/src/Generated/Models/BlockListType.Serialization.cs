// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;

namespace Azure.Storage.Blobs.Models
{
    internal static partial class BlockListTypeExtensions
    {
        public static string ToSerialString(this BlockListType value) => value switch
        {
            BlockListType.Committed => "committed",
            BlockListType.Uncommitted => "uncommitted",
            BlockListType.All => "all",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown BlockListType value.")
        };

        public static BlockListType ToBlockListType(this string value)
        {
            if (string.Equals(value, "committed", StringComparison.InvariantCultureIgnoreCase)) return BlockListType.Committed;
            if (string.Equals(value, "uncommitted", StringComparison.InvariantCultureIgnoreCase)) return BlockListType.Uncommitted;
            if (string.Equals(value, "all", StringComparison.InvariantCultureIgnoreCase)) return BlockListType.All;
            throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown BlockListType value.");
        }
    }
}
