using System.Data.SqlClient;

namespace Login
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form2 LoginForm = new Form2();

            LoginForm.Show();

            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Connection string to connect to the database
            SqlConnection conn = new SqlConnection("Data Source=.;Initial Catalog=MyDatabase;Integrated Security=True;");

            try
            {
                conn.Open(); // Open the database connection

                // Retrieve input from text boxes
                string username = textBox2.Text;
                string email = textBox3.Text;
                string userpassword = textBox4.Text;

              
                Guid userid = Guid.NewGuid();

                // Create the query to insert values into the AppUser table
                string query = "INSERT INTO AppUser (userID, userName, Email, UserPassword, JoinDate, SubscriptionState) " +
                               "VALUES (@userid, @username,@email,@userpassword, @joindate, @subscriptionstatus)";

                // Create a command object with parameterized queries to prevent SQL injection
                SqlCommand cm = new SqlCommand(query, conn);
                cm.Parameters.AddWithValue("@userid", userid.ToString());
                cm.Parameters.AddWithValue("@username", username);
                cm.Parameters.AddWithValue("@userpassword", userpassword);
                cm.Parameters.AddWithValue("@email", email);
                cm.Parameters.AddWithValue("@joindate", DateTime.Now);
                cm.Parameters.AddWithValue("@subscriptionstatus", "NotActive");

                cm.ExecuteNonQuery(); // Execute the query

                MessageBox.Show("Data Added Successfully"); // Display success message
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message); // Display error message in case of exception
            }
            finally
            {
                conn.Close(); // Ensure the connection is closed
            }
        }

    }
}
