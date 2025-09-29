using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ExaminationSystem;

namespace ExaminationSys
{
    public partial class ExamStatsWindow : Window
    {
        public ExamStatsWindow(List<Question> practiceQuestions, List<Question> finalQuestions, 
                              PracticeExam practiceExam, FinalExam finalExam)
        {
            InitializeComponent();
            DisplayStatistics(practiceQuestions, finalQuestions, practiceExam, finalExam);
        }

        private void DisplayStatistics(List<Question> practiceQuestions, List<Question> finalQuestions, 
                                     PracticeExam practiceExam, FinalExam finalExam)
        {
            // Practice Exam Statistics
            PracticeTotalQuestions.Text = practiceQuestions.Count.ToString();
            PracticeTotalMarks.Text = practiceQuestions.Sum(q => q.Marks).ToString();
            PracticeTimeLimit.Text = $"{practiceExam.Time} minutes";
            PracticeQuestionTypes.Text = GetQuestionTypesString(practiceQuestions);
            PracticeAvgMarks.Text = practiceQuestions.Count > 0 ? 
                (practiceQuestions.Sum(q => q.Marks) / (double)practiceQuestions.Count).ToString("F1") : "0";

            // Final Exam Statistics
            FinalTotalQuestions.Text = finalQuestions.Count.ToString();
            FinalTotalMarks.Text = finalQuestions.Sum(q => q.Marks).ToString();
            FinalTimeLimit.Text = $"{finalExam.Time} minutes";
            FinalQuestionTypes.Text = GetQuestionTypesString(finalQuestions);
            FinalAvgMarks.Text = finalQuestions.Count > 0 ? 
                (finalQuestions.Sum(q => q.Marks) / (double)finalQuestions.Count).ToString("F1") : "0";

            // Overall Statistics
            var allQuestions = practiceQuestions.Concat(finalQuestions).ToList();
            OverallTotalQuestions.Text = allQuestions.Count.ToString();
            OverallTotalMarks.Text = allQuestions.Sum(q => q.Marks).ToString();
            OverallQuestionTypes.Text = GetUniqueQuestionTypesString(allQuestions);
        }

        private string GetQuestionTypesString(List<Question> questions)
        {
            if (questions.Count == 0) return "None";

            var typeCounts = questions.GroupBy(q => q.GetType().Name)
                                   .ToDictionary(g => g.Key, g => g.Count());

            return string.Join(", ", typeCounts.Select(kvp => $"{kvp.Key} ({kvp.Value})"));
        }

        private string GetUniqueQuestionTypesString(List<Question> questions)
        {
            if (questions.Count == 0) return "None";

            var uniqueTypes = questions.Select(q => q.GetType().Name).Distinct().ToList();
            return string.Join(", ", uniqueTypes);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

