using System;
using System.Linq;
using System.Windows;
using ExaminationSystem;

namespace ExaminationSys
{
    public partial class QuestionPreviewWindow : Window
    {
        public QuestionPreviewWindow(Question question)
        {
            InitializeComponent();
            DisplayQuestion(question);
        }

        private void DisplayQuestion(Question question)
        {
            PreviewHeader.Text = question.Header;
            PreviewBody.Text = question.Body;
            PreviewType.Text = question.GetType().Name;
            PreviewMarks.Text = question.Marks.ToString();

            // Display choices based on question type
            if (question is TrueFalseQuestion)
            {
                PreviewChoices.Text = "True, False";
            }
            else if (question is ChooseOneQuestion chooseOne)
            {
                PreviewChoices.Text = string.Join(", ", chooseOne.Choices);
            }
            else if (question is ChooseAllQuestion chooseAll)
            {
                PreviewChoices.Text = string.Join(", ", chooseAll.Choices);
            }

            // Display correct answers
            PreviewCorrectAnswers.Text = string.Join(", ", question.CorrectAnswers.Select(a => a.Content));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

