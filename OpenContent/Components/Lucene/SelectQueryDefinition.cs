using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Satrabel.OpenContent.Components.Datasource.Search;
using Satrabel.OpenContent.Components.Lucene.Mapping;
using Version = Lucene.Net.Util.Version;

namespace Satrabel.OpenContent.Components.Lucene
{
    public class SelectQueryDefinition
    {
        private static readonly Query _DefaultQuery = new MatchAllDocsQuery();

        public SelectQueryDefinition()
        {
            Query = _DefaultQuery;
            Sort = Sort.RELEVANCE;
            PageSize = 100;
        }
        public Query Filter { get; private set; }
        public Query Query { get; private set; }
        public Sort Sort { get; private set; }
        public int PageSize { get; private set; }
        public int PageIndex { get; private set; }

        public SelectQueryDefinition Build(Select select)
        {
            BuildPage(select);
            Filter = BuildFilter(select.Filter);
            Query = BuildFilter(select.Query, "");
            Sort = BuildSort(select);
            return this;
        }

        public SelectQueryDefinition Build(Select select, string cultureCode)
        {
            BuildPage(select);
            Filter = BuildFilter(select.Filter);
            Query = BuildFilter(select.Query, cultureCode);
            Sort = BuildSort(select);
            return this;
        }

        #region private methods

        private SelectQueryDefinition BuildPage(Select select)
        {
            PageSize = select.PageSize == 0 ? 100 : select.PageSize;
            PageIndex = select.PageIndex;
            // ????? DefaultNoResults = ;
            return this;
        }

        private static Query BuildFilter(FilterGroup filter, string cultureCode = "")
        {
            BooleanQuery q = new BooleanQuery();
            //if (filter.FilterRules.Count == 0 && filter.FilterGroups.Count == 0)
            //{
            //    q.Add(new MatchAllDocsQuery(), Occur.MUST);
            //}

            if (filter.FilterRules.Count > 0 && filter.FilterRules.All(r => r.FieldOperator == OperatorEnum.NOT_EQUAL))
            {
                q.Add(new MatchAllDocsQuery(), Occur.MUST);
            }

            Occur cond = Occur.MUST; // AND
            if (filter.Condition == ConditionEnum.OR)
            {
                cond = Occur.SHOULD;
            }
            AddRules(q, filter.FilterRules, cond, cultureCode);
            foreach (var rule in filter.FilterGroups)
            {
                Occur groupCond = Occur.MUST; // AND
                if (rule.Condition == ConditionEnum.OR)
                {
                    groupCond = Occur.SHOULD;
                }
                BooleanQuery groupQ = new BooleanQuery();
                AddRules(groupQ, rule.FilterRules, groupCond, cultureCode);
                q.Add(groupQ, cond);
            }
            q = q.Clauses.Count > 0 ? q : null;
            return q;
        }

        private static void AddRules(BooleanQuery q, List<FilterRule> filterRules, Occur cond, string cultureCode = "")
        {
            foreach (var rule in filterRules)
            {
                string fieldName = rule.Field;
                if (fieldName == "id") fieldName = "$id";
                if (fieldName == "userid") fieldName = "$userid";

                if (rule.FieldOperator == OperatorEnum.EQUAL)
                {
                    if (rule.FieldType == FieldTypeEnum.BOOLEAN)
                    {
                        int ival = rule.Value.AsBoolean ? 1 : 0;
                        q.Add(NumericRangeQuery.NewIntRange(fieldName, ival, ival, true, true), cond);
                    }
                    else if (rule.FieldType == FieldTypeEnum.DATETIME)
                    {
                        var startDate = rule.Value.AsDateTime;
                        var endDate = rule.Value.AsDateTime;
                        q.Add(NumericRangeQuery.NewLongRange(fieldName, startDate.Ticks, endDate.Ticks, true, true), cond);
                    }
                    else if (rule.FieldType == FieldTypeEnum.FLOAT)
                    {
                        float fval = rule.Value.AsFloat;
                        q.Add(NumericRangeQuery.NewFloatRange(fieldName, fval, fval, true, true), cond);
                    }
                    else if (rule.FieldType == FieldTypeEnum.STRING || rule.FieldType == FieldTypeEnum.TEXT || rule.FieldType == FieldTypeEnum.HTML)
                    {
                        q.Add(ParseQuery(rule.Value.AsString + "*", fieldName, cultureCode), cond);
                    }
                    else
                    {
                        string searchstring = QueryParser.Escape(rule.Value.AsString);
                        q.Add(new TermQuery(new Term(fieldName, searchstring)), cond);
                    }
                }
                else if (rule.FieldOperator == OperatorEnum.NOT_EQUAL)
                {
                    q.Add(new TermQuery(new Term(fieldName, QueryParser.Escape(rule.Value.AsString))), Occur.MUST_NOT);
                }
                else if (rule.FieldOperator == OperatorEnum.START_WITH)
                {
                    if (rule.FieldType == FieldTypeEnum.STRING || rule.FieldType == FieldTypeEnum.TEXT || rule.FieldType == FieldTypeEnum.HTML)
                    {
                        q.Add(ParseQuery(rule.Value.AsString + "*", fieldName, cultureCode), cond);
                    }
                    else
                    {
                        q.Add(new WildcardQuery(new Term(fieldName, QueryParser.Escape(rule.Value.AsString) + "*")), cond);
                    }
                }
                else if (rule.FieldOperator == OperatorEnum.IN)
                {

                    BooleanQuery arrQ = new BooleanQuery();
                    foreach (var arrItem in rule.MultiValue)
                    {
                        arrQ.Add(new TermQuery(new Term(fieldName, QueryParser.Escape(arrItem.AsString))), Occur.SHOULD); // OR                        
                        /*
                        var phraseQ = new PhraseQuery();
                        phraseQ.Add(new Term(fieldName, arrItem.AsString));
                        arrQ.Add(phraseQ, Occur.SHOULD); // OR                        
                         */
                    }
                    q.Add(arrQ, cond);
                }
                else if (rule.FieldOperator == OperatorEnum.BETWEEN)
                {
                    if (rule.FieldType == FieldTypeEnum.DATETIME)
                    {
                        var startDate = rule.LowerValue.AsDateTime;
                        var endDate = rule.UpperValue.AsDateTime;
                        q.Add(NumericRangeQuery.NewLongRange(fieldName, startDate.Ticks, endDate.Ticks, true, true), cond);
                    }
                    else if (rule.FieldType == FieldTypeEnum.FLOAT)
                    {
                        var startValue = rule.LowerValue.AsFloat;
                        var endValue = rule.UpperValue.AsFloat;
                        q.Add(NumericRangeQuery.NewFloatRange(fieldName, startValue, endValue, true, true), cond);
                    }
                }
                else if (rule.FieldOperator == OperatorEnum.GREATER_THEN_OR_EQUALS)
                {
                    if (rule.FieldType == FieldTypeEnum.DATETIME)
                    {
                        DateTime startDate = rule.Value.AsDateTime;
                        DateTime endDate = DateTime.MaxValue;
                        q.Add(NumericRangeQuery.NewLongRange(fieldName, startDate.Ticks, endDate.Ticks, true, true), cond);
                    }
                    else if (rule.FieldType == FieldTypeEnum.FLOAT)
                    {
                        var startValue = rule.Value.AsFloat;
                        var endValue = float.MaxValue;
                        q.Add(NumericRangeQuery.NewFloatRange(fieldName, startValue, endValue, true, true), cond);
                    }
                }
                else if (rule.FieldOperator == OperatorEnum.LESS_THEN_OR_EQUALS)
                {
                    if (rule.FieldType == FieldTypeEnum.DATETIME)
                    {
                        DateTime startDate = DateTime.MinValue;
                        DateTime endDate = rule.Value.AsDateTime;
                        q.Add(NumericRangeQuery.NewLongRange(fieldName, startDate.Ticks, endDate.Ticks, true, true), cond);
                    }
                    else if (rule.FieldType == FieldTypeEnum.FLOAT)
                    {
                        var startValue = float.MinValue;
                        var endValue = rule.Value.AsFloat;
                        q.Add(NumericRangeQuery.NewFloatRange(fieldName, startValue, endValue, true, true), cond);
                    }
                }
                else if (rule.FieldOperator == OperatorEnum.GREATER_THEN)
                {
                }
                else if (rule.FieldOperator == OperatorEnum.LESS_THEN)
                {
                }
            }
        }

