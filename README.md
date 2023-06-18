# Portpolio Splatoon
Unity3d를 이용한 Splatoon 개인 모작입니다.


## Coding Convention
+ private, protected 변수 맨 앞에 _붙이고 카멜표기법      ex) private Animator _animator  <br>
+ public 변수 그냥 카멜표기법                            ex) public Animator animator  <br>
+ 프로퍼티 파스칼표기법                                  ex) public int PlayerHp {get; set;}  <br>

+ interface 앞에 I 붙이기                               ex) IDamagable  <br>
+ Enum 앞에 E붙이기                                     ex) EState  <br>
+ Coroutine 앞에 Co 붙이기                              ex) private IEnumerator FireCo()  <br>
+ bool 변수 is이나 can 붙이기                           ex) isDead, canJump, isJump...  <br>

+ 함수 이름은 동사로 시작, 맨 처음 글자는 대문자          ex) public void Fire, public void GenerateEnemy...  <br>
+ Constants 쓸거면 다 대문자                            ex) public const int MINVALUE = 4;  <br>



## Git Commit Message Convention
효과적인 협업을 위한 커밋메시지 작성법
아래의 규칙을 최대한 지키면서 협업을 했으면 좋겠습니다.
고치고 싶은 부분이 있다면 말해주세요. 고치겠습니다.

```
[Type]<커밋제목>
[<커밋 내용>]
```


# Types

| Type          | Description |
|:-------------:|-------------|
| `feat`     | 새로운 기능 추가 |
| `fix`         | 버그 수정 |
| `docs`        | 문서 수정 |
| `refactor`    | 코드 리팩터링 |
| `test`        | 테스트 코드, 리팩터링 테스트 코드 추가 |
| `chore`       | 빌드 업무 수정, 패키지 매니저 수정 |
| `design`      | UI/UX 디자인 변경|



# 커밋제목
변경사항에 대한 간결한 설명이 포함된 부분!!

* 마침표 찍지 말기 (.)
* 한글로 적어도 됩니다.
* 너무 길게 적지 맙시다. (최대 30자 정도  많이 적고 싶으면 커밋 내용에 쓰세요)


# Message Body
변경사항에 대한 내용을 적는 부분!!

* 어떻게 변경했는지보다, 무엇을 변경했고, 왜 변경했는지 설명
* 여기도 한글로 적어도 됩니다.
* 너무 길게 적지 맙시다. 일기 적는 곳이 아니에요.


# Example

+ `[feat]` 플레이어 이동 구현
+ `[fix]` 플레이어 날라가는 버그 수정
+ `[docs]` Commit Message.md 수정
+ `[refactor]` 유지보수를 위한 플레이어 스크립트 리팩토링
...

### 타입 종류에 적히지 않은 단순한 작업은 타입을 적지 않습니다.
+ ex) 불필요 파일 정리, 빌드 설정


## Git 전략

# branch 설명

+ main : 본 결과물이 저장될 main branch
+ confirm : main branch에 삽입 전 검사 및 저장용 공간
+ develop : 실제로 작업할 공간

