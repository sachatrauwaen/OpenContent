namespace Satrabel.OpenContent.Components.Form
{
    public class NotificationDTO
    {
        public string From { get; set; }
        public string FromName { get; set; }
        public string FromEmailField { get; set; }
        public string FromNameField { get; set; }
        public string FromEmail { get; set; }
        public string To { get; set; }
        public string ToName { get; set; }
        public string ToEmailField { get; set; }
        public string ToNameField { get; set; }
        public string ToEmail { get; set; }
        public string ReplyTo { get; set; }
        public string ReplyToName { get; set; }
        public string ReplyToEmail { get; set; }
        public string ReplyToNameField { get; set; }
        public string ReplyToEmailField { get; set; }
        public string CcEmails { get; set; }
        public string BccEmails { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string EmailBodyType { get; set; }
        public string EmailRaw { get; set; }
    }
}