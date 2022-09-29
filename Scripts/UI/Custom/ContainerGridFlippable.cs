using Godot;
using System;
using System.Collections.Generic;

public class ContainerGridFlippable:GridContainer {
    public override void _Notification(int p_what) {
        switch(p_what) {
            case NotificationSortChildren:
                Dictionary<int, int> col_minw = new Dictionary<int, int>(); // Max of min_width of all controls in each col (indexed by col).
                Dictionary<int, int> row_minh = new Dictionary<int, int>(); // Max of min_height of all controls in each row (indexed by row).
                HashSet<int> col_expanded = new HashSet<int>(); // Columns which have the SIZE_EXPAND flag set.
                HashSet<int> row_expanded = new HashSet<int>(); // Rows which have the SIZE_EXPAND flag set.

                int hsep = GetConstant("hseparation");
                int vsep = GetConstant("vseparation");
                int max_col = Mathf.Min(GetChildCount(), Columns);
                int max_row = (int)Mathf.Ceil((float)GetChildCount() / (float)Columns);

                // Compute the per-column/per-row data.
                int valid_controls_index = 0;
                for(int i = 0; i < GetChildCount(); i++) {
                    Control c = GetChild(i) as Control;
                    if(c == null || !c.IsVisibleInTree()) {
                        continue;
                    }

                    int row = valid_controls_index / Columns;
                    int col = valid_controls_index % Columns;
                    valid_controls_index++;

                    Vector2 ms = c.GetCombinedMinimumSize();
                    if(col_minw.ContainsKey(col)) {
                        col_minw[col] = (int)Mathf.Max(col_minw[col], ms.x);
                    } else {
                        col_minw[col] = (int)ms.x;
                    }
                    if(row_minh.ContainsKey(row)) {
                        row_minh[row] = Mathf.Max(row_minh[row], (int)ms.y);
                    } else {
                        row_minh[row] = (int)ms.y;
                    }

                    if((c.SizeFlagsHorizontal & (int)SizeFlags.Expand) > 0) {
                        col_expanded.Add(col);
                    }
                    if((c.SizeFlagsVertical & (int)SizeFlags.Expand) > 0) {
                        row_expanded.Add(row);
                    }
                }

                // Consider all empty Columns expanded.
                for(int i = valid_controls_index; i < Columns; i++) {
                    col_expanded.Add(i);
                }

                // Evaluate the remaining space for expanded Columns/rows.
                Vector2 remaining_space = RectSize;
                foreach(KeyValuePair<int, int> E in col_minw) {
                    if(!col_expanded.Contains(E.Key)) {
                        remaining_space.x -= E.Value;
                    }
                }
                foreach(KeyValuePair<int, int> E in row_minh) {
                    if(!row_expanded.Contains(E.Key)) {
                        remaining_space.y -= E.Value;
                    }
                }
                remaining_space.x -= vsep * Mathf.Max(max_row - 1, 0);
                remaining_space.y -= hsep * Mathf.Max(max_col - 1, 0);

                bool can_fit = false;
                while(!can_fit && col_expanded.Count > 0) {
                    // Check if all minwidth constraints are OK if we use the remaining space.
                    can_fit = true;
                    int max_index = col_expanded.GetEnumerator().Current;
                    foreach(int E in col_expanded) {
                        if(col_minw[E] > col_minw[max_index]) {
                            max_index = E;
                        }
                        if(can_fit && (remaining_space.x / col_expanded.Count) < col_minw[E]) {
                            can_fit = false;
                        }
                    }

                    // If not, the column with maximum minwidth is not expanded.
                    if(!can_fit) {
                        col_expanded.Remove(max_index);
                        remaining_space.x -= col_minw[max_index];
                    }
                }

                can_fit = false;
                while(!can_fit && row_expanded.Count > 0) {
                    // Check if all minheight constraints are OK if we use the remaining space.
                    can_fit = true;
                    int max_index = row_expanded.GetEnumerator().Current;
                    foreach(int E in row_expanded) {
                        if(row_minh[E] > row_minh[max_index]) {
                            max_index = E;
                        }
                        if(can_fit && (remaining_space.x / row_expanded.Count) < row_minh[E]) {
                            can_fit = false;
                        }
                    }

                    // If not, the row with maximum minheight is not expanded.
                    if(!can_fit) {
                        row_expanded.Remove(max_index);
                        remaining_space.x -= row_minh[max_index];
                    }
                }

                // Finally, fit the nodes.
                int col_expand = col_expanded.Count > 0 ? (int)remaining_space.x / col_expanded.Count : 0;
                int row_expand = row_expanded.Count > 0 ? (int)remaining_space.y / row_expanded.Count : 0;

                int col_ofs = 0;
                int row_ofs = 0;

                valid_controls_index = 0;
                for(int i = 0; i < GetChildCount(); i++) {
                    Control c = GetChild(i) as Control;
                    if(c == null || !c.IsVisibleInTree()) {
                        continue;
                    }
                    int row = valid_controls_index / Columns;
                    int col = valid_controls_index % Columns;
                    valid_controls_index++;

                    if(col == 0) {
                        col_ofs = 0;
                        if(row > 0) {
                            row_ofs += (row_expanded.Contains(row - 1) ? row_expand : row_minh[row - 1]) + vsep;
                        }
                    }

                    Vector2 p = new Vector2(col_ofs, row_ofs);
                    Vector2 s = new Vector2(col_expanded.Contains(col) ? col_expand : col_minw[col], row_expanded.Contains(row) ? row_expand : row_minh[row]);

                    FitChildInRect(c, new Rect2(p, s));

                    col_ofs += (int)s.x + hsep;
                }
                break;
            case NotificationThemeChanged:
                MinimumSizeChanged();
                break;
        }
    }
}