﻿## 전원 켜는 순서
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
[v] 전반적인 UX 적용
[V] 상태체크 로직 구현(pc 제외 디바이스)(핑포함)
[v] 각 아이템의 이름 글자크기 좀 줄이고, ... 처리하기
[v] 등록 수정 할때 width 정보 모두 통일
[v] 디바이스 수정하고,  디바이스 추가하면 기존 수정한 내역이 남아있음

#2024-09-02
[V] 메인타이틀에도 local_pc 이름
[V] dlp 프로젝터 이름바꾸기
[V] PC 대문자로 수정
[V] 요일별 전원관리 처리(isOn 대신 다른 거 추가)

[V] 요일별 전원제어 토글 적용(이미 되어있었음)
[V] 현장에서 테스트 할 수 있게 로그처리와 단위테스트 처리(F1 누르면 로그뜸)

[v] 아이콘 적용
[v] 디바이스 status 체크하는 딜레이 시간 config로 빼기
[v] setting.json에 vpnPw 일괄 적용 가능하도록(추가)
[V] PDU, RELAY 아이콘 변경

#2024-09-03
[V] 가이드 작성
[V] 자동 전원 설정 토글 버튼 디자인 적용
[V] 프로젝터 (delay)> RELAY, PDU (delay)> PC
[V] PC (delay)> RELAY, PDU (delay)> 프로젝터
[V] 전원 제어 delay setting으로 빼기 앞딜레이 뒷딜레이

#2024-09-04
[V] 메인 스크린 윈도우 중앙에 오게
[V] FTP 리스트 아이콘 갯수 가로 최대 4개 > 5개
[V] 버튼은 손가락 커서로 변경
[V] 스크롤 디자인(피그마 가이드)
[V] 자동 전원 시간 설정에서 콤보박스에 스크롤 수정안되면 그냥 텍스트박스로 바꾸기
[V] PJLINK 상태체크 핑말고, tcp 방식
[V] DLP 상태체크 핑말고, tcp 방식
[V] PDU 상태 체크 api 방식으로
[v] 장비 리스트 박스 라운드 값 적용
[v] 삭제 팝업 디자인 : 박스 라운드 값

#2024-09-05
[V] PDU 상태처리 이슈 해결
[V] 폰트적용 피그마에서 확인
[v] 버튼 오버 효과(개발로 변경하거나 디자인) - 이거 피그마에 만들어둔대
[v] 빈 화면 우클릭 : 장비 추가 네이밍 및 디자인(장비 리스트 우클릭 디자인과 통일)
[v] 전체적으로 버튼 라운드 처리, 
[진행중] 텍스트 중앙정렬(가로,세로) 등 제대로 적용되어있지 않아서 적용 필요
[V] FTP 이름 바꾸기 팝업 새로 디자인





#2024-09-10
[ ] 반응형 다시 풀어넣을 것
[ ] tight vnc media mng false 처리
[ ] 



#2024-09-24
[ ] FTP 파일 1개씩 팝업뜨는거 완료하면 한번 뜨는걸로 변경
[v] 전체 전원 제어 시 1분동안 한번만 제어되게 변경
[ ] CMS 아이콘 적용
[v] 세팅 빼두기(WebapiHelper에 있는 _username,_password)
[v] 기기 수정시 대문자로 강제 변경
#
수정하면 템플릿때매 구조가 변경 작업이 많이필요함


#2024-09-25

[v] 아이템 이름 글자수 조절
[v] 하단 가로 스크롤이 너무 두껍다
[v] 아래 방식으로 수정
{
  "CMSTitle": "pc",
  "ContentsPath": "C:\\GL-MEDIA",
  "AutoPowerEnabled": false,
  "Progress_duration": 50.0,
  "useVNC": false,
  "useFTP": false,
  "VNC_Password": "0909",
  "Status_Check_Interval": 30,
  "PowerInterval01": 0,
  "PowerInterval02": 0,
  "Web_name": "1",
  "Web_Password": "1"
}

요거 

{
  "CMSTitle": "pc",
  "ContentsPath": "C:\\GL-MEDIA",
  "AutoPowerEnabled": false,
  "ProgressDuration": 50.0,
  "UseVNC": false,
  "UseFTP": false,
  "VNCPassword": "0909",
  "StatusCheckInterval": 30,
  "PowerInterval01": 0,
  "PowerInterval02": 0,
  "WebName": "1",
  "WebPassword": "1"
}

이렇게 네이밍 변경 해주고

"ProgressDuration": 50.0, 
"StatusCheckInterval": 30, 이것들도 밀리 세컨즈로 바꿔줘

#
-릴레이는 상태체크 안한다
-pdu는 상태체크한다
-쉐도우는 제외


PDU 상태체크 로 검색
전체 상태체크

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