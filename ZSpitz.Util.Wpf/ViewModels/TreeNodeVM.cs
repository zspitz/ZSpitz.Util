﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ZSpitz.Util;
using static System.Linq.Enumerable;
using static ZSpitz.Util.Wpf.FilterStates;

namespace ZSpitz.Util.Wpf {
    public class TreeNodeVM<TData> : TreeNodeBase<TData, TreeNodeVM<TData>, ReadOnlyObservableCollection<TreeNodeVM<TData>>>, INotifyPropertyChanged {
        private readonly ObservableCollection<TreeNodeVM<TData>> oc = new();
        protected override (IList<TreeNodeVM<TData>> innerCollection, ReadOnlyObservableCollection<TreeNodeVM<TData>> collectionWrapper) InitWith() {
            var roc = new ReadOnlyObservableCollection<TreeNodeVM<TData>>(oc);
            return (oc, roc);
        }

        public TreeNodeVM() : this(default!, default) { }

        public TreeNodeVM([AllowNull] TData data = default, IEnumerable<TData>? children = default) : base(data, children) {
            IsSelected =
                Children.Any() ? Children.Select(x => x.IsSelected).Unanimous() :
                false;

            oc.CollectionChanged += (s, e) => {
                IsSelected = Children.Select(x => x.IsSelected).Unanimous();
                if (FilterState == DescendantMatched && Children.None(x => x.FilterState.In(Matched, DescendantMatched))) {
                    FilterState = NotMatched;
                } else if (FilterState == NotMatched && Children.Any(x => x.FilterState.In(Matched,DescendantMatched))) {
                    FilterState = DescendantMatched;
                }
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool? isSelected;
        public bool? IsSelected {
            get => isSelected;
            set {
                var old = isSelected;
                if (old == value) { return; }
                isSelected = value;

                if (value is { }) {
                    Children.ForEach(x => x.IsSelected = value);
                }

                if (Parent is { }) {
                    Parent.IsSelected = 
                        value is null || Parent.Children.Any(x => x.IsSelected != value) ? 
                            null : 
                            value;
                }

                // TODO If parent selection has been changed, parent notification will arrive before current node's notification
                //      Not sure if this is an issue
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        private FilterStates? filterState;
        public FilterStates? FilterState {
            get => filterState;
            set {
                if (filterState == value) { return; }

                var hasMatchedDescendant = Children.Any(x => x.FilterState.In(Matched, DescendantMatched));

                if (value == NotMatched && hasMatchedDescendant) {
                    value = DescendantMatched;
                } else if (value == DescendantMatched && !hasMatchedDescendant) {
                    value = NotMatched;
                }
                if (filterState == value) { return; }

                var old = filterState;
                filterState = value;

                // Only overwrite parent filter state if it reflects state of children, which null and Matched do not
                if (Parent is { } && Parent.FilterState.In(NotMatched, DescendantMatched)) {
                    if (old.In(null, NotMatched) && value.In(Matched, DescendantMatched)) {
                        Parent.FilterState = DescendantMatched; // will coerce to NotMatched if parent has no other children with Matched/DescendantMatched
                    } else if (old.In(Matched, DescendantMatched) && value.In(null, NotMatched)) {
                        Parent.FilterState = NotMatched; // will coerce to DescendantMatched if parent has other children with Matched/DescendantMatched
                    }
                }

                // TODO if parent filter-state has been changed, parent notification will arrive before current node's notification
                // Not sure if this is an issue
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterState)));
            }
        }

        public void ApplyFilter(Func<TData?, bool> predicate) {
            var matched = predicate(Data);
            FilterState = matched ? Matched : NotMatched;

            Children.ForEach(x => x.ApplyFilter(predicate));
        }

        /// <summary>Clears the filter state from this and all descendants</summary>
        public void ResetFilter() {
            FilterState = null;
            Children.ForEach(x => x.ResetFilter());
        }
    }
}
