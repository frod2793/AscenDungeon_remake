# README

# Prototype-refactory

그 기업반 프로젝트 그거를 리팩토링한 프로젝트 입니다

## 코드 작성 권장사핳

- 멤버변수는 코드의 두곳 이상에서 변수를 사용하지 않는 이상, 지역 변수로 대체합니다.
- 멤버변수는 대문자로 시작합니다. **private**이나 **protected**변수의 경우 접두사 **m**을 사용합니다.

```csharp
public  int  FloorIndex;
private int mFloorIndex;
```

- 간단한 **get** 메서드 변수라면, 람다식을 사용합니다.

```csharp
public  int  FloorIndex => mFloorIndex;
private int mFloorIndex;
```

- 코루틴을 비롯한 **IEnumerator,** **IEnumerable**은 접두사 **E**를 사용합니다.

```csharp
private IEnumerator EMove() { ... }
```

- **IEnumerator,** **IEnumerable** 멤버변수 또한 접두사 **m**을 사용합니다.

```csharp
private IEnumerator mEMove;
```

- 변수나 객체의 초기화는 선언과 동시에 이루어지게 되면 코드가 지저분해 보이므로, 
선언과 초기화를 분리합니다.

```csharp
// X
private List<GameObject> mObjectList = new List<GameObject>();
```

```csharp
// O
private List<GameObject> mObjectList;

private void Start()
{
	mObjectList = new List<GameObject>();
}
```

- 인스펙터창에서 값을 수정할 수 있도록 설계한 변수들은 **private**이나 **protected**를 사용한 뒤,
씨리얼라이즈합니다.

```csharp
[SerializeField] private GameObject SwitchingObject;
```

---

## 커밋 권장사항

**제목행은 명령문을 사용한다**

 명령문은 커밋 제목행과 잘 어우러지는 문체이기도 하고, Git의 기본 커밋 제목행또한 명령문으로 되어있기 때문에, 명령문으로 작성할 것을 권장합니다.

다만 명령문을 사용하는 것이 익숙하지 않아, 사용하기 힘들 수 있습니다. 이를 보완하는 방법이 있는데,

> 만약 이 커밋이 적용되면 이 커밋은 **[제목행 내용]**

바로 이 문장에 제목행 내용을 대입하였을 때 문장이 자연스럽다면, 명령문을 성공적으로 사용한 것 입니다.