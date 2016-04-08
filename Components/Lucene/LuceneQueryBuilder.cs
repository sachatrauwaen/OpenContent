using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Satrabel.OpenContent.Components.JPList;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components.Lucene
{
    public static class LuceneQueryBuilder
    {

        public static Query BuildLuceneQuery(JpListQueryDTO jpListQuery, FieldConfig indexConfig)
        {
            if (jpListQuery.Filters.Any())
            {
                BooleanQuery query = new BooleanQuery();
                foreach (FilterDTO f in jpListQuery.Filters)
                {
                    if (f.ExactSearchMultiValue != null && f.ExactSearchMultiValue.Any()) //group is bv multicheckbox, vb categories where(categy="" OR category="")
                    {
                        var groupQuery = new BooleanQuery();
                        foreach (var p in f.ExactSearchMultiValue)
                        {
                            var termQuery = new TermQuery(new Term(f.Name, p));
                            groupQuery.Add(termQuery, Occur.SHOULD); // or
                        }
                        query.Add(groupQuery, Occur.MUST); //and
                    }
                    else
                    {
                        var names = f.Names;
                        var groupQuery = new BooleanQuery();
                        foreach (var n in names)
                        {

                            if (!string.IsNullOrEmpty(f.ExactSearchValue))
                            {
                                //for dropdownlists; value is keyword => never partial search
                                var termQuery = new TermQuery(new Term(n, f.ExactSearchValue));
                                groupQuery.Add(termQuery, Occur.SHOULD); // or
                            }
                            else
                            {
                                //textbox
                                Query query1;
                                var field = indexConfig.Fields.ContainsKey(n) ? indexConfig.Fields[n] : null;
                                bool ml = field != null && field.MultiLanguage;

                                if (field != null &&
                                    (field.IndexType == "key" || (field.Items != null && field.Items.IndexType == "key")))
                                {
                                    query1 = new WildcardQuery(new Term(n, f.WildCardSearchValue));
                                }
                                else
                                {
                                    string fieldName = ml ? n + "." + DnnUtils.GetCurrentCultureCode() : n;
                                    query1 = LuceneController.ParseQuery(fieldName + ":" + f.WildCardSearchValue + "*", "Title");
                                }

                                groupQuery.Add(query1, Occur.SHOULD); // or
                            }
                        }
                        query.Add(groupQuery, Occur.MUST); //and
                    }
                }
                return query;
            }
            else
            {
                Query query = new MatchAllDocsQuery();
                return query;
            }
        }

    }
}