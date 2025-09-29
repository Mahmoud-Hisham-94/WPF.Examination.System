using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ExaminationSystem;

namespace ExaminationSys
{
    public partial class ResultsWindow : Window
    {
        public ResultsWindow(Exam exam, int totalScore, int totalMarks, bool isPracticeExam)
        {
            InitializeComponent();
            
            ExamTypeText.Text = isPracticeExam ? "Practice Exam" : "Final Exam";
            ResultsTitleText.Text = isPracticeExam ? "Practice Exam Results" : "Final Exam Results";
            
            // Calculate percentage
            double percentage = totalMarks > 0 ? (double)totalScore / totalMarks * 100 : 0;
            
            // Display score
            ScoreText.Text = $"{totalScore}/{totalMarks}";
            PercentageText.Text = $"{percentage:F1}%";
            
            // Set score color based on performance
            if (percentage >= 80)
            {
                ScoreText.Foreground = new SolidColorBrush(Colors.Green);
            }
            else if (percentage >= 60)
            {
                ScoreText.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                ScoreText.Foreground = new SolidColorBrush(Colors.Red);
            }
            
            // Populate detailed results
            PopulateDetailedResults(exam, isPracticeExam);
        }

        private void PopulateDetailedResults(Exam exam, bool isPracticeExam)
        {
            var results = new List<QuestionResult>();
            
            foreach (var kvp in exam.UserAnswers)
            {
                var question = kvp.Key;
                var userAnswer = kvp.Value;
                var correctAnswer = question.CorrectAnswers;
                
                // Check if answer is correct
                bool isCorrect = userAnswer.SequenceEqual(correctAnswer, new AnswerComparer());
                
                // Get user answer text
                string userAnswerText = userAnswer.Count > 0 
                    ? string.Join(", ", userAnswer.Select(a => a.Content))
                    : "No answer provided";
                
                // Get correct answer text
                string correctAnswerText = correctAnswer.Count > 0 
                    ? string.Join(", ", correctAnswer.Select(a => a.Content))
                    : "No correct answer";
                
                // Create result item
                var result = new QuestionResult
                {
                    QuestionHeader = question.Header,
                    QuestionBody = question.Body,
                    UserAnswer = userAnswerText,
                    CorrectAnswer = correctAnswerText,
                    IsCorrect = isCorrect,
                    Marks = question.Marks,
                    MarksText = isCorrect ? $"{question.Marks}/{question.Marks}" : $"0/{question.Marks}",
                    AnswerStyle = isCorrect ? "CorrectAnswerStyle" : "WrongAnswerStyle"
                };
                
                results.Add(result);
            }
            
            ResultsItemsControl.ItemsSource = results;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class QuestionResult
    {
        public string QuestionHeader { get; set; }
        public string QuestionBody { get; set; }
        public string UserAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public int Marks { get; set; }
        public string MarksText { get; set; }
        public string AnswerStyle { get; set; }
    }
}
