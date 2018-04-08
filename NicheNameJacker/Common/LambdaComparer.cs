using System;
using System.Collections;
using System.Collections.Generic;

namespace NicheNameJacker.Common
{
    public class LambdaComparer<TObject, TProperty> : IComparer<TObject>, IComparer
        where TProperty : IComparable<TProperty>
    {
        readonly Func<TObject, TProperty> _propertySelector;

        public LambdaComparer(Func<TObject, TProperty> propertySelector)
        {
            _propertySelector = propertySelector;
        }

        public int Compare(object x, object y) => Compare((TObject)x, (TObject)y);

        public int Compare(TObject x, TObject y) => _propertySelector(x).CompareTo(_propertySelector(y));
    }
}
