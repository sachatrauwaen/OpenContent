namespace Satrabel.OpenContent.Components.Json
{
    /// <summary>
    /// <para>Options for merging JTokens</para>
    /// </summary>
    public class JsonMergeOptions
    {
        /// <summary>
        /// <para>How to handle arrays</para>
        /// </summary>
        public JsonMergeOptionArrayHandling ArrayHandling { get; set; }

        /// <summary>
        /// <para>Default for merge options</para>
        /// </summary>
        public static readonly JsonMergeOptions Default = new JsonMergeOptions
        {
            ArrayHandling = JsonMergeOptionArrayHandling.Overwrite
        };
    }
}