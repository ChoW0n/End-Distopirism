using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    public int stageNumber;
    public string sceneName; // 스테이지 씬 이름
    public int requiredCharacterCount; // 스테이지에 필요한 캐릭터 수
    public Vector3[] characterPositions; // 플레이어 캐릭터들의 시작 위치
} 