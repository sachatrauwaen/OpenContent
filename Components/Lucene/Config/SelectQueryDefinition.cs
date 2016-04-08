using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;
using Lucene.Net.Index;
using Satrabel.OpenContent.Components.Datasource.search;

namespace Satrabel.OpenContent.Components.Lucene.Config
{
    public class SelectQueryDefinition
    {
        private bool DefaultNoResults;
        private Query _Query;

        public SelectQueryDefinition()
        {

            Query = new MatchAllDocsQuery();
            Sort = Sort.RELEVANCE;
            PageSize = 100;
        }
        public Query Filter { get; set; }
        public Sort Sort { get; set; }
        public Query Query
        {
            get
            {
                return _Query;
            }
            set
            {
                if (DefaultNoResults && value != null && (new MatchAllDocsQuery()).ToString() == value.ToString())
                {
                    _Query = new BooleanQuery();
                }
                else
                {
                    _Query = value;
                }
            }
        }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public SelectQueryDefinition Build(Select select)
        {
            BuildPage(select);
            Filter = BuildFilter(select.Filter);
            Query = BuildFilter(select.Query);
            BuildSort(select);
            return this;
        }
        public SelectQueryDefinition BuildPage(Select select)
        {
            PageSize = select.PageSize;
            PageIndex = select.PageIndex;
            // ????? DefaultNoResults = ;
            return this;
        }
        public Query BuildFilter(FilterGroup filter)
        {
            BooleanQuery q = new BooleanQuery();
            q.Add(new MatchAllDocsQuery(), Occur.MUST);

            Occur cond = Occur.MUST; // AND
            if (filter.Condition == ConditionEnum.OR)
            {
                cond = Occur.SHOULD;
            }
            foreach (var rule in filter.FilterRules)
            {
                string fieldName = rule.Field;
                if (fieldName == "id") fieldName = "$id";

                if (rule.FieldOperator == OperatorEnum.EQUAL)
                {
                    if (rule.FieldType == FieldTypeEnum.BOOLEAN)
                    {
                        int ival = rule.Value.AsBoolean ? 1 : 0;
                        q.Add(NumericRangeQuery.NewIntRange(fieldName, ival, ival, true, true), cond);
                    }
                    else
                    {
                        q.Add(new TermQuery(new Term(fieldName, rule.Value.AsString)), cond);
                    }
                }
                else if (rule.FieldOperator == OperatorEnum.NOT_EQUAL)
                {
                    q.Add(new TermQuery(new Term(fieldName, rule.Value.AsString)), Occur.MUST_NOT);
                }
                else if (rule.FieldOperator == OperatorEnum.START_WITH)
                {
                    q.Add(new WildcardQuery(new Term(fieldName, rule.Value.AsString)), cond);
                }
                else if (rule.FieldOperator == OperatorEnum.IN)
                {
                    BooleanQuery arrQ = new BooleanQuery();
                    foreach (var arrItem in rule.MultiValue)
                    {
                        arrQ.Add(new TermQuery(new Term(fieldName, arrItem.AsString)), Occur.SHOULD); // OR
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
            q = q.Clauses.Count > 0 ? q : null;
            return q;
        }
        public SelectQueryDefinition BuildSort(Select select)
        {
            var sort = Sort.RELEVANCE;
            if (select.Sort.Any())
            {
                var sortFields = new List<SortField>();
                foreach (var rule in select.Sort)
                {
                    int sortfieldtype = SortField.STRING;
                    string sortFieldPrefix = "";
                    Sortfieldtype(rule.FieldType, ref sortfieldtype, ref sortFieldPrefix);
                    sortFields.Add(new SortField(sortFieldPrefix + rule.Field, sortfieldtype, rule.Descending));
                }
                Sort = new Sort(sortFields.ToArray());
            }
            Sort = sort;
            return this;
        }
        private void Sortfieldtype(FieldTypeEnum fieldType, ref int sortfieldtype, ref string sortFieldPrefix)
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
                sortfieldtype = SortField.LONG;
            }
            else if (fieldType == FieldTypeEnum.LONG)
            {
                sortfieldtype = SortField.LONG;
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
    }
}