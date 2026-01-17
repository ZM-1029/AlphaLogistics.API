namespace WALMS.API.Common
{
    public class AppConfigureData
    {      
        public Dictionary<string, string> Salutation { get; set; }
        public Dictionary<string, string> OrderStatus { get; set; }
        public Dictionary<string, string> CommunicationType { get; set; }
        public Dictionary<string, string> ValueType { get; set; }
        public Dictionary<string, string> AdditonalType { get; set; }
        public Dictionary<string, string> UOM { get; set; }      
        public Dictionary<string, string> TrainingType { get; set; }      
        public class StaticDataOptions
        {
            public Dictionary<string, string> TimeZones { get; set; } = new();
            public Dictionary<string, string> RolePermission { get; set; } = new();
            public Dictionary<string, string> NoteTypes { get; set; } = new();
            public Dictionary<string, string> Gender { get; set; } = new();
            public Dictionary<string, string> BloodGroup { get; set; } = new();
            public Dictionary<string, string> MaritalStatus { get; set; } = new();
            public Dictionary<string, string> EmploymentType { get; set; } = new();
            public Dictionary<string, string> EmploymentStatus { get; set; } = new();
            public Dictionary<string, string> SourceofHire { get; set; } = new();
            public Dictionary<string, string> Shift { get; set; } = new();
            public Dictionary<string, string> ReguralrisationReason { get; set; } = new();
            public Dictionary<string, string> Status { get; set; } = new();

            public Dictionary<string, string> Request { get; set; } = new();

            public Dictionary<string, string> Exitreason { get; set; } = new();
            public Dictionary<string, string> HolidayClassification { get; set; } = new();
            public Dictionary<string, string> HolidayTarget { get; set; } = new();

            public Dictionary<string, string> OrganizationType { get; set; } = new();

            public Dictionary<string,string> AttedenceStatus { get; set; } = new();

            public Dictionary<string, string> DisciplinaryTypes { get; set; } = new();

        }
    }
}
