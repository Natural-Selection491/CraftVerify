using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataAccessLibraryCraftVerify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibraryCraftVerify.Tests
{
    [TestClass()]
    public class SQLServerDAOTests
    {
        [TestMethod()]
        public void InsertAttributeTest()
        {
            //string MySqlServerconnString = "@Server=myServerAddress;Database=myDataBase;User=myUsername;Password=myPassword;\r\n";

            string INSERTsqlcommand = "INSERT INTO ClaimPrinciple (claim, stuff, hashPrinciple) VALUES ('admin', 'admin', 'aikjwnrvikhjqb3nrb');";
            SQLServerDAO sqlDAO = new SQLServerDAO();

            Assert.IsTrue((sqlDAO.InsertAttribute(INSERTsqlcommand)) == 1);
        }

        [TestMethod()]
        public void GetAttributeTest()
        {
            string GETsqlcommand = "SELECT * FROM ClaimPrinciple WHERE claim = 'admin';";
            SQLServerDAO sqlDAO = new SQLServerDAO();
            Console.WriteLine(sqlDAO.GetAttribute(GETsqlcommand));
            Assert.IsTrue(sqlDAO.GetAttribute(GETsqlcommand) != null);
        }
    }
}