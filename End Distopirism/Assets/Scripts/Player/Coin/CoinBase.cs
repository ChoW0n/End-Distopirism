using UnityEngine;

public class CoinBase : MonoBehaviour, IBattleCoin
{
    public int maxCoin;

    [field: SerializeField]
    [field: Header("보유하고 있는 코인")]
    public int Coin { get; private set; }

    // FIXME: 여기에 존재하면 안 됨.
    //        별도로 정신력 관련 클레스 생성
    private float maxMentality = 100.0f;
    
    private const float MAX_PROBABILITY = 60.0f;

    private void Start()
    {
        Coin = maxCoin;
    }

    public void AddCoin(int amount)
    {
        Coin += amount;
    }

    public int CoinRoll(int rerollCount, float mentality)
    {
        int successCount = 0;

        for (int i = 0; i < rerollCount; ++i)
        {
            float currentProbability = Mathf.Max(0.0f, MAX_PROBABILITY * (mentality / maxMentality));

            for (int j = 0; j < Coin - 1; ++j)
            {
                if (currentProbability > Random.value)
                {
                    Debug.Log($"{gameObject.name} > Coin throw success count:({successCount}) Coin count:({Coin})");
                    ++successCount;
                }
            }
        }

        return successCount;
    }

    public bool IsCoinLeft() => Coin > 0;
}
