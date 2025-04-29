using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp11
{
    /// <summary>
    /// 사용자로부터 텍스트 입력을 받기 위한 대화 상자 클래스
    /// </summary>
    public class InputDialog : Window
    {
        // 사용자 입력을 받는 텍스트 상자
        private TextBox textBox;

        /// <summary>
        /// InputDialog 생성자
        /// </summary>
        /// <param name="question">대화 상자의 제목으로 표시될 질문</param>
        /// <param name="defaultAnswer">텍스트 상자에 표시될 기본 답변(기본값: 빈 문자열)</param>
        public InputDialog(string question, string defaultAnswer = "")
        {
            // 대화 상자 크기 설정
            Width = 300;
            Height = 150;
            // 대화 상자의 제목을 질문으로 설정
            Title = question;
           
            // 그리드 레이아웃 생성
            Grid grid = new Grid();
            // 두 개의 행 정의 추가: 텍스트 상자용과 버튼용
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // 텍스트 상자 생성 및 설정
            textBox = new TextBox { Margin = new Thickness(5), Text = defaultAnswer };
            grid.Children.Add(textBox);
            Grid.SetRow(textBox, 0);  // 첫 번째 행에 텍스트 상자 배치

            // 확인 버튼 생성 및 설정
            Button okButton = new Button { Content = "OK", Width = 60, Margin = new Thickness(5) };
            // 버튼 클릭 이벤트 핸들러: 클릭 시 DialogResult = true로 설정하여 대화 상자 닫기
            okButton.Click += (sender, e) => { DialogResult = true; };
            grid.Children.Add(okButton);
            Grid.SetRow(okButton, 1);  // 두 번째 행에 버튼 배치

            // 그리드를 윈도우의 내용으로 설정
            Content = grid;

            // 텍스트 상자에 포커스 설정하여 즉시 입력 가능하게 함
            textBox.Focus();
        }

        /// <summary>
        /// 사용자가 입력한 텍스트를 반환하는 속성
        /// </summary>
        public string Answer
        {
            get { return textBox.Text; }
        }
    }
}
