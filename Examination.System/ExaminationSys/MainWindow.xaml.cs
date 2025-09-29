using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExaminationSystem;

namespace ExaminationSys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Subject _mathSubject;
        private List<Question> _practiceQuestions;
        private List<Question> _finalQuestions;
        private PracticeExam _practiceExam;
        private FinalExam _finalExam;

        public MainWindow()
        {
            InitializeComponent();
            InitializeExaminationSystem();
        }

        private void InitializeExaminationSystem()
        {
            try
            {
                // Create a subject
                _mathSubject = new Subject("Mathematics");

                // Add students
                _mathSubject.Students.Add(new Student("Alice"));
                _mathSubject.Students.Add(new Student("Bob"));
                _mathSubject.Students.Add(new Student("Charlie"));

                // Set up file paths
                string practiceQuestionsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "practice_questions.json");
                string finalQuestionsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "final_questions.json");
                string practiceLogPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "practice_log.json");
                string finalLogPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "final_log.json");

                // Create JSON question files if they don't exist
                CreateSampleJsonFilesIfNotExist(practiceQuestionsPath, finalQuestionsPath);

                // Load questions from JSON files
                _practiceQuestions = QuestionJsonRepository.LoadQuestions(practiceQuestionsPath);
                _finalQuestions = QuestionJsonRepository.LoadQuestions(finalQuestionsPath);

                // Create exam instances
                _practiceExam = new PracticeExam(60, _mathSubject, practiceLogPath);
                _finalExam = new FinalExam(90, _mathSubject, finalLogPath);

                // Add loaded questions to exam instances
                _practiceExam.Questions.AddRange(_practiceQuestions);
                _finalExam.Questions.AddRange(_finalQuestions);

                StatusText.Text = $"System loaded - {_practiceQuestions.Count} practice questions, {_finalQuestions.Count} final questions";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing examination system: {ex.Message}", "Initialization Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error loading examination system";
            }
        }

        private void CreateSampleJsonFilesIfNotExist(string practicePath, string finalPath)
        {
            if (!File.Exists(practicePath))
            {
                var samplePracticeQuestions = new List<Question>
                {
                    new TrueFalseQuestion
                    {
                        Header = "C# is an object-oriented language",
                        Body = "Is this statement true or false?",
                        Marks = 1,
                        CorrectAnswers = { new Answer("True") }
                    },
                    new ChooseOneQuestion
                    {
                        Header = "Capital of France",
                        Body = "What is the capital of France?",
                        Marks = 2,
                        Choices = new List<string> { "London", "Paris", "Berlin", "Madrid" },
                        CorrectAnswers = { new Answer("Paris") }
                    },
                    new ChooseAllQuestion
                    {
                        Header = "Prime numbers",
                        Body = "Select all prime numbers",
                        Marks = 3,
                        Choices = new List<string> { "2", "4", "5", "9" },
                        CorrectAnswers = { new Answer("2"), new Answer("5") }
                    }
                };
                QuestionJsonRepository.SaveQuestions(practicePath, samplePracticeQuestions);
            }

            if (!File.Exists(finalPath))
            {
                var sampleFinalQuestions = new List<Question>
                {
                    new TrueFalseQuestion
                    {
                        Header = ".NET is cross-platform",
                        Body = "True or false?",
                        Marks = 1,
                        CorrectAnswers = { new Answer("True") }
                    },
                    new ChooseOneQuestion
                    {
                        Header = "Largest planet",
                        Body = "Which is the largest planet in our solar system?",
                        Marks = 2,
                        Choices = new List<string> { "Earth", "Mars", "Jupiter", "Saturn" },
                        CorrectAnswers = { new Answer("Jupiter") }
                    },
                    new ChooseAllQuestion
                    {
                        Header = "Programming paradigms",
                        Body = "Select all programming paradigms supported by C#",
                        Marks = 3,
                        Choices = new List<string> { "Object-oriented", "Functional", "Procedural", "Logic" },
                        CorrectAnswers = { new Answer("Object-oriented"), new Answer("Functional"), new Answer("Procedural") }
                    }
                };
                QuestionJsonRepository.SaveQuestions(finalPath, sampleFinalQuestions);
            }
        }

        private void PracticeExamButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var examWindow = new ExamWindow(_practiceExam, "Practice Exam");
                examWindow.Owner = this;
                examWindow.ShowDialog();
                StatusText.Text = "Practice exam completed";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting practice exam: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FinalExamButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var examWindow = new ExamWindow(_finalExam, "Final Exam");
                examWindow.Owner = this;
                examWindow.ShowDialog();
                StatusText.Text = "Final exam completed";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting final exam: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewQuestionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var viewWindow = new ViewQuestionsWindow(_practiceQuestions, _finalQuestions);
                viewWindow.Owner = this;
                viewWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening questions view: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}