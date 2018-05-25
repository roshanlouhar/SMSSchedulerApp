using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace MsgSchedulerApp
{


    public class DBConnect
    {
        private MySqlConnection connection;

        //Constructor
        public DBConnect()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {
            string ConnectionString = ConfigurationManager.ConnectionStrings["cnstring"].ConnectionString;
            //string ConnectionString = ConfigurationManager.AppSettings["dbConnection"];
            connection = new MySqlConnection(ConnectionString);
        }


        //open connection to database
        public bool OpenConnection()
        {
            try
            {
                if (connection.State.ToString() == "Open")
                {
                    return true;
                }
                else
                {
                    connection.Open();
                    return true;
                }

            }
            catch (MySqlException ex)
            {
                return false;
            }
        }

        //Close connection
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                return false;
            }
        }

        //Insert statement
        public void Insert(string query)
        {
            //open connection
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        //Update statement
        public void Update(string query)
        {
            //Open connection
            if (this.OpenConnection() == true)
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                //Execute query
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        //Delete statement
        public void Delete(string query)
        {

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        //Select statement
        public DataTable Select(string query)
        {
            DataTable dt = new DataTable();

            //Open connection
            if (this.OpenConnection() == true)
            {

                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dt);

                return dt;
            }
            else
            {
                return dt;
            }
        }

        //Count statement
        public int Count(string query)
        {

            int Count = -1;

            //Open Connection
            if (this.OpenConnection() == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");

                //close Connection
                this.CloseConnection();

                return Count;
            }
            else
            {
                return Count;
            }
        }

        //Backup
        public void Backup()
        {
            //try
            //{
            //    DateTime Time = DateTime.Now;
            //    int year = Time.Year;
            //    int month = Time.Month;
            //    int day = Time.Day;
            //    int hour = Time.Hour;
            //    int minute = Time.Minute;
            //    int second = Time.Second;
            //    int millisecond = Time.Millisecond;

            //    //Save file to C:\ with the current date as a filename
            //    string path;
            //    path = "C:\\" + year + "-" + month + "-" + day + "-" + hour + "-" + minute + "-" + second + "-" + millisecond + ".sql";
            //    StreamWriter file = new StreamWriter(path);


            //    ProcessStartInfo psi = new ProcessStartInfo();
            //    psi.FileName = "mysqldump";
            //    psi.RedirectStandardInput = false;
            //    psi.RedirectStandardOutput = true;
            //    psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}", uid, password, server, database);
            //    psi.UseShellExecute = false;

            //    Process process = Process.Start(psi);

            //    string output;
            //    output = process.StandardOutput.ReadToEnd();
            //    file.WriteLine(output);
            //    process.WaitForExit();
            //    file.Close();
            //    process.Close();
            //}
            //catch (IOException ex)
            //{
            //    MessageBox.Show("Error , unable to backup!");
            //}
        }

        //Restore
        public void Restore()
        {
            //try
            //{
            //    //Read file from C:\
            //    string path;
            //    path = "C:\\MySqlBackup.sql";
            //    StreamReader file = new StreamReader(path);
            //    string input = file.ReadToEnd();
            //    file.Close();


            //    ProcessStartInfo psi = new ProcessStartInfo();
            //    psi.FileName = "mysql";
            //    psi.RedirectStandardInput = true;
            //    psi.RedirectStandardOutput = false;
            //    psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}", uid, password, server, database);
            //    psi.UseShellExecute = false;


            //    Process process = Process.Start(psi);
            //    process.StandardInput.WriteLine(input);
            //    process.StandardInput.Close();
            //    process.WaitForExit();
            //    process.Close();
            //}
            //catch (IOException ex)
            //{
            //    MessageBox.Show("Error , unable to Restore!");
            //}
        }
    }
}
