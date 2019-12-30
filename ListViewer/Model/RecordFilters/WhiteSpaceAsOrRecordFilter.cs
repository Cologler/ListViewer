﻿using System;
using System.Linq;
using ListViewer.Abstractions;

namespace ListViewer.Model.RecordFilters
{
    class WhiteSpaceAsOrRecordFilter : IRecordFilter
    {
        private readonly string[] _searchTexts;

        public WhiteSpaceAsOrRecordFilter(string searchText)
        {
            this._searchTexts = searchText.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        }

        public bool IsMatch(IRecordSearchFieldValuesReader reader)
        {
            foreach (var value in reader.GetSearchFieldValues())
            {
                foreach (var word in this._searchTexts)
                {
                    if (value.Contains(word, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
