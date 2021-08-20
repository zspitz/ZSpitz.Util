// based on https://github.com/carbonrobot/wpf-autogrid/blob/master/source/WpfAutoGrid/AutoGrid.cs

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static ZSpitz.Util.Wpf.Functions;
using static System.Linq.Enumerable;
using static System.Windows.FrameworkPropertyMetadataOptions;

namespace ZSpitz.Util.Wpf {

    /// <summary>
    /// Defines a flexible grid area that consists of columns and rows.
    /// Depending on the orientation, either the rows or the columns are auto-generated,
    /// and the children's position is set according to their index.
    ///
    /// Partially based on work at http://rachel53461.wordpress.com/2011/09/17/wpf-grids-rowcolumn-count-properties/
    /// </summary>
    public class AutoGrid : Grid {
        /// <summary>
        /// Gets or sets the child horizontal alignment.
        /// </summary>
        /// <value>The child horizontal alignment.</value>
        [Category("Layout"), Description("Presets the horizontal alignment of all child controls")]
        public HorizontalAlignment? ChildHorizontalAlignment {
            get => (HorizontalAlignment?)GetValue(ChildHorizontalAlignmentProperty);
            set => SetValue(ChildHorizontalAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the child margin.
        /// </summary>
        /// <value>The child margin.</value>
        [Category("Layout"), Description("Presets the margin of all child controls")]
        public Thickness? ChildMargin {
            get => (Thickness?)GetValue(ChildMarginProperty);
            set => SetValue(ChildMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the child vertical alignment.
        /// </summary>
        /// <value>The child vertical alignment.</value>
        [Category("Layout"), Description("Presets the vertical alignment of all child controls")]
        public VerticalAlignment? ChildVerticalAlignment {
            get => (VerticalAlignment?)GetValue(ChildVerticalAlignmentProperty);
            set => SetValue(ChildVerticalAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the column count
        /// </summary>
        [Category("Layout"), Description("Defines a set number of columns")]
        public int ColumnCount {
            get => (int)GetValue(ColumnCountProperty);
            set => SetValue(ColumnCountProperty, value);
        }

        /// <summary>
        /// Gets or sets the columns
        /// </summary>
        [Category("Layout"), Description("Defines all columns using comma separated grid length notation")]
        public string Columns {
            get => (string)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        /// <summary>
        /// Gets or sets the fixed column width
        /// </summary>
        [Category("Layout"), Description("Presets the width of all columns set using the ColumnCount property")]
        public GridLength ColumnWidth {
            get => (GridLength)GetValue(ColumnWidthProperty);
            set => SetValue(ColumnWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the children are automatically indexed.
        /// <remarks>
        /// The default is <c>true</c>.
        /// Note that if children are already indexed, setting this property to <c>false</c> will not remove their indices.
        /// </remarks>
        /// </summary>
        [Category("Layout"), Description("Set to false to disable the auto layout functionality")]
        public bool IsAutoIndexing {
            get => (bool)GetValue(IsAutoIndexingProperty);
            set => SetValue(IsAutoIndexingProperty, value);
        }

        /// <summary>
        /// Gets or sets the orientation.
        /// <remarks>The default is Vertical.</remarks>
        /// </summary>
        /// <value>The orientation.</value>
        [Category("Layout"), Description("Defines the directionality of the autolayout. Use vertical for a column first layout, horizontal for a row first layout.")]
        public Orientation Orientation {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets the number of rows
        /// </summary>
        [Category("Layout"), Description("Defines a set number of rows")]
        public int RowCount {
            get => (int)GetValue(RowCountProperty);
            set => SetValue(RowCountProperty, value);
        }

        /// <summary>
        /// Gets or sets the fixed row height
        /// </summary>
        [Category("Layout"), Description("Presets the height of all rows set using the RowCount property")]
        public GridLength RowHeight {
            get => (GridLength)GetValue(RowHeightProperty);
            set => SetValue(RowHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the rows
        /// </summary>
        [Category("Layout"), Description("Defines all rows using comma separated grid length notation")]
        public string Rows {
            get => (string)GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        private GridLength defaultColumnWidth => ColumnDefinitions.FirstOrDefault()?.Width ?? GridLength.Auto;

        private void buildColumnDefinitions() {
            var lengths = Parse(Columns);
            var max = Math.Max(ColumnCount, lengths.Length);

            ColumnDefinitions.Clear();
            Range(0, max).Select(index => new ColumnDefinition {
                Width =
                    lengths.Length > index ? lengths[index] : defaultColumnWidth
            }).AddRangeTo(ColumnDefinitions);
        }

        /// <summary>
        /// Parse an array of grid lengths from comma delim text
        /// </summary>
        public static GridLength[] Parse(string text) => text.Split(',').Select(str => {
            double value;

            // ratio
            if (str.Contains('*')) {
                if (!double.TryParse(str.Replace("*", ""), out value)) {
                    value = 1.0;
                }
                return new GridLength(value, GridUnitType.Star);
            }

            // pixels
            if (double.TryParse(str, out value)) {
                return new GridLength(value);
            }

            // auto
            return GridLength.Auto;
        }).ToArray();

        // use an existing row definition height, or Auto
        private GridLength defaultRowHeight => RowDefinitions.FirstOrDefault()?.Height ?? GridLength.Auto;

        private void buildRowDefinitions() {
            var lengths = Parse(Rows);
            var max = Math.Max(RowCount, lengths.Length);

            RowDefinitions.Clear();
            Range(0, max).Select(index => new RowDefinition {
                Height =
                     lengths.Length > index ? lengths[index] : defaultRowHeight
            }).AddRangeTo(RowDefinitions);
        }

        private static void propogateToChildren<T>(AutoGrid grid, T? currentValue, DependencyProperty dp) where T : struct {
            foreach (var child in grid.Children.Cast<UIElement>()) {
                child.SetValue(dp, currentValue ?? DependencyProperty.UnsetValue);
            }
        }

        /// <summary>
        /// Apply child margins and layout effects such as alignment
        /// </summary>
        private void applyChildLayout(UIElement child) {
            if (ChildMargin != null) {
                child.SetIfDefault(MarginProperty, ChildMargin.Value);
            }
            if (ChildHorizontalAlignment != null) {
                child.SetIfDefault(HorizontalAlignmentProperty, ChildHorizontalAlignment.Value);
            }
            if (ChildVerticalAlignment != null) {
                child.SetIfDefault(VerticalAlignmentProperty, ChildVerticalAlignment.Value);
            }
        }

        /// <summary>
        /// Perform the grid layout of row and column indexes
        /// </summary>
        private void performLayout() {
            var fillRowFirst = Orientation == Orientation.Horizontal;
            buildRowDefinitions();
            buildColumnDefinitions();

            var rowCount = RowDefinitions.Count;
            var colCount = ColumnDefinitions.Count;

            if (rowCount == 0 || colCount == 0) { return; }

            var position = 0;
            var skip = new HashSet<(int row, int col)>();
            foreach (var child in Children.Cast<UIElement>()) {
                var childIsCollapsed = child.Visibility == Visibility.Collapsed;
                if (IsAutoIndexing && !childIsCollapsed) {
                    if (fillRowFirst) {
                        var row = position / colCount;
                        var col = position % colCount;
                        while (skip.Contains((row, col))) {
                            position++;
                            row = position / colCount;
                            col = position % colCount;
                        }

                        if (row >= rowCount) {
                            RowDefinitions.Add(new RowDefinition { Height = defaultRowHeight });
                            rowCount = RowDefinitions.Count;
                        }

                        SetRow(child, row);
                        SetColumn(child, col);

                        var skipCols = Range(col, GetColumnSpan(child)).ToArray();
                        var skipRows = Range(row, GetRowSpan(child)).ToArray();
                        skipCols.SelectMany(col1 => skipRows.Select(row1 => (row1, col1))).Where(x => x != (row, col)).AddRangeTo(skip);

                        position += 1;
                    } else {
                        var row = position % rowCount;
                        var col = position / rowCount;
                        while (skip.Contains((row, col))) {
                            position++;
                            row = position % rowCount;
                            col = position / rowCount;
                        }

                        if (col >= colCount) {
                            ColumnDefinitions.Add(new ColumnDefinition { Width = defaultColumnWidth });
                            colCount = ColumnDefinitions.Count;
                        }

                        SetRow(child, row);
                        SetColumn(child, col);

                        var skipCols = Range(col, GetColumnSpan(child)).ToArray();
                        var skipRows = Range(row, GetRowSpan(child)).ToArray();
                        skipCols.SelectMany(col1 => skipRows.Select(row1 => (row1, col1))).Where(x => x != (row, col)).AddRangeTo(skip);

                        position += 1;
                    }
                }

                applyChildLayout(child);
            }
        }

        public static readonly DependencyProperty ChildHorizontalAlignmentProperty =
            DPRegister<HorizontalAlignment?, AutoGrid>(null, AffectsMeasure,
                (grid, _, _) => propogateToChildren(grid, grid.ChildHorizontalAlignment, HorizontalAlignmentProperty)
            );

        public static readonly DependencyProperty ChildMarginProperty =
            DPRegister<Thickness?, AutoGrid>(null, AffectsMeasure,
                (grid, _, _) => propogateToChildren(grid, grid.ChildMargin, MarginProperty)
            );

        public static readonly DependencyProperty ChildVerticalAlignmentProperty =
            DPRegister<VerticalAlignment?, AutoGrid>(null, AffectsMeasure,
                (grid, _, _) => propogateToChildren(grid, grid.ChildVerticalAlignment, VerticalAlignmentProperty)
            );

        public static readonly DependencyProperty ColumnCountProperty =
            DPRegisterAttached<int, AutoGrid>(0, AffectsMeasure, (grid, @new, _) => {
                if (@new < 0) { return; }
                grid.buildColumnDefinitions();
            });

        public static readonly DependencyProperty ColumnsProperty =
            DPRegisterAttached<string, AutoGrid>("", AffectsMeasure, (grid, @new, _) => {
                if (@new == "") { return; }
                grid.buildColumnDefinitions();
            });

        public static readonly DependencyProperty ColumnWidthProperty =
            DPRegisterAttached<GridLength, AutoGrid>(GridLength.Auto, AffectsMeasure, (grid, @new, _) => {
                // add a default column if missing
                if (grid.ColumnDefinitions.Count == 0) {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                // set all existing columns to this width
                for (var i = 0; i < grid.ColumnDefinitions.Count; i++) {
                    grid.ColumnDefinitions[i].Width = @new;
                }
            });

        public static readonly DependencyProperty IsAutoIndexingProperty =
            DPRegister<bool, AutoGrid>(true, AffectsMeasure);

        public static readonly DependencyProperty OrientationProperty =
            DPRegister<Orientation, AutoGrid>(Orientation.Horizontal, AffectsMeasure);

        public static readonly DependencyProperty RowCountProperty =
            DPRegisterAttached<int, AutoGrid>(1, AffectsMeasure, (grid, @new, _) => {
                if (@new < 0) { return; }
                grid.buildRowDefinitions();
            });

        public static readonly DependencyProperty RowHeightProperty =
            DPRegisterAttached<GridLength, AutoGrid>(GridLength.Auto, AffectsMeasure, (grid, @new, old) => {
                // add a default row if missing
                if (grid.RowDefinitions.Count == 0) {
                    grid.RowDefinitions.Add(new RowDefinition());
                }

                // set all existing rows to this height
                for (var i = 0; i < grid.RowDefinitions.Count; i++) {
                    grid.RowDefinitions[i].Height = @new;
                }
            });

        public static readonly DependencyProperty RowsProperty =
            DPRegisterAttached<string, AutoGrid>("", AffectsMeasure, (grid, @new, old) => {
                if (@new == "") { return; }
                grid.buildRowDefinitions();
            });

        #region Overrides

        /// <summary>
        /// Measures the children of a <see cref="T:System.Windows.Controls.Grid"/> in anticipation of arranging them during the <see cref="M:ArrangeOverride"/> pass.
        /// </summary>
        /// <param name="constraint">Indicates an upper limit size that should not be exceeded.</param>
        /// <returns>
        /// 	<see cref="Size"/> that represents the required size to arrange child content.
        /// </returns>
        protected override Size MeasureOverride(Size constraint) {
            performLayout();
            return base.MeasureOverride(constraint);
        }

        #endregion Overrides
    }
}
