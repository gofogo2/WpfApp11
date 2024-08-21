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
[] 가이드 작성


#2024-08-21
[V] 기본 전원제어 포트 변경 8889
[V] 전원 켜는 순서 반영
[V] 전원 제어 전체
[V] pdu랑 relay에서 사용할 채널 UI 추가(등록,수정)
[V] pdu랑 relay는 등록할때 채널도 같이등록하고 제어
[V] 전원 제어 True 인것만 전체전원 제어 & 요일 전원제어에 포함

[] 세팅 구조 변경
[] 아이콘 적용

#ftp 설치

[복사]
c:\exe 에 Setup-IIS-FTP.ps1

[실행]
시작 -> PowerShell ISE 관리자 권한으로 실행

[붙여넣기]
Set-ExecutionPolicy Unrestricted 

[IIS 설치 FTP 설치 ftpuser // 1 생성]
cd c:\exe
.\ Setup-IIS-FTP.ps1

시작 -> 실행 -> inetmgr -> 사이트 우클릭 [FTP 사이트 추가] -> 사이트 이름 : contentServer -> 경로 서버로 둘 경로 -> 다음 ->SSL 사용 안함 -> 다음 -> 기본인증 -> 엑세스허용 모든 사용자 -> 읽기,쓰기 -> 마침



