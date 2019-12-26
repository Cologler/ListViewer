﻿namespace ListViewer.ConfiguresModel
{
    class Column
    {
        public string? ColumnName { get; set; }

        public string? ColumnField { get; set; }

        public bool SearchOn { get; set; } = true;

        public bool IsDisplay { get; set; } = true;

        public bool IsContextField { get; set; }
    }
}
