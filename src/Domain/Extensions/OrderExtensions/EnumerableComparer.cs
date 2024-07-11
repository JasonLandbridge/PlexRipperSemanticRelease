﻿namespace PlexRipper.Domain;

// From: https://github.com/postworthy/OrderByNatural/blob/master/OrderByNatural/EnumerableComparer.cs
// A special thanks for this class goes to: https://www.interact-sw.co.uk/iangblog/2007/12/13/natural-sorting
internal class EnumerableComparer<T> : IComparer<IEnumerable<T>>
{
    public EnumerableComparer()
    {
        comp = Comparer<T>.Default;
    }

    public EnumerableComparer(IComparer<T> comparer)
    {
        comp = comparer;
    }

    private IComparer<T> comp;

    public int Compare(IEnumerable<T> x, IEnumerable<T> y)
    {
        using (var leftIt = x.GetEnumerator())
        using (var rightIt = y.GetEnumerator())
        {
            while (true)
            {
                var left = leftIt.MoveNext();
                var right = rightIt.MoveNext();

                if (!(left || right))
                    return 0;

                if (!left)
                    return -1;
                if (!right)
                    return 1;

                var itemResult = comp.Compare(leftIt.Current, rightIt.Current);
                if (itemResult != 0)
                    return itemResult;
            }
        }
    }
}
