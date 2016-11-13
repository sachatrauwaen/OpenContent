namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    /// <summary>
    ///     The web service contact description.
    /// </summary>
    public class Contact
    {
        /// <summary>
        ///     The identifying name of the contact person/organization.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     The URL pointing to the contact information. MUST be in the format of a URL.
        /// </summary>
        /// <value>
        ///     The URL.
        /// </value>
        public string Url { get; set; }

        /// <summary>
        ///     The email address of the contact person/organization. MUST be in the format of an email address.
        /// </summary>
        /// <value>
        ///     The email.
        /// </value>
        public string Email { get; set; }
    }
}