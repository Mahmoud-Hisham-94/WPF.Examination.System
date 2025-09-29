using System;
using System.Windows;

namespace ExaminationSys
{
    public partial class LoginWindow : Window
    {
        public enum UserRole
        {
            Student,
            Teacher
        }

        public UserRole SelectedRole { get; private set; }
        public bool IsAuthenticated { get; private set; }

        // Hardcoded teacher credentials
        private const string TEACHER_USERNAME = "teacher";
        private const string TEACHER_PASSWORD = "admin123";

        public LoginWindow()
        {
            InitializeComponent();
            UsernameTextBox.Text = TEACHER_USERNAME; // Pre-fill for convenience
        }

        private void StudentLoginButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedRole = UserRole.Student;
            IsAuthenticated = true;
            this.DialogResult = true;
            this.Close();
        }

        private void TeacherLoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (username == TEACHER_USERNAME && password == TEACHER_PASSWORD)
            {
                SelectedRole = UserRole.Teacher;
                IsAuthenticated = true;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password.\n\nTeacher Credentials:\nUsername: teacher\nPassword: admin123", 
                    "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Clear password field
                PasswordBox.Password = "";
                PasswordBox.Focus();
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            UsernameTextBox.Focus();
        }
    }
}

