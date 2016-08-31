using Newtonsoft.Json;
using System;

namespace Satrabel.OpenContent.Components.Datasource.search
{
    public class RuleValue
    {
        public virtual string AsString
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        [JsonIgnore]
        public virtual int AsInteger
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        [JsonIgnore]
        public virtual float AsFloat
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        [JsonIgnore]
        public virtual long AsLong
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        [JsonIgnore]
        public virtual DateTime AsDateTime
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        [JsonIgnore]
        public virtual bool AsBoolean
        {
            get
            {
                throw new NotImplementedException();
            }
        }

    }
}