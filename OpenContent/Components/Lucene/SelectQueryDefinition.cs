using System;
using System.Collections.Generic;
using System.Linq;
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
        public SelectQueryDefinition()
        {
            Query = new MatchAllDocsQuery();
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
            Query = BuildFilter(select.Query);
            BuildSort(select);
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

        private static Query BuildFilter(FilterGroup filter)
        {
            BooleanQuery q = new BooleanQuery();
            q.Add(new MatchAllDocsQuery(), Occur.MUST);
            Occur cond = Occur.MUST; // AND
            if (filter.Condition == ConditionEnum.OR)
            {
                cond = Occur.SHOULD;
            }
            AddRules(q, filter.FilterRules, cond);
            foreach (var rule in filter.FilterGroups)
            {
                Occur groupCond = Occur.MUST; // AND
                if (rule.Condition == ConditionEnum.OR)
                {
                    groupCond = Occur.SHOULD;
                }
                BooleanQuery groupQ = new BooleanQuery();
                AddRules(groupQ, rule.FilterRules, groupCond);
                q.Add(groupQ, cond);
            }
            q = q.Clauses.Count > 0 ? q : null;
            return q;
        }

        private static void AddRules(BooleanQuery q, List<FilterRule> filterRules, Occur cond)
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
                    else if (rule.FieldType == FieldTypeEnum.FLOAT)
                    {
                        float fval = rule.Value.AsFloat;
                        q.Add(NumericRangeQuery.NewFloatRange(fieldName, fval, fval, true, true), cond);
                    }
                    else if (rule.FieldType == FieldTypeEnum.STRING || rule.FieldType == FieldTypeEnum.TEXT || rule.FieldType == FieldTypeEnum.HTML)
                    {
                        q.Add(ParseQuery(rule.Value.AsString + "*", fieldName), cond);
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
                        q.Add(ParseQuery(rule.Value.AsString + "*", fieldName), cond);
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
                }
                else if (rule.FieldOperator == OperatorEnum.GREATER_THEN_OR_EQUALS)
                {
                    DateTime startDate = rule.Value.AsDateTime;
                    DateTime endDate = DateTime.MaxValue;
                    q.Add(NumericRangeQuery.NewLongRange(fieldName, startDate.Ticks, endDate.Ticks, true, true), cond);
                }
                else if (rule.FieldOperator == OperatorEnum.LESS_THEN_OR_EQUALS)
                {
                    if (rule.FieldType == FieldTypeEnum.DATETIME)
                    {
                        DateTime startDate = DateTime.MinValue;
                        DateTime endDate = rule.Value.AsDateTime;
                        q.Add(NumericRangeQuery.NewLongRange(fieldName, startDate.Ticks, endDate.Ticks, true, true), cond);
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

        public static Query ParseQuery(string searchQuery, string defaultFieldName)
        {
            var parser = new QueryParser(Version.LUCENE_30, defaultFieldName, JsonMappingUtils.GetAnalyser());
            Query query;
            try
            {
                if (string.IsNullOrEmpty(searchQuery))
                {
                    query = new MatchAllDocsQuery();
                }
                else
                {
                    query = parser.Parse(searchQuery.Trim());
                }
            }
            catch (ParseException)
            {
                query = searchQuery != null ? parser.Parse(QueryParser.Escape(searchQuery.Trim())) : new MatchAllDocsQuery();
            }
            return query;
        }

        private SelectQueryDefinition BuildSort(Select select)
        {
            var sort = Sort.RELEVANCE;
            if (!select.Sort.Any())
            {
                SortRule sortOneCreateDate = new SortRule
                {
                    Field = "createdondate",
                    FieldType = FieldTypeEnum.DATETIME,
                    Descending = false
                };
                select.Sort.Add(sortOneCreateDate);
            }

            var sortFields = new List<SortField>();
            foreach (var rule in select.Sort)
            {
                int sortfieldtype;
                string sortFieldPrefix = "";
                Sortfieldtype(rule.FieldType, out sortfieldtype, ref sortFieldPrefix);

                if (rule.Field == "createdondate") rule.Field = "$createdondate";

                sortFields.Add(new SortField(sortFieldPrefix + rule.Field, sortfieldtype, rule.Descending));
            }
            sort = new Sort(sortFields.ToArray());

            Sort = sort;
            return this;
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