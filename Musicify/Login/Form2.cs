using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Login
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectionString = "Data Source=.;Initial Catalog=MyDatabase;Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Get username and password from text boxes
                    string username = textBox1.Text;
                    string password = textBox2.Text;

                    // Use a parameterized query to validate credentials and get the UserID
                    string query = "SELECT UserID FROM AppUser WHERE username = @Username AND userpassword = @UserPassword";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@UserPassword", password);

                    // Execute the query and retrieve the UserID
                    object result = cmd.ExecuteScalar();

                    if (result != null) // User found
                    {
                        string userId = result.ToString(); // Extract UserID as a string

                        // Successful login
                        MessageBox.Show("Login Successful!");

                        // Pass the UserID to the MainForm
                        Form3 mainForm = new Form3(userId);
                        mainForm.Show();

                        // Hide the login form
                        this.Hide();
                    }
                    else
                    {
                        // Invalid credentials
                        MessageBox.Show("Invalid Username or Password!");
                    }
                }
                catch (SqlException ex)
                {
                    // Display error message in case of an exception
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
                finally
                {
                    // Ensure the connection is closed
                    conn.Close();
                }
            }
        }



        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form1 SignUpForm = new Form1();

            SignUpForm.Show();

            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
