using System.Collections.Generic;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// The reserved people for patrons. Built once from its own list; ambient pedestrians are never put in it and
    /// never take from it, so patrons cannot be starved by the crowd (and the crowd cannot lose people to patrons).
    /// </summary>
    public sealed class PatronPool<T> where T : class
    {
        private readonly Stack<T> _free = new Stack<T>();

        /// <summary>Creates the pool holding <paramref name="members"/>, all free.</summary>
        public PatronPool(IReadOnlyList<T> members)
        {
            for (var i = 0; i < members.Count; i++) _free.Push(members[i]);
        }

        /// <summary>How many members are free.</summary>
        public int FreeCount => _free.Count;

        /// <summary>Takes a free member, or returns false when all are out.</summary>
        public bool TryRent(out T member)
        {
            member = _free.Count > 0 ? _free.Pop() : null;
            return member != null;
        }

        /// <summary>Gives a member back.</summary>
        public void Return(T member) => _free.Push(member);
    }
}
