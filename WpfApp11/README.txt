## 체크 사항
-하드웨어
-UI
-PDU 에서 뱅크 아울렛 뭔지 체크

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



##cms
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
[ ] (Relay)Basso
[ ] (Relay)ETC
[V] PDU


#launcher
[V] PORT
[V] 전원 관리 ON / OFF
[V] 통신 기능 ON / OFF
[ ] 통신프로세스(와치아웃,resolume)

## issue
[이슈] - 뭐가 안됨


#ftp
[V] 스크립트생성
[V] 계정생성
[] 가이드 작성


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



