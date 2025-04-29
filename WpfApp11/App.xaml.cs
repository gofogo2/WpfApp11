using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp11
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        // 중복 실행 방지를 위한 Mutex 객체 선언
        Mutex mutex = null;

        /// <summary>
        /// App 클래스 생성자
        /// </summary>
        public App()
        {
            // 현재 실행 중인 프로세스의 이름을 가져옴
            string applicationName = Process.GetCurrentProcess().ProcessName;
            // 중복 실행 검사 메서드 호출
            Duplicate_execution(applicationName);
        }

        /// <summary>
        /// 애플리케이션의 중복 실행을 방지하는 메서드
        /// </summary>
        /// <param name="mutexName">고유한 Mutex 이름(프로세스 이름)</param>
        private void Duplicate_execution(string mutexName)
        {
            try
            {
                // Mutex 객체 생성 (첫 번째 매개변수가 false이므로 이 인스턴스는 Mutex의 초기 소유자가 아님)
                mutex = new Mutex(false, mutexName);
            }
            catch (Exception ex)
            {
                // Mutex 생성 실패 시 애플리케이션 종료
                Application.Current.Shutdown();
            }
            
            // WaitOne 메서드를 호출하여 Mutex의 소유권 획득 시도
            // 타임아웃 0은 즉시 반환, 두 번째 매개변수는 중단 가능 여부
            if (mutex.WaitOne(0, false))
            {
                // Mutex 획득 성공 (첫 번째 인스턴스) - 애플리케이션 초기화 진행
                InitializeComponent();
            }
            else
            {
                // Mutex 획득 실패 (이미 다른 인스턴스가 실행 중) - 애플리케이션 종료
                Application.Current.Shutdown();
            }
        }
    }
}
