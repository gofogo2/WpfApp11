using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp11
{
    public class InputDialog : Window
    {
        private TextBox textBox;

        public InputDialog(string question, string defaultAnswer = "")
        {
            Width = 300;
            Height = 150;
            Title = question;

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            textBox = new TextBox { Margin = new Thickness(5), Text = defaultAnswer };
            grid.Children.Add(textBox);
            Grid.SetRow(textBox, 0);

            Button okButton = new Button { Content = "OK", Width = 60, Margin = new Thickness(5) };
            okButton.Click += (sender, e) => { DialogResult = true; };
            grid.Children.Add(okButton);
            Grid.SetRow(okButton, 1);

            Content = grid;

            textBox.Focus();
        }

        public string Answer
        {
            get { return textBox.Text; }
        }
    }
}
