## 전원 켜는 순서
ON - relay(pdu) -> pdu -> projector -> pc
OFF - pc -> projector -> pdu -> relay(pdu)

##디바이스 타입
PC
PJLINK PROJECTOR - PJLINK
DLP PROJECTOR - TCP
RELAY01 - UDP
RELAY02 - UDP
PDU - WEBAPI

##리소스
ON/OFF/REBOOT 버튼 이미지 공통
폴더 및 파일 리소스
디바이스 타입에 따른 이미지(relay, pdu, project 2가지, PC)



#config
[V] CMS TITLE
[V] VNC ON / OFF
[V] 파일관리 ON / OFF
[V] 파일 기본 경로
[V] 프로그래스 시간 조절

#program
[V] 기기 관리 등록 / 삭제 
[V] 기기 관리 수정(유형은 수정 안되게 처리)
[V] 전체 기기 전원 ON / OFF / REBOOT / 상태체크(우선은 PC만)
[V] 스케쥴링 요일 별 전원 ON / OFF 시간 설정(우선은 PC만)
[V] 정해진 폴더에 파일 업데이트(버튼 방식)
[V] 폴더 생성
[V] 파일/폴더 이름 변경
[V] 선택 폴더/파일 삭제
[V] VNC 기능
[V] 스케쥴러에 일괄적용 추가
[V] 전원 On 시, 프로젝터 먼저 켜고, 그 뒤에 PC 켜지게 프로젝터는 일괄안되고 하나당 1초씩 딜레이둬서 먼저 프로젝터 먼저 켜지고 그뒤에 PC

#power
[V] DLP Projector
[V] PJLINK
[V] PC
[V] Relay
[V] PDU


## issue
[이슈] - 뭐가 안됨


#ftp
[V] 스크립트생성
[V] 계정생성



#2024-08-21
[V] 기본 전원제어 포트 변경 8889
[V] 전원 켜는 순서 반영
[V] 전원 제어 전체
[V] pdu랑 relay에서 사용할 채널 UI 추가(등록,수정)
[V] pdu랑 relay는 등록할때 채널도 같이등록하고 제어
[V] 전원 제어 True 인것만 전체전원 제어 & 요일 전원제어에 포함
[V] 세팅 구조 변경

#2024-08-22
[v] 그리드에 아이템 없는 부분 우클릭 시 기기 추가/삭제 메뉴
[v] 그리드에 아이템 없는 부분 더블클릭시 기기 추가 페이지열기
[v] 전체적으로 팝업 페이지에서 esc누르면 창 닫기(ftp 제외)
[v] itemConfig.json,schedule.json,settings.json 없을 때 기본으로 생성
[v] 맥어드레스 예외처리 (숫자랑 알파벳만 12자리 가능하게) ex)000000000000 ex)F02ER3V1123R3E

#2024-08-29
[V]우클릭 콘텍스트 메뉴 UI 구현

[v] 파일 폴더 수정 / 삭제 시 특정파일이 선택되어있어야가능


[V] 디바이스 타입 콤보박스 오류 수정
[V] 디바이스 수정 팝업 오류 수정
[V] relay 기능 업데이트
[V] pdu 기능 업데이트
[V] pdu 인덱스 처리 on / off /// 즉시 on_immediate // off_immediate
[V] relay 전원관리 테스트 및 업데이트
[V] pdu 전원관리 테스트 및 업데이트
[V] 전체 전원관리 파워 온
[V] 전체 전원관리 파워 오프
[V] 전체 전원관리 프로그래스 바 텍스트 수정
[V] 전원 관리 테스트 진행(PC, 개별 - 전체)
[V] dlp 프로젝터 tcp로 바꾸기
[V] pdu 아이템 등록하고, 수정하면 relay로 되어있음

[진행중] 전반적인 UX 적용
[진행중] 아이콘 적용

[] 가이드 작성

[] 디바이스 status 체크하는 딜레이 시간 config로 빼기
[V] 상태체크 로직 구현(pc 제외 디바이스)(핑포함)

[] 각 아이템의 이름 글자크기 좀 줄이고, ... 처리하기
[] dlp 프로젝터 이름바꾸기
[ ] setting.json에 vpnPw 일괄 적용 가능하도록(추가)
[] 등록 수정 할때 width 정보 모두 통일
[] 요일별 전원관리 처리(isOn 대신 다른 거 추가)
[] 디바이스 수정하고,  디바이스 추가하면 기존 수정한 내역이 남아있음

#ftp 설치

[복사]
c:\exe 에 Setup-IIS-FTP.ps1

[실행]
시작 -> PowerShell ISE 관리자 권한으로 실행

[붙여넣기]
Set-ExecutionPolicy Unrestricted 

[IIS 설치 FTP 설치 ftpuser // 1 생성]
cd c:\exe
.\ Setup-II ineS-FTP.ps1

시작 -> 실행 ->tmgr -> 사이트 우클릭 [FTP 사이트 추가] -> 사이트 이름 : contentServer -> 경로 서버로 둘 경로 -> 다음 ->SSL 사용 안함 -> 다음 -> 기본인증 -> 엑세스허용 모든 사용자 -> 읽기,쓰기 -> 마침
