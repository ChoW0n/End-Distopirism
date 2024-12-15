using UnityEngine;

public class CardClickHandler : MonoBehaviour
{
    private CharacterProfile characterProfile;
    private int cardIndex = -1; // -1은 선택한 스킬 카드, 0,1,2는 각각 카드1,2,3

    public void Initialize(CharacterProfile profile, int index = -1)
    {
        characterProfile = profile;
        cardIndex = index;
    }

    private void OnMouseDown()
    {
        if (characterProfile != null && 
            characterProfile.CompareTag("Player") && 
            BattleManager.Instance.state == GameState.playerTurn)
        {
            if (cardIndex == -1)
            {
                // 선택한 스킬 카드를 클릭했을 때
                characterProfile.OnSelectedSkillClicked();
            }
            else
            {
                // 스킬 카드 1,2,3 중 하나를 클릭했을 때
                characterProfile.OnSkillCardSelected(cardIndex);
            }
        }
    }
} 