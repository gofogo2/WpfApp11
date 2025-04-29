using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp11.Helpers
{
    /// <summary>
    /// 애플리케이션 로깅을 담당하는 정적 클래스
    /// 다양한 유형의 로그(일반, 전원, 오류)를 별도 파일에 기록
    /// </summary>
    public static class Logger
    {
        // 로그 파일 경로 정의 (현재 날짜 기준으로 파일명 생성)
        /// <summary>
        /// 일반 로그 파일 경로 - 장치 관련 로그 저장
        /// </summary>
        public static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory+"Log\\", $"{DateTime.Now:yyyy-MM-dd}"+"_device_logs.txt");
        
        /// <summary>
        /// 전원 로그 파일 경로 - 장치 전원 상태 변경 관련 로그 저장
        /// </summary>
        public static readonly string LogPowerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Log\\", $"{DateTime.Now:yyyy-MM-dd}" + "_power_logs.txt");
        
        /// <summary>
        /// 오류 로그 파일 경로 - 애플리케이션 오류 관련 로그 저장
        /// </summary>
        public static readonly string LogErrorFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Log\\", $"{DateTime.Now:yyyy-MM-dd}" + "_error_logs.txt");

        /// <summary>
        /// 로그 디렉토리가 존재하지 않는 경우 생성하는 메서드
        /// </summary>
        public static void CreateD()
        {
            // 로그 디렉토리 경로가 존재하지 않으면 생성
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Log"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Log");
            }
        }

        /// <summary>
        /// 장치 관련 로그를 기록하는 메서드
        /// </summary>
        /// <param name="deviceName">장치 이름 또는 IP 주소</param>
        /// <param name="deviceType">장치 유형(예: 프로젝터, PC 등)</param>
        /// <param name="action">수행된 동작 또는 포트 정보</param>
        /// <param name="result">동작 결과 또는 상세 내용</param>
        public static void Log(string deviceName, string deviceType, string action, string result)
        {
            // 로그 메시지 형식 지정: 시간 - 아이피: 장치명 (장치유형) - 포트: 동작 - 내용: 결과
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - 아이피: {deviceName} ({deviceType}) - 포트: {action} - 내용: {result}";
            
            try
            {
                // 로그 디렉토리 존재 확인/생성
                CreateD();
                // 로그 파일에 메시지 추가 (새 줄과 함께)
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // 로그 파일 쓰기 실패 시 콘솔에 오류 출력
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        /// <summary>
        /// 간단한 내용만 포함하는 로그를 기록하는 메서드
        /// </summary>
        /// <param name="result">로그에 기록할 내용</param>
        public static void Log2(string result)
        {
            // 로그 메시지 형식 지정: 시간 - 내용: 결과
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - 내용: {result}";

            try
            {
                // 로그 디렉토리 존재 확인/생성
                CreateD();
                // 로그 파일에 메시지 추가 (새 줄과 함께)
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // 로그 파일 쓰기 실패 시 콘솔에 오류 출력
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        /// <summary>
        /// 전원 관련 로그를 기록하는 메서드
        /// </summary>
        /// <param name="result">전원 관련 로그 내용</param>
        public static void LogPower(string result)
        {
            // 로그 메시지 형식 지정: 시간 - 내용: 결과
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - 내용: {result}";

            try
            {
                // 로그 디렉토리 존재 확인/생성
                CreateD();
                // 전원 로그 파일에 메시지 추가 (새 줄과 함께)
                File.AppendAllText(LogPowerFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // 로그 파일 쓰기 실패 시 콘솔에 오류 출력
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        /// <summary>
        /// 오류 관련 로그를 기록하는 메서드
        /// </summary>
        /// <param name="result">오류 내용</param>
        public static void LogError(string result)
        {
            // 로그 메시지 형식 지정: 시간 - 내용: 결과
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - 내용: {result}";

            try
            {
                // 로그 디렉토리 존재 확인/생성
                CreateD();
                // 오류 로그 파일에 메시지 추가 (새 줄과 함께)
                File.AppendAllText(LogErrorFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // 로그 파일 쓰기 실패 시 콘솔에 오류 출력
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
    }
}
