using System;
using System.Collections.Generic;
using System.Text;

namespace Kimbokchi
{
    public static class Sort
    {
        public static void MergeSort<T>(this T[] array, Func<T, T, bool> IsRValueBigger)
        {
            MergeSort(array, 0, array.Length - 1, new T[array.Length], IsRValueBigger);
        }
        public static void MergeSort<T>(this T[] array, int min, int max, T[] tempArray, Func<T, T, bool> IsRValueBigger)
        {
            if (min >= max) return;

            int mid = (min + max) / 2;

            MergeSort(array, min,     mid, tempArray, IsRValueBigger);
            MergeSort(array, mid + 1, max, tempArray, IsRValueBigger);

            int i = min;
            int j = mid + 1;

            for (int k = min; k <= max; k++)
            {
                if (i > mid)
                {
                    tempArray[k] = array[j++];
                }
                else if (j > max)
                {
                    tempArray[k] = array[i++];
                }
                else if(IsRValueBigger(array[i], array[j]))
                {
                    tempArray[k] = array[i++];
                }
                else
                {
                    tempArray[k] = array[j++];
                }
            }
            for (i = min; i <= max; i++) array[i] = tempArray[i];
        }
        public static void MergeSort<T>(this List<T> list, Func<T, T, bool> IsRValueBigger)
        {
            MergeSort(list, 0, list.Count - 1, new List<T>(list), IsRValueBigger);
        }
        public static void MergeSort<T>(this List<T> list, int min, int max, List<T> tempList, Func<T, T, bool> IsRValueBigger)
        {
            if (min >= max) return;

            int mid = (min + max) / 2;

            MergeSort(list, min,     mid, tempList, IsRValueBigger);
            MergeSort(list, mid + 1, max, tempList, IsRValueBigger);

            int i = min;
            int j = mid + 1;

            for (int k = min; k <= max; k++)
            {
                if (i > mid)
                {
                    tempList[k] = list[j++];
                }
                else if (j > max)
                {
                    tempList[k] = list[i++];
                }
                else if (IsRValueBigger(list[i], list[j]))
                {
                    tempList[k] = list[i++];
                }
                else
                {
                    tempList[k] = list[j++];
                }
            }
            for (i = min; i <= max; i++) list[i] = tempList[i];
        }

        public static void SelectionSort<T>(this T[] array, Func<T, T, bool> IsRValueBigger)
        {
            int i, j, minIndex, length = array.Length;

            for (i = 0; i < length - 1; ++i)
            {
                minIndex = i;

                for (j = i + 1; j < length; ++j)
                {
                    if (IsRValueBigger(array[j], array[minIndex]))
                    {
                        minIndex = j;
                    }
                }
                array.Swap(minIndex, i);
            }
        }
        public static void SelectionSort<T>(this List<T> list, Func<T, T, bool> IsRValueBigger)
        {
            int i, j, minIndex, length = list.Count;

            for (i = 0; i < length - 1; ++i)
            {
                minIndex = i;

                for (j = i + 1; j < length; ++j)
                {
                    if (IsRValueBigger(list[j], list[minIndex]))
                    {
                        minIndex = j;
                    }
                }
                list.Swap(minIndex, i);
            }
        }

        public static void InsertionSort<T>(this T[] array, Func<T, T, bool> IsRValueBigger)
        {
            T remember;

            int i, j;

            for (i = 1; i < array.Length; i++)
            {
                remember = array[j = i];

                while (--j >= 0 && IsRValueBigger(remember, array[j]))
                {
                    array[j + 1] = array[j];
                    array[j] = remember;
                }
            }
        }
        public static void InsertionSort<T>(this List<T> list, Func<T, T, bool> IsRValueBigger)
        {
            T remember;

            int i, j;

            for (i = 1; i < list.Count; i++)
            {
                remember = list[j = i];

                while (--j >= 0 && IsRValueBigger(remember, list[j]))
                {
                    list[j + 1] = list[j];
                    list[j] = remember;
                }
            }
        }
    }
}
