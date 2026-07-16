namespace SchoolManagement.Application.Common
{
    public static class CollectionSynchronizer
    {
        /// <summary>
        /// Synchronizes the contents of an existing collection with an incoming sequence by updating, adding, or
        /// removing items as necessary.
        /// </summary>
        /// <typeparam name="TExisting">The type of elements in the existing collection.</typeparam>
        /// <typeparam name="TIncoming">The type of elements in the incoming sequence.</typeparam>
        /// <typeparam name="TKey">The type of the key used to match elements.</typeparam>
        /// <param name="existing">The collection to be synchronized.</param>
        /// <param name="incoming">The incoming sequence to synchronize with.</param>
        /// <param name="existingKey">A function to extract the key from an existing element.</param>
        /// <param name="incomingKey">A function to extract the key from an incoming element.</param>
        /// <param name="update">An action to update an existing element with an incoming element.</param>
        /// <param name="create">A function to create a new existing element from an incoming element.</param>
        public static void Sync<TExisting, TIncoming, TKey>(
            ICollection<TExisting> existing,
            IEnumerable<TIncoming> incoming,
            Func<TExisting, TKey> existingKey,
            Func<TIncoming, TKey> incomingKey,
            Action<TExisting, TIncoming> update,
            Func<TIncoming, TExisting> create)
            where TKey : notnull
        {
            ArgumentNullException.ThrowIfNull(existing);
            ArgumentNullException.ThrowIfNull(incoming);
            ArgumentNullException.ThrowIfNull(existingKey);
            ArgumentNullException.ThrowIfNull(incomingKey);
            ArgumentNullException.ThrowIfNull(update);
            ArgumentNullException.ThrowIfNull(create);

            if (existing.IsReadOnly)
                throw new InvalidOperationException("The existing collection is read-only.");

            // Single enumeration of incoming to avoid double-enumeration bugs
            List<TIncoming> incomingList = [.. incoming];

            // Build existing lookup, throwing on duplicate keys (data integrity)
            Dictionary<TKey, TExisting> existingLookup = new(existing.Count);
            foreach (TExisting item in existing)
            {
                TKey key = existingKey(item);
                if (!existingLookup.TryAdd(key, item))
                    throw new InvalidOperationException($"Duplicate key '{key}' found in the existing collection.");
            }

            // Build incoming lookup (last-write-wins for duplicate keys)
            Dictionary<TKey, TIncoming> incomingLookup = new(incomingList.Count);
            foreach (TIncoming item in incomingList)
            {
                TKey key = incomingKey(item);
                incomingLookup[key] = item;
            }

            // Update existing matches, create new entries
            foreach (TIncoming value in incomingList)
            {
                TKey key = incomingKey(value);

                if (existingLookup.TryGetValue(key, out var existed))
                {
                    update(existed, value);
                }
                else
                {
                    TExisting created = create(value);
                    existing.Add(created);
                    existingLookup.Add(key, created);
                }
            }

            // Remove items no longer present in incoming
            List<TExisting> toRemove = existing
                .Where(x => !incomingLookup.ContainsKey(existingKey(x)))
                .ToList();

            foreach (TExisting item in toRemove)
            {
                existing.Remove(item);
            }
        }
    }
}
