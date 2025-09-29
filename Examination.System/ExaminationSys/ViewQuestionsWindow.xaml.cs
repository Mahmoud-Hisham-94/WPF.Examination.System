using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ExaminationSystem;

namespace ExaminationSys
{
    public partial class ViewQuestionsWindow : Window
    {
        private List<Question> _practiceQuestions;
        private List<Question> _finalQuestions;

        public enum QuestionSource { Practice, Final }

        public ViewQuestionsWindow(List<Question> practiceQuestions, List<Question> finalQuestions)
        {
            InitializeComponent();
            _practiceQuestions = practiceQuestions;
            _finalQuestions = finalQuestions;
            PopulateQuestions();
        }

        private void PopulateQuestions()
        {
            // Convert practice questions to display format with source and original reference
            var practiceDisplayQuestions = _practiceQuestions.Select(q => new QuestionDisplayItem
            {
                Header = q.Header,
                Body = q.Body,
                QuestionType = q.GetType().Name,
                Marks = q.Marks.ToString(),
                Choices = GetChoicesText(q),
                CorrectAnswer = string.Join(", ", q.CorrectAnswers.Select(a => a.Content)),
                OriginalQuestion = q,
                Source = QuestionSource.Practice
            }).ToList();

            // Convert final questions to display format with source and original reference
            var finalDisplayQuestions = _finalQuestions.Select(q => new QuestionDisplayItem
            {
                Header = q.Header,
                Body = q.Body,
                QuestionType = q.GetType().Name,
                Marks = q.Marks.ToString(),
                Choices = GetChoicesText(q),
                CorrectAnswer = string.Join(", ", q.CorrectAnswers.Select(a => a.Content)),
                OriginalQuestion = q,
                Source = QuestionSource.Final
            }).ToList();

            PracticeQuestionsControl.ItemsSource = practiceDisplayQuestions;
            FinalQuestionsControl.ItemsSource = finalDisplayQuestions;
        }

        private string GetChoicesText(Question question)
        {
            if (question is TrueFalseQuestion)
            {
                return "True, False";
            }
            else if (question is ChooseOneQuestion chooseOne)
            {
                return string.Join(", ", chooseOne.Choices);
            }
            else if (question is ChooseAllQuestion chooseAll)
            {
                return string.Join(", ", chooseAll.Choices);
            }
            return "N/A";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement fe && fe.DataContext is QuestionDisplayItem item && item.OriginalQuestion != null)
                {
                    var editor = new QuestionEditorWindow(item.OriginalQuestion);
                    editor.Owner = this;
                    if (editor.ShowDialog() == true)
                    {
                        var updated = editor.ResultQuestion ?? item.OriginalQuestion;

                        // Replace in the corresponding list
                        if (item.Source == QuestionSource.Practice)
                        {
                            int idx = _practiceQuestions.IndexOf(item.OriginalQuestion);
                            if (idx >= 0) _practiceQuestions[idx] = updated;
                            SaveQuestions("practice_questions.json", _practiceQuestions);
                        }
                        else
                        {
                            int idx = _finalQuestions.IndexOf(item.OriginalQuestion);
                            if (idx >= 0) _finalQuestions[idx] = updated;
                            SaveQuestions("final_questions.json", _finalQuestions);
                        }

                        // Refresh view
                        PopulateQuestions();
                        MessageBox.Show("Question updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing question: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement fe && fe.DataContext is QuestionDisplayItem item && item.OriginalQuestion != null)
                {
                    if (MessageBox.Show("Are you sure you want to delete this question?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        if (item.Source == QuestionSource.Practice)
                        {
                            _practiceQuestions.Remove(item.OriginalQuestion);
                            SaveQuestions("practice_questions.json", _practiceQuestions);
                        }
                        else
                        {
                            _finalQuestions.Remove(item.OriginalQuestion);
                            SaveQuestions("final_questions.json", _finalQuestions);
                        }

                        PopulateQuestions();
                        MessageBox.Show("Question deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting question: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveQuestions(string fileName, List<Question> questions)
        {
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            QuestionJsonRepository.SaveQuestions(filePath, questions);
        }
    }

    public class QuestionDisplayItem
    {
        public string Header { get; set; }
        public string Body { get; set; }
        public string QuestionType { get; set; }
        public string Marks { get; set; }
        public string Choices { get; set; }
        public string CorrectAnswer { get; set; }
        public Question OriginalQuestion { get; set; }
        public ViewQuestionsWindow.QuestionSource Source { get; set; }
    }
}
