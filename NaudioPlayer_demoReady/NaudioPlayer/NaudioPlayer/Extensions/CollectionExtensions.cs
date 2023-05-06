using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;

namespace NaudioPlayer.Extensions
{
    public static class CollectionExtensions
    {
        public static T NextItem<T>(this ObservableCollection<T> collection, T currentItem)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            int currentIndex = collection.IndexOf(currentItem);

            if (currentIndex == -1)
            {
                throw new InvalidOperationException("The current item is not part of the collection.");
            }

            if (currentIndex == collection.Count - 1)
            {
                return collection.First(); // If the current item is the last one, return the first item.
            }

            return collection[currentIndex + 1]; // Return the next item.
        }

        public static ObservableCollection<T> Shuffle<T>(this ObservableCollection<T> collection)
        {
            var random = new Random();
            for (int i = collection.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temp = collection[i];
                collection[i] = collection[j];
                collection[j] = temp;
            }
            return collection;
        }
    }

    public static class StringExtensions
    {
        public static string RemovePath(this string fullPath)
        {
            return Path.GetFileName(fullPath);
        }
    }
}