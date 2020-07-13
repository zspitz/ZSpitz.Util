using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ZSpitz.Util;
using static System.Linq.Enumerable;
using static ZSpitz.Util.Wpf.FilterStates;

namespace ZSpitz.Util.Wpf {
    public class TreeNodeVM<TData> : TreeNodeBase<TData, TreeNodeVM<TData>, ReadOnlyObservableCollection<TreeNodeVM<TData>>>, INotifyPropertyChanged {
        private readonly ObservableCollection<TreeNodeVM<TData>> oc = new ObservableCollection<TreeNodeVM<TData>>();
        protected override (IList<TreeNodeVM<TData>> innerCollection, ReadOnlyObservableCollection<TreeNodeVM<TData>> collectionWrapper) InitWith() {
            var roc = new ReadOnlyObservableCollection<TreeNodeVM<TData>>(oc);
            return (oc, roc);
        }

        public TreeNodeVM() : this(default!, default) { }

        public TreeNodeVM(TData data = default, IEnumerable<TData>? children = default) : base(data, children) {
            IsSelected = Children.Select(x => x.IsSelected).Unanimous(null);
            oc.CollectionChanged += (s, e) => {
                IsSelected = Children.Select(x => x.IsSelected).Unanimous(null);
                if (FilterState == DescendantMatched && Children.None(x => x.FilterState.In(Matched, DescendantMatched))) {
                    FilterState = NotMatched;
                } else if (FilterState == NotMatched && Children.Any(x => x.FilterState.In(Matched,DescendantMatched))) {
                    FilterState = DescendantMatched;
                }
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool? _isSelected;
        public bool? IsSelected {
            get => _isSelected;
            set {
                var old = _isSelected;
                if (old == value) { return; }
                _isSelected = value;

                if (value is { }) {
                    Children.ForEach(x => x.IsSelected = value);
                }

                if (Parent is { }) {
                    if (value is null) {
                        Parent.IsSelected = null;
                    } else if (Parent.Children.Any(x => x.IsSelected != value)) {
                        Parent.IsSelected = null;
                    } else {
                        Parent.IsSelected = value;
                        // This doesn't cause a recursive loop (Parent.IsSelected sets IsSelected for each
                        // child) because the underlying field has already been set on the child, and we exit early
                    }
                }

                // TODO If parent selection has been changed, parent notification will arrive before current node's notification
                //      Not sure if this is an issue
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        private FilterStates? _filterState;
        public FilterStates? FilterState {
            get => _filterState;
            set {
                if (_filterState == value) { return; }

                var hasMatchedDescendant = Children.Any(x => x.FilterState.In(Matched, DescendantMatched));

                if (value == NotMatched && hasMatchedDescendant) {
                    value = DescendantMatched;
                } else if (value == DescendantMatched && !hasMatchedDescendant) {
                    value = NotMatched;
                }
                if (_filterState == value) { return; }

                var old = _filterState;
                _filterState = value;

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

        public void ApplyFilter(Func<TData, bool> predicate) {
            var matched = predicate(Data);
            if (matched) {
                FilterState = Matched;
            } else {
                FilterState = NotMatched;
            }

            Children.ForEach(x => x.ApplyFilter(predicate));
        }

        /// <summary>Clears the filter state from this and all descendants</summary>
        public void ResetFilter() {
            FilterState = null;
            Children.ForEach(x => x.ResetFilter());
        }
    }
}
