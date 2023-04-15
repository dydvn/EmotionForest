❗ 감정숲 프로젝트에서 제가 작성했고, 불필요한 부분은 제거한 스크립트를 올리는 리포지토리입니다. ❗

🎮 Android download link : 23.04 출시 예정입니다.

🎞 게임 소개 영상 : https://www.youtube.com/watch?v=wbWmojGrdJ0

------------------------------------------------------------------------

Release date : 2023.04 (출시 예정)

Platform : Mobile (Google play)

------------------------------------------------------------------------


🛠 저는 이 게임의 구현 전체를 맡았고, 대표적으로 이런 걸 구현했습니다!
- 캐릭터들의 자동 이동 및 애니메이션
- 하늘에서 떨어지는 이펙트 제작
- 카메라 컨트롤
- Addressable asset system 적용
- Firebase 연동


------------------------------------------------------------------------

🛠 캐릭터들의 자동 이동 및 애니메이션



https://user-images.githubusercontent.com/62327209/232230912-27440f7a-83be-4c91-8c82-160cc2212871.mp4



- Navmesh를 이용하여 랜덤한 위치로 이동하고, 도착하면 Idle 애니메이션 출력 후 일정 시간 뒤에 새로운 목적지를 설정한 뒤 이동하도록 구현하였습니다.
- 캐릭터를 터치하면 특정 애니메이션이 출력되고, 잠시 뒤에 다시 이동하도록 구현하였습니다.
- Code - https://github.com/dydvn/EmotionForest/blob/main/Mood_MyRoom.cs


------------------------------------------------------------------------

🛠 하늘에서 떨어지는 이펙트 제작



https://user-images.githubusercontent.com/62327209/232231017-4821fe48-940c-4921-b9d9-6de365db80d7.mp4

- 하늘에서 떨어지는 오브젝트들을 파티클로 구현하였고, 자연스러운 연출을 위해 바닥에 부딪힌 뒤 일정 시간 뒤에 사라지도록 구현하였습니다.

------------------------------------------------------------------------

🛠 카메라 컨트롤


https://user-images.githubusercontent.com/62327209/232231075-dfe5735d-167e-442d-8e52-04ce2196435c.mp4

- 카메라 이동 제한 좌표값을 고정값으로 설정하지 않고 카메라 줌 수치에 따라 비율로 변동하게 하여 카메라 줌 수치가 변경하여도 일정한 구간만 출력되도록 하였습니다.
- Code - https://github.com/dydvn/EmotionForest/blob/main/CameraControl.cs


------------------------------------------------------------------------

🛠 Addressable asset system 적용



https://user-images.githubusercontent.com/62327209/232231263-79ebda8d-2057-4868-ad6a-ac26cd12e19d.mp4



- 리소스 데이터가 변경되거나 추가되더라도 애플리케이션 업데이트를 진행하지 않고 업데이트가 이루어질 수 있도록 Addressable asset system을 사용하였습니다.
- Code - https://github.com/dydvn/EmotionForest/blob/main/Addressable_Asset_System.cs
- Code - https://github.com/dydvn/EmotionForest/blob/main/Init_Menu_Func.cs

------------------------------------------------------------------------

🛠 Firebase 연동

![화면 캡처 2023-04-02 200723](https://user-images.githubusercontent.com/62327209/232232603-d77c1506-eccf-495c-b2e3-539c8afe98f4.png)
- 유저 데이터와 게임 데이터 모두를 서버에서 읽어올 때 1회 접속 시 read 횟수

![화면 캡처 2023-04-15 235309](https://user-images.githubusercontent.com/62327209/232232611-4f338f1b-8818-4505-b42a-466cb99581d1.png)
- 게임 데이터는 로컬에서, 유저 데이터는 서버에서 읽어올 때 1회 접속 시 read 횟수


- 유저 데이터와 게임 데이터를 로컬 폴더에 저장하지 않고 서버에 저장하여 사용하도록 Firebase를 연동하여 사용하였습니다.
- 개발이 진행되고 데이터가 많아질수록 read 요청이 급격하게 많아져 서버 유지비용 증가가 우려되었고, 게임 데이터는 로컬에 저장하기로 결정하였습니다.
- 게임 접속 시 로컬에 json 파일이 있는지 검사 후, 존재하지 않다면 서버에서 read 후 로컬에 json으로 저장하도록 하였습니다.
- 게임 접속 시 로컬에 json 파일이 존재한다면 read를 하지않고 로컬의 json 파일을 사용하도록 구하였습니다.
- Code - 
