using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp11.Helpers
{
    /// <summary>
    /// 전역 메시지 서비스 클래스
    /// 애플리케이션 내에서 컴포넌트 간 메시지 전달을 위한 정적 클래스
    /// 이벤트 기반 메시징 패턴을 사용하여 느슨한 결합(Loose Coupling) 구현
    /// </summary>
    public static class GlobalMessageService
    {
        /// <summary>
        /// 메시지 수신 이벤트
        /// 메시지가 전달될 때 발생하며, 메시지 내용을 string 형태로 전달
        /// </summary>
        public static event EventHandler<string> MessageReceived;

        /// <summary>
        /// 메시지를 전체 애플리케이션에 브로드캐스트하는 메서드
        /// </summary>
        /// <param name="message">전달할 메시지 내용</param>
        public static void ShowMessage(string message)
        {
            // 이벤트 핸들러가 등록되어 있으면 이벤트 발생
            // null 조건부 연산자(?.)를 사용하여 이벤트 핸들러가 null인 경우 안전하게 처리
            MessageReceived?.Invoke(null, message);
        }
    }
}
