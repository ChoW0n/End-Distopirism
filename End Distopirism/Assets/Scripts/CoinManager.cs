using UnityEngine;

public class CoinManager
{
    internal void CoinRoll(CharacterProfile character)
    {
        float maxMenTality = 100f;
        float maxProbability = 0.6f;

        float currentProbability = Mathf.Max(0f, maxProbability * (character.GetPlayer.menTality / maxMenTality));

        character.successCount = 0;
        for (int j = 0; j < character.GetPlayer.coin - 1; j++)
        {
            if (Random.value < currentProbability)
            {
                character.successCount++;
            }
        }
        character.coinBonus = character.successCount * character.GetPlayer.dmgUp;
        Debug.Log($"{character.GetPlayer.charName}의 코인 던지기 성공 횟수: {character.successCount} / {character.GetPlayer.coin} ");
        Debug.Log($"{character.GetPlayer.charName}의 남은 코인: {character.GetPlayer.coin} / {character.GetPlayer.maxCoin}");
    }
}
