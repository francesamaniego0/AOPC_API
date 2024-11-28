namespace AuthSystem.Manager
{
    public class DBConn
    {
        private static string GetConnectionString()
        {
            // Return My.Settings.ConnString.ToString
            //  return "Data Source=192.168.0.84,36832;Initial Catalog=EQMS;User ID=randy;Password=otik"; //test
            //return "Data Source=192.168.0.222,36832;Initial Catalog=EQMS;User ID=randy;Password=otik"; //live
           //return "Data Source=DESKTOP-4CFJ01F;Initial Catalog=AOPCDB;User ID=test;Password=1234"; //live
            //return "Data Source=EC2AMAZ-AN808JE\\MSSQLSERVER01;Initial Catalog=AOPCDB;User ID=test;Password=1234"; //server 

            return "Data Source=localhost\\SQLEXPRESS;Initial Catalog=AOPCDB;Trusted_Connection=True;User ID=test;Password=1234"; //France
        }

        public static string ConnectionString
        {
            get
            {
                return GetConnectionString();
            }
        }
    }
}
