using System.Reflection;

namespace CraftVerify.Natural.UserManagement
{
    public class AccountCreation : IAccountCreation
    {
        public int userID { get; set; }
        public string userHash { get; set; }
        public string email { get; set; }
        public DateTime DOB { get; set; }
        public string userRole { get; set; }
        public bool userStatus { get; set; }
        public DateTime dateCreate { get; set; }
        public string secureAnswer1 { get; set; }
        public string secureAnswer2 { get; set; }
        public string secureAnswer3 { get; set; }
        public string securityAnswerSalt { get; set; }
        public DateTime firstAuthenticationFailTimestamp { get; set; }
        public int failedAuthenticationAttempts { get; set; }
        // Dictionary for additional, dynamic attributes
        public Dictionary<string, object> AdditionalAttributes { get; set; }


        public AccountCreation()
        {
            userRole = "regular";
            dateCreate = DateTime.Now;
            userStatus = true;
            //firstAuthenticationFailTimestamp = DateTime.Now;
            //failedAuthenticationAttempts = 0;
            AdditionalAttributes = new Dictionary<string, object>();
        }


        public void SetAdditionalAttribute(string key, object value)
        {
            // Implementation goes here.
            if (AdditionalAttributes == null)
                AdditionalAttributes = new Dictionary<string, object>();

            AdditionalAttributes[key] = value;
        }

        public object GetAdditionalAttribute(string key)
        {
            // Implementation goes here.
            if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue(key, out var value))
            {
                return value;
            }

            return null; // Or throw an exception if that's the expected behavior.
        }


        public void DisplayAccountInfo()
        {
            Console.WriteLine("Account Information:");
            Console.WriteLine("--------------------");

            // Display standard properties
            foreach (PropertyInfo prop in this.GetType().GetProperties())
            {
                Console.WriteLine($"{prop.Name}: {prop.GetValue(this, null)}");
            }

            // Display additional attributes from the dictionary
            if (AdditionalAttributes != null && AdditionalAttributes.Count > 0)
            {
                Console.WriteLine("\nAdditional Attributes:");
                foreach (var item in AdditionalAttributes)
                {
                    Console.WriteLine($"{item.Key}: {item.Value}");
                }
            }
        }


    }
}