        public static Query ParseQuery(string searchQuery, string defaultFieldName, string CultureCode = "")
        {
            searchQuery = RemoveDiacritics(searchQuery);
            var parser = new QueryParser(Version.LUCENE_30, defaultFieldName, JsonMappingUtils.GetAnalyser(CultureCode));
            Query query;
            try
            {
                if (string.IsNullOrEmpty(searchQuery))
                {
                    query = _DefaultQuery;
                }
                else
                {
                    query = parser.Parse(searchQuery.Trim());
                }
            }
            catch (ParseException)
            {
                query = searchQuery != null ? parser.Parse(QueryParser.Escape(searchQuery.Trim())) : _DefaultQuery;
            }
            return query;
        }

        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC);
        }

        private static Sort BuildSort(Select select)
        {
            if (!select.Sort.Any())
            {
                var sortOnCreateDate = new SortRule
                {
                    Field = "createdondate",
                    FieldType = FieldTypeEnum.DATETIME,
                    Descending = false
                };
                select.Sort.Add(sortOnCreateDate);
            }

            var sortFields = new List<SortField>();
            sortFields.Add(SortField.FIELD_SCORE);

            foreach (var rule in select.Sort)
            {
                int sortfieldtype;
                string sortFieldPrefix = "";
                Sortfieldtype(rule.FieldType, out sortfieldtype, ref sortFieldPrefix);

                if (rule.Field == "createdondate") rule.Field = "$createdondate";

                sortFields.Add(new SortField(sortFieldPrefix + rule.Field, sortfieldtype, rule.Descending));
            }
            var sort = new Sort(sortFields.ToArray());

            return sort;
        }

        private static void Sortfieldtype(FieldTypeEnum fieldType, out int sortfieldtype, ref string sortFieldPrefix)
        {
            if (fieldType == FieldTypeEnum.DATETIME)
            {
                sortfieldtype = SortField.LONG;
            }
            else if (fieldType == FieldTypeEnum.BOOLEAN)
            {
                sortfieldtype = SortField.INT;
            }
            else if (fieldType == FieldTypeEnum.INTEGER)
            {
                sortfieldtype = SortField.FLOAT;
            }
            else if (fieldType == FieldTypeEnum.LONG)
            {
                sortfieldtype = SortField.FLOAT;
            }
            else if (fieldType == FieldTypeEnum.FLOAT) // or double
            {
                sortfieldtype = SortField.FLOAT;
            }
            else if (fieldType == FieldTypeEnum.KEY)
            {
                sortfieldtype = SortField.STRING;
            }
            else if (fieldType == FieldTypeEnum.TEXT || fieldType == FieldTypeEnum.HTML)
            {
                sortfieldtype = SortField.STRING;
                sortFieldPrefix = "@";
            }
            else
            {
                sortfieldtype = SortField.STRING;
                sortFieldPrefix = "@";
            }
        }
        #endregion

    }
}