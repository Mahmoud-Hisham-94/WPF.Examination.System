using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ExaminationSystem;

namespace ExaminationSys
{
    public partial class QuestionEditorWindow : Window
    {
        private List<RadioButton> _chooseOneRadioButtons = new List<RadioButton>();
        private List<CheckBox> _chooseAllCheckBoxes = new List<CheckBox>();
        private bool _isEditMode = false;
        private Question _originalQuestion;

        // Expose the result when editing
        public Question ResultQuestion { get; private set; }

        public QuestionEditorWindow()
        {
            InitializeComponent();
            QuestionTypeComboBox.SelectedIndex = 0; // Select first item
            UpdateQuestionType();
        }

        // Edit mode constructor
        public QuestionEditorWindow(Question questionToEdit) : this()
        {
            _isEditMode = true;
            _originalQuestion = questionToEdit;
            LoadFromQuestion(questionToEdit);
        }

        private void QuestionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateQuestionType();
        }

        private void UpdateQuestionType()
        {
            var selectedItem = QuestionTypeComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;

            string questionType = selectedItem.Tag.ToString();

            // Show/hide choices section
            ChoicesSection.Visibility = questionType == "TrueFalse" ? Visibility.Collapsed : Visibility.Visible;

            // Update correct answer section
            UpdateCorrectAnswerSection(questionType);
        }

        private void UpdateCorrectAnswerSection(string questionType)
        {
            // Hide all answer sections
            TrueFalseAnswers.Visibility = Visibility.Collapsed;
            ChooseOneAnswers.Visibility = Visibility.Collapsed;
            ChooseAllAnswers.Visibility = Visibility.Collapsed;

            // Clear existing dynamic controls
            ChooseOneAnswers.Children.Clear();
            ChooseAllAnswers.Children.Clear();
            _chooseOneRadioButtons.Clear();
            _chooseAllCheckBoxes.Clear();

            switch (questionType)
            {
                case "TrueFalse":
                    TrueFalseAnswers.Visibility = Visibility.Visible;
                    CorrectAnswerLabel.Text = "Correct Answer";
                    CorrectAnswerHint.Text = "Select the correct answer:";
                    break;

                case "ChooseOne":
                    ChooseOneAnswers.Visibility = Visibility.Visible;
                    CorrectAnswerLabel.Text = "Correct Answer";
                    CorrectAnswerHint.Text = "Select the correct answer:";
                    GenerateChooseOneAnswers();
                    break;

                case "ChooseAll":
                    ChooseAllAnswers.Visibility = Visibility.Visible;
                    CorrectAnswerLabel.Text = "Correct Answers";
                    CorrectAnswerHint.Text = "Select all correct answers:";
                    GenerateChooseAllAnswers();
                    break;
            }
        }

        private void GenerateChooseOneAnswers()
        {
            var choices = GetChoicesFromText();
            for (int i = 0; i < choices.Count; i++)
            {
                var radioButton = new RadioButton
                {
                    Content = choices[i],
                    FontSize = 14,
                    Margin = new Thickness(0, 2, 0, 2),
                    Tag = i
                };
                _chooseOneRadioButtons.Add(radioButton);
                ChooseOneAnswers.Children.Add(radioButton);
            }
        }

        private void GenerateChooseAllAnswers()
        {
            var choices = GetChoicesFromText();
            for (int i = 0; i < choices.Count; i++)
            {
                var checkBox = new CheckBox
                {
                    Content = choices[i],
                    FontSize = 14,
                    Margin = new Thickness(0, 2, 0, 2),
                    Tag = i
                };
                _chooseAllCheckBoxes.Add(checkBox);
                ChooseAllAnswers.Children.Add(checkBox);
            }
        }

