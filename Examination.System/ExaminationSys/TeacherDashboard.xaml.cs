using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ExaminationSystem;

namespace ExaminationSys
{
    public partial class TeacherDashboard : Window
    {
        private List<Question> _practiceQuestions;
        private List<Question> _finalQuestions;
        private PracticeExam _practiceExam;
        private FinalExam _finalExam;

        public TeacherDashboard(List<Question> practiceQuestions, List<Question> finalQuestions, 
                               PracticeExam practiceExam, FinalExam finalExam)
        {
            InitializeComponent();
            
            _practiceQuestions = practiceQuestions;
            _finalQuestions = finalQuestions;
            _practiceExam = practiceExam;
            _finalExam = finalExam;
            
            UpdateStatusDisplay();
        }

        private void UpdateStatusDisplay()
        {
            PracticeQuestionsCount.Text = _practiceQuestions.Count.ToString();
            FinalQuestionsCount.Text = _finalQuestions.Count.ToString();
            PracticeExamTime.Text = $"{_practiceExam.Time} minutes";
            FinalExamTime.Text = $"{_finalExam.Time} minutes";
        }

        private void AddQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var questionEditor = new QuestionEditorWindow();
                questionEditor.Owner = this;
                if (questionEditor.ShowDialog() == true)
                {
                    // Refresh questions after adding
                    RefreshQuestions();
                    MessageBox.Show("Question added successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding question: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewAllQuestionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var viewWindow = new ViewQuestionsWindow(_practiceQuestions, _finalQuestions);
                viewWindow.Owner = this;
                viewWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing questions: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshQuestionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshQuestions();
                MessageBox.Show("Questions refreshed successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing questions: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExamSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = new ExamSettingsWindow(_practiceExam, _finalExam);
                settingsWindow.Owner = this;
                if (settingsWindow.ShowDialog() == true)
                {
                    UpdateStatusDisplay();
                    MessageBox.Show("Exam settings updated successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening exam settings: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewExamStatsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var statsWindow = new ExamStatsWindow(_practiceQuestions, _finalQuestions, _practiceExam, _finalExam);
                statsWindow.Owner = this;
                statsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing exam statistics: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestPracticeExamButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var examWindow = new ExamWindow(_practiceExam, "Practice Exam (Teacher Test)");
                examWindow.Owner = this;
                examWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing practice exam: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestFinalExamButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var examWindow = new ExamWindow(_finalExam, "Final Exam (Teacher Test)");
                examWindow.Owner = this;
                examWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing final exam: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportQuestionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Export Questions",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var allQuestions = new List<Question>();
                    allQuestions.AddRange(_practiceQuestions);
                    allQuestions.AddRange(_finalQuestions);
                    
                    QuestionJsonRepository.SaveQuestions(saveDialog.FileName, allQuestions);
                    MessageBox.Show($"Questions exported successfully to {saveDialog.FileName}", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting questions: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RefreshQuestions()
        {
            try
            {
                // Reload questions from JSON files
                string practiceQuestionsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "practice_questions.json");
                string finalQuestionsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "final_questions.json");
                
                _practiceQuestions = QuestionJsonRepository.LoadQuestions(practiceQuestionsPath);
                _finalQuestions = QuestionJsonRepository.LoadQuestions(finalQuestionsPath);
                
                // Update exam instances
                _practiceExam.Questions.Clear();
                _practiceExam.Questions.AddRange(_practiceQuestions);
                
                _finalExam.Questions.Clear();
                _finalExam.Questions.AddRange(_finalQuestions);
                
                UpdateStatusDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing questions: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

