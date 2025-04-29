using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace OSC_Test.Helpers
{
    /// <summary>
    /// OSC(Open Sound Control) 메시지 전송을 담당하는 헬퍼 클래스
    /// 싱글톤 패턴을 사용하여 구현되어 애플리케이션 내에서 단일 인스턴스만 존재
    /// </summary>
    public class OSCSenderHelper
    {
        /// <summary>
        /// 싱글톤 인스턴스를 저장하는 private 정적 프로퍼티
        /// </summary>
        private static OSCSenderHelper _instance { get; set; }
        
        /// <summary>
        /// 싱글톤 인스턴스에 접근하기 위한 public 정적 프로퍼티
        /// 인스턴스가 없는 경우 새로 생성하여 반환 (지연 초기화)
        /// </summary>
        public static OSCSenderHelper Instance
        {
            get
            {
                // null 병합 연산자(??)를 사용하여 _instance가 null이면 새 인스턴스 생성
                return _instance ?? (_instance = new OSCSenderHelper());
            }
        }

        /// <summary>
        /// OSC 메시지를 지정된 IP 주소로 전송하는 메서드
        /// </summary>
        /// <param name="ip">대상 장치의 IP 주소</param>
        /// <param name="address">OSC 주소 경로의 일부</param>
        public void Send(string ip, string address)
        {
            // UDP 전송기 생성 (대상 IP와 포트 7000 사용)
            var sender = new SharpOSC.UDPSender(ip, 7000);
            // OSC 메시지 생성 (/composition/columns/{address}/connect 형식의 주소와 1 값)
            var msg = new SharpOSC.OscMessage($"/composition/columns/{address}/connect", 1);
            // 메시지 전송
            sender.Send(msg);
        }
    }
}