        private List<string> GetChoicesFromText()
        {
            var choices = new List<string>();
            var lines = ChoicesTextBox.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    choices.Add(trimmed);
                }
            }
            return choices;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                var question = CreateQuestion();
                if (question == null)
                    return;

                if (_isEditMode)
                {
                    // Return the updated question to the caller; caller will persist
                    ResultQuestion = question;
                    this.DialogResult = true;
                    this.Close();
                    return;
                }

                // Create mode: Save to appropriate exam files
                SaveQuestionToFiles(question);

                MessageBox.Show("Question saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving question: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFromQuestion(Question q)
        {
            // Populate basic fields
            HeaderTextBox.Text = q.Header;
            BodyTextBox.Text = q.Body;
            MarksTextBox.Text = q.Marks.ToString();

            // Select type in combo box by Tag
            string tag = q switch
            {
                TrueFalseQuestion => "TrueFalse",
                ChooseOneQuestion => "ChooseOne",
                ChooseAllQuestion => "ChooseAll",
                _ => "TrueFalse"
            };

            // Find ComboBoxItem with Tag = tag
            foreach (var item in QuestionTypeComboBox.Items)
            {
                if (item is ComboBoxItem cbi && cbi.Tag?.ToString() == tag)
                {
                    QuestionTypeComboBox.SelectedItem = cbi;
                    break;
                }
            }

            // Update UI for selected type
            UpdateQuestionType();

            // Populate choices if applicable
            if (q is ChooseOneQuestion co)
            {
                ChoicesTextBox.Text = string.Join("\n", co.Choices);
                GenerateChooseOneAnswers();
                // Set correct answer
                var correct = q.CorrectAnswers.Select(a => a.Content).ToHashSet();
                foreach (var rb in _chooseOneRadioButtons)
                {
                    rb.IsChecked = correct.Contains(rb.Content?.ToString() ?? string.Empty);
                }
            }
            else if (q is ChooseAllQuestion ca)
            {
                ChoicesTextBox.Text = string.Join("\n", ca.Choices);
                GenerateChooseAllAnswers();
                var correct = q.CorrectAnswers.Select(a => a.Content).ToHashSet();
                foreach (var cb in _chooseAllCheckBoxes)
                {
                    cb.IsChecked = correct.Contains(cb.Content?.ToString() ?? string.Empty);
                }
            }
            else if (q is TrueFalseQuestion)
            {
                var ans = q.CorrectAnswers.FirstOrDefault()?.Content;
                TrueRadioButton.IsChecked = string.Equals(ans, "True", StringComparison.OrdinalIgnoreCase);
                FalseRadioButton.IsChecked = string.Equals(ans, "False", StringComparison.OrdinalIgnoreCase);
            }

            // In edit mode, default to practice/final unchanged; teacher can reassign if desired
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(HeaderTextBox.Text))
            {
                MessageBox.Show("Please enter a question header.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                HeaderTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(BodyTextBox.Text))
            {
                MessageBox.Show("Please enter a question body.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                BodyTextBox.Focus();
                return false;
            }

            if (!int.TryParse(MarksTextBox.Text, out int marks) || marks <= 0)
            {
                MessageBox.Show("Please enter a valid number of marks.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                MarksTextBox.Focus();
                return false;
            }

            var selectedItem = QuestionTypeComboBox.SelectedItem as ComboBoxItem;
            string questionType = selectedItem?.Tag.ToString();

            if (questionType != "TrueFalse")
            {
                var choices = GetChoicesFromText();
                if (choices.Count < 2)
                {
                    MessageBox.Show("Please enter at least 2 choices.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ChoicesTextBox.Focus();
                    return false;
                }
            }

            if (!HasCorrectAnswerSelected())
            {
                MessageBox.Show("Please select at least one correct answer.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!PracticeExamCheckBox.IsChecked.Value && !FinalExamCheckBox.IsChecked.Value)
            {
                MessageBox.Show("Please select at least one exam to assign this question to.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool HasCorrectAnswerSelected()
        {
            var selectedItem = QuestionTypeComboBox.SelectedItem as ComboBoxItem;
            string questionType = selectedItem?.Tag.ToString();

            switch (questionType)
            {
                case "TrueFalse":
                    return TrueRadioButton.IsChecked == true || FalseRadioButton.IsChecked == true;

                case "ChooseOne":
                    return _chooseOneRadioButtons.Any(rb => rb.IsChecked == true);

                case "ChooseAll":
                    return _chooseAllCheckBoxes.Any(cb => cb.IsChecked == true);

                default:
                    return false;
            }
        }

        private Question CreateQuestion()
        {
            var selectedItem = QuestionTypeComboBox.SelectedItem as ComboBoxItem;
            string questionType = selectedItem?.Tag.ToString();
            string header = HeaderTextBox.Text.Trim();
            string body = BodyTextBox.Text.Trim();
            int marks = int.Parse(MarksTextBox.Text);

            Question question = null;
            var correctAnswers = GetCorrectAnswers();

            switch (questionType)
            {
                case "TrueFalse":
                    question = new TrueFalseQuestion
                    {
                        Header = header,
                        Body = body,
                        Marks = marks,
                        CorrectAnswers = correctAnswers
                    };
                    break;

                case "ChooseOne":
                    var choices = GetChoicesFromText();
                    question = new ChooseOneQuestion
                    {
                        Header = header,
                        Body = body,
                        Marks = marks,
                        Choices = choices,
                        CorrectAnswers = correctAnswers
                    };
                    break;

                case "ChooseAll":
                    var allChoices = GetChoicesFromText();
                    question = new ChooseAllQuestion
                    {
                        Header = header,
                        Body = body,
                        Marks = marks,
                        Choices = allChoices,
                        CorrectAnswers = correctAnswers
                    };
                    break;
            }

            return question;
        }

        private AnswerList GetCorrectAnswers()
        {
            var correctAnswers = new AnswerList();
            var selectedItem = QuestionTypeComboBox.SelectedItem as ComboBoxItem;
            string questionType = selectedItem?.Tag.ToString();

            switch (questionType)
            {
                case "TrueFalse":
                    if (TrueRadioButton.IsChecked == true)
                        correctAnswers.Add(new Answer("True"));
                    else if (FalseRadioButton.IsChecked == true)
                        correctAnswers.Add(new Answer("False"));
                    break;

                case "ChooseOne":
                    var selectedRadio = _chooseOneRadioButtons.FirstOrDefault(rb => rb.IsChecked == true);
                    if (selectedRadio != null)
                    {
                        correctAnswers.Add(new Answer(selectedRadio.Content.ToString()));
                    }
                    break;

                case "ChooseAll":
                    var selectedCheckBoxes = _chooseAllCheckBoxes.Where(cb => cb.IsChecked == true);
                    foreach (var checkBox in selectedCheckBoxes)
                    {
                        correctAnswers.Add(new Answer(checkBox.Content.ToString()));
                    }
                    break;
            }

            return correctAnswers;
        }

        private void SaveQuestionToFiles(Question question)
        {
            if (PracticeExamCheckBox.IsChecked == true)
            {
                SaveQuestionToFile(question, "practice_questions.json");
            }

            if (FinalExamCheckBox.IsChecked == true)
            {
                SaveQuestionToFile(question, "final_questions.json");
            }
        }

        private void SaveQuestionToFile(Question question, string fileName)
        {
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            
            // Load existing questions
            var existingQuestions = QuestionJsonRepository.LoadQuestions(filePath);
            
            // Add new question
            existingQuestions.Add(question);
            
            // Save back to file
            QuestionJsonRepository.SaveQuestions(filePath, existingQuestions);
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                var question = CreateQuestion();
                if (question == null)
                    return;

                var previewWindow = new QuestionPreviewWindow(question);
                previewWindow.Owner = this;
                previewWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error previewing question: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

