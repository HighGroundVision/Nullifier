using HGV.Nullifier.Collection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HGV.Nullifier.Collection
{
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<IEnumerable<T>> DifferentCombinations<T>(this IEnumerable<T> elements, int k)
        {
            return k == 0 ? new[] { new T[0] } :
              elements.SelectMany((e, i) =>
                elements.Skip(i + 1).DifferentCombinations(k - 1).Select(c => (new[] {e}).Concat(c)));
        }

        public static List<int> GetItems(this PlayerHistory p)
        {
            var items = new List<int?>()
            {
                p.Item0, p.Item1, p.Item2, p.Item3, p.Item4, p.Item5,
                p.Backpack0, p.Backpack0, p.Backpack0,
                p.ItemNeutral,
            };
            return items.Where(_ => _.HasValue && _.Value > 0).Select(_ => _.GetValueOrDefault()).ToList();
        }

        public static List<int> GetAbilities(this PlayerHistory p)
        {
            return p.AbilityUpgrades.EmptyIfNull().Select(_ => _.Ability).Distinct().ToList();
        }
    }
}
