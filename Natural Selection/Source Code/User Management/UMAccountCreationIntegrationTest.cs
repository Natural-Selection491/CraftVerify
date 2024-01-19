using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftVerify.Natural.UserManagement
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // interation test 
            Services services = new Services();
            string email = "anananana@gmail.com";
            DateTime DOB = new DateTime(2000, 1, 1);
            string RoleString = "regular";
            string securityAnswer1 = "secure 1";
            string securityAnswer2 = "secure 2";
            string securityAnswer3 = "secure 3";

            if (services.IsValidEmail(email) && services.IsValidDate(DOB, new DateTime(1970, 1, 1), DateTime.Now)
                && services.IsValidUserRole(RoleString) && services.IsValidSecureAnswers(securityAnswer1) && services.IsValidSecureAnswers(securityAnswer2)
                && services.IsValidSecureAnswers(securityAnswer3))
            {
                string userHash = services.CreateEmailPepperHash(email, "C:\\Users\\vanan\\teamSynology\\CECS 491A\\Milestone_2_analysis\\pepper.txt");
                int userID = services.GenerateUserID(10);
                string answerSalt = services.GenerateUserID(10).ToString();
                string answer1Hash = services.HashSHA256(securityAnswer1, answerSalt, "");
                string answer2Hash = services.HashSHA256(securityAnswer2, answerSalt, "");
                string answer3Hash = services.HashSHA256(securityAnswer3, answerSalt, "");

                AccountCreation acc = new AccountCreation();
                acc.userID = userID;
                acc.email = email;
                acc.DOB = DOB;
                acc.userHash = userHash;
                acc.userRole = RoleString;
                acc.securityAnswerSalt = answerSalt;
                acc.secureAnswer1 = answer1Hash;
                acc.secureAnswer2 = answer2Hash;
                acc.secureAnswer3 = answer3Hash;
                acc.DisplayAccountInfo();



                try
                {
                    if(services.InsertAccountToDatabase(acc));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                    Console.WriteLine("\n");
                }
                Console.WriteLine("Done test succcess\n");

            }




            // test 1 table inserting fail RoleString1

            string email1 = "h11110@gmail.com";
            DateTime DOB1 = new DateTime(2000, 1, 1);
            string RoleString1 = "regular";
            string securityAnswer11 = "secure 1";
            string securityAnswer21 = "secure 2";
            string securityAnswer31 = "secure 3";

            if (services.IsValidEmail(email1) && services.IsValidDate(DOB1, new DateTime(1970, 1, 1), DateTime.Now)
                && services.IsValidUserRole(RoleString1) && services.IsValidSecureAnswers(securityAnswer11) && services.IsValidSecureAnswers(securityAnswer21)
                && services.IsValidSecureAnswers(securityAnswer31))
            {
                string userHash1 = services.CreateEmailPepperHash(email1, "C:\\Users\\vanan\\teamSynology\\CECS 491A\\Milestone_2_analysis\\pepper.txt");
                int userID1 = services.GenerateUserID(10);
                string answerSalt1 = services.GenerateUserID(10).ToString();
                string answer1Hash1 = services.HashSHA256(securityAnswer11, answerSalt1, "");
                string answer2Hash1 = services.HashSHA256(securityAnswer21, answerSalt1, "");
                string answer3Hash1 = services.HashSHA256(securityAnswer31, answerSalt1, "");

                AccountCreation acc1 = new AccountCreation();
                acc1.userID = userID1;
                acc1.email = email1;
                acc1.DOB = DOB1;
                acc1.userHash = userHash1;
                acc1.userRole = "regular12345";
                acc1.securityAnswerSalt = answerSalt1;
                acc1.secureAnswer1 = answer1Hash1;
                acc1.secureAnswer2 = answer2Hash1;
                acc1.secureAnswer3 = answer3Hash1;
                acc1.DisplayAccountInfo();



                try
                {
                    if(services.InsertAccountToDatabase(acc1));
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("An error occurred: " + ex2.Message);
                    Console.WriteLine("\n");
                }
                Console.WriteLine("Done test fail\n");

            }
        }
    }
}