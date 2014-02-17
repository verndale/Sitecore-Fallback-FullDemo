using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Search;
using scSearchContrib.Searcher;
using scSearchContrib.Searcher.Parameters;

namespace Verndale.SharedSource.Helpers
{
    public class SearchHelper
    {
        public static string FormatKeywordWithSpacesForSearch(string searchString)
        {
            var formattedSearchString = searchString.Contains(" ") &&
                                       (!searchString.StartsWith("\"") && !searchString.EndsWith("\""))
                ? "\"" + searchString + "\""
                : searchString;

            return formattedSearchString;
        }

        public static List<SkinnyItem> GetItems(string indexName,
                                            string language,
                                            string templateFilter,
                                            string locationFilter,
                                            string fullTextQuery,
                                            bool performSort = false,
                                            string sortFieldName = "",
                                            bool reverse = false)
        {
            var searchParam = new SearchParam
            {
                Database = "web",
                Language = language,
                TemplateIds = templateFilter,
                LocationIds = locationFilter,
                FullTextQuery = fullTextQuery
            };

            using (var runner = new QueryRunner(indexName))
            {
                if (performSort)
                    return runner.GetItems(searchParam, sortField: sortFieldName, reverse: reverse);
                else
                    return runner.GetItems(searchParam);
            }
        }


        public static List<SkinnyItem> GetItems(string indexName,
                                            string language,
                                            string templateFilter,
                                            string locationFilter,
                                            string fullTextQuery,
                                            List<DateRangeSearchParam.DateRange> ranges,
                                            QueryOccurance occuranceTypeDateRanges = QueryOccurance.Must,
                                            bool performSort = false,
                                            string sortFieldName = "",
                                            bool reverse = false)
        {
            var searchParam = new DateRangeSearchParam
            {
                Database = "web",
                Ranges = ranges,
                LocationIds = locationFilter,
                TemplateIds = templateFilter,
                FullTextQuery = fullTextQuery,
                InnerCondition = occuranceTypeDateRanges,
                Language = language
            };

            using (var runner = new QueryRunner(indexName))
            {
                if (performSort)
                    return runner.GetItems(searchParam, sortField: sortFieldName, reverse: reverse);
                else
                    return runner.GetItems(searchParam);
            }
        }

        public static List<SkinnyItem> GetItems(string indexName,
                                            string language,
                                            string templateFilter,
                                            string locationFilter,
                                            string fullTextQuery,
                                            List<NumericRangeSearchParam.NumericRangeField> ranges,
                                            QueryOccurance occuranceTypeNumRanges = QueryOccurance.Must,
                                            bool performSort = false,
                                            string sortFieldName = "",
                                            bool reverse = false)
        {
            var searchParam = new NumericRangeSearchParam
            {
                Database = "web",
                Ranges = ranges,
                LocationIds = locationFilter,
                TemplateIds = templateFilter,
                FullTextQuery = fullTextQuery,
                InnerCondition = occuranceTypeNumRanges,
                Language = language
            };

            using (var runner = new QueryRunner(indexName))
            {
                if (performSort)
                    return runner.GetItems(searchParam, sortField: sortFieldName, reverse: reverse);
                else
                    return runner.GetItems(searchParam);
            }
        }

        public static List<SkinnyItem> GetItems(string indexName,
                                                string language,
                                                string templateFilter,
                                                string locationFilter,
                                                string fullTextQuery,
                                                List<MultiFieldSearchParam.Refinement> refinements,
                                                QueryOccurance occuranceTypeRefinements = QueryOccurance.Must,
                                                bool performSort = false,
                                                string sortFieldName = "",
                                                bool reverse = false)
        {

            var searchParam = new MultiFieldSearchParam
            {
                Database = "web",
                Refinements = refinements,
                InnerCondition = occuranceTypeRefinements,
                LocationIds = locationFilter,
                TemplateIds = templateFilter,
                FullTextQuery = fullTextQuery,
                Language = language
            };

            using (var runner = new QueryRunner(indexName))
            {
                if (performSort)
                    return runner.GetItems(searchParam, sortField: sortFieldName, reverse: reverse);
                else
                    return runner.GetItems(searchParam);
            }
        }

