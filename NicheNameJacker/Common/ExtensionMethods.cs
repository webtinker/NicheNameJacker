using Newtonsoft.Json.Linq;
using NicheNameJacker.ViewModels;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Text.RegularExpressions;

namespace NicheNameJacker.Common
{
    public static class ExtensionMethods
    {
        public static IEnumerable<List<T>> ToChunks<T>(this IEnumerable<T> values, int chunkSize)
        {
            var chunkQuantity = Math.Ceiling(values.Count() * 1.0 / chunkSize);
            for (int i = 0; i < chunkQuantity; i++)
            {
                yield return values.Skip(i * chunkSize).Take(chunkSize).ToList();
            }
        }
        public static string FormattedString(this decimal num)
        {
            if (num >= 100000000)
                return (num / 1000000).ToString("#,0M");

            if (num >= 10000000)
                return (num / 1000000).ToString("0.#") + "M";

            if (num >= 100000)
                return (num / 1000).ToString("#,0K");

            if (num >= 10000)
                return (num / 1000).ToString("0.#") + "K";

            return num.ToString("0.##");
        }
        public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);

        public static ObservableCollection<T> AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
            return collection;
        }

        public static ObservableCollection<T> RemoveWhere<T>(this ObservableCollection<T> collection, Func<T, bool> predicate)
        {
            var i = 0;
            while (i < collection.Count)
            {
                var item = collection[i];
                if (predicate(item))
                {
                    collection.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            return collection;
        }

        public static ListView FilterBy<TProperty>(this ListView listView, Func<TProperty, bool> predicate)
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
            view.Filter = o => predicate((TProperty)o);
            return listView;
        }

        public static DataGrid FilterBy<TProperty>(this DataGrid dataGrid, Func<TProperty, bool> predicate)
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            view.Filter = o => predicate((TProperty)o);
            return dataGrid;
        }

        public static ListView SortBy<TItem, TPropery>(this ListView listView, Func<TItem, TPropery> sortFunc)
            where TPropery : IComparable<TPropery>
        {
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
            view.SortDescriptions.Clear();
            view.CustomSort = new LambdaComparer<TItem, TPropery>(sortFunc);
            return listView;
        }

        public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate) => !source.Any(predicate);

        public static IObservable<Unit> MergeUnits<T1, T2>(this IObservable<T1> x, IObservable<T2> y) =>
            x.SelectUnit().Merge(y.SelectUnit());

        public static IObservable<Unit> SelectUnit<T>(this IObservable<T> x) => x.Select(_ => Unit.Default);

        public static string JoinStrings<T>(this IEnumerable<T> source, string separator = "") =>
            string.Join(separator, source);

        public static string Invert(this string s) => new string(s.Reverse().ToArray());

        public static bool Isomorphic(this SingleDomain x, SingleDomain y) => x.Address.Equals(y.Address, StringComparison.InvariantCultureIgnoreCase);

        public static string ValueOf(this JToken token, string name) => token[name]?.ToString();

        public static bool? BoolValueOf(this JToken token, string name)
        {
            bool result;
            return bool.TryParse(token.ValueOf(name), out result) ? result : default(bool?);
        }

        public static decimal? ToDecimal(this string value)
        {
            decimal result;
            return decimal.TryParse(value, out result) ? result : default(decimal?);
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(dop)
                select Task.Run(async delegate
                {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current);
                }));
        }

        public static string CreateExtensionsRegex(this string source, IEnumerable<string> extensions)
            {
            var extensionsRegex = string.Join ("|", extensions.Select (x => x.Replace (".", @"\.")));
            return extensionsRegex;
            }

        public static string urlPattern => @"([a-zA-Z]+\.)*[a-zA-Z0-9]+\.(com|org|net|int|edu|gov|mil)";
        public static string CleanDomainUrl(this string source)
            {
            var matching = Regex.Match (source, urlPattern, RegexOptions.Singleline);
            return matching.ToString ();
            }
        }
}
