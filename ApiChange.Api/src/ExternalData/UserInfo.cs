

namespace ApiChange.ExternalData
{
    public class UserInfo
    {
        // Methods
        public UserInfo()
        {
            this.DisplayName = "";
            this.Mail = "";
            this.UserName = "";
            this.SureName = "";
            this.GivenName = "";
            this.Phone = "";
            this.Department = "";
        }

        // Properties
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Mail { get; set; }
        public string SureName { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
        public string Department { get; set; }
    }

}