        public static List<SkinnyItem> GetItems(string indexName,
                                                string language,
                                                string templateFilter,
                                                string locationFilter,
                                                string fullTextQuery,
                                                string relationFilter,
                                                bool performSort = false,
                                                string sortFieldName = "",
                                                bool reverse = false)
        {

            var searchParam = new SearchParam
            {
                Database = "web",
                RelatedIds = relationFilter,
                LocationIds = locationFilter,
                TemplateIds = templateFilter,
                FullTextQuery = fullTextQuery,
                Language = language
            };

            using (var runner = new QueryRunner(indexName))
            {
                if (performSort)
                    return runner.GetItems(searchParam, sortField: sortFieldName, reverse: reverse);
                else
                    return runner.GetItems(searchParam);
            }
        }

        public static List<SkinnyItem> GetItems(string indexName,
                                              string language,
                                              string templateFilter,
                                              string locationFilter,
                                              string fullTextQuery,
                                               string fieldName,
                                                string fieldValue,
                                                bool canBePartial,
                                                bool performSort = false,
                                                string sortFieldName = "",
                                                bool reverse = false)
        {
            var searchParam = new FieldSearchParam
            {
                Database = "web",
                Language = language,
                FieldName = fieldName,
                FieldValue = fieldValue,
                TemplateIds = templateFilter,
                LocationIds = locationFilter,
                FullTextQuery = fullTextQuery,
                Partial = canBePartial
            };

            using (var runner = new QueryRunner(indexName))
            {
                if (performSort)
                    return runner.GetItems(searchParam, sortField: sortFieldName, reverse: reverse);
                else
                    return runner.GetItems(searchParam);
            }
        }

        public static List<SkinnyItem> GetItems(string indexName,
                                                string language,
                                                string templateFilter,
                                                string locationFilter,
                                                string fullTextQuery,
                                                string relationFilter,
                                                List<MultiFieldSearchParam.Refinement> refinements,
                                                QueryOccurance occuranceTypeRefinements,
                                                List<DateRangeSearchParam.DateRange> dateRanges,
                                                QueryOccurance occuranceTypeDateRanges,
                                                List<NumericRangeSearchParam.NumericRangeField> numRanges,
                                                QueryOccurance occuranceTypeNumRanges,
                                                string fieldName,
                                                string fieldValue,
                                                bool canBePartial,
                                                bool performSort = false,
                                                string sortFieldName = "",
                                                bool reverse = false)
        {

            var searchParam = new SearchParam
            {
                Database = "web",
                RelatedIds = relationFilter,
                Language = language,
                TemplateIds = templateFilter,
                LocationIds = locationFilter,
                FullTextQuery = fullTextQuery
            };

            var searchParamMultiField = new MultiFieldSearchParam
            {
                Refinements = refinements,
                InnerCondition = occuranceTypeRefinements
            };

            var searchParamDate = new DateRangeSearchParam
            {
                Ranges = dateRanges,
                InnerCondition = occuranceTypeDateRanges
            };

            var searchParamNum = new NumericRangeSearchParam
            {
                Ranges = numRanges,
                InnerCondition = occuranceTypeNumRanges
            };

            var searchParamField = new FieldSearchParam
            {
                FieldName = fieldName,
                FieldValue = fieldValue,
                Partial = canBePartial
            };

            using (var runner = new QueryRunner(indexName))
            {
                if (performSort)
                    return runner.GetItems(new[] { searchParam, searchParamMultiField, searchParamDate, searchParamField, searchParamNum }, sortField: sortFieldName, reverse: reverse);
                else
                    return runner.GetItems(new[] { searchParam, searchParamMultiField, searchParamDate, searchParamField, searchParamNum });
            }
        }

    }
}
