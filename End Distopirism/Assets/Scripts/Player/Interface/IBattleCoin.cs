/// <summary>
/// 플레이어 코인 관련 인터페이스
/// </summary>
interface IBattleCoin
{
    /// <summary>
    /// 코인이 남아 있는지 확인
    /// </summary>
    /// <returns>코인이 0보다 크다면 True</returns>
    public bool IsCoinLeft();

    /// <summary>
    /// 코인 리롤
    /// </summary>
    /// <param name="rerollCount">리롤 횟수</param>
    /// <param name="mentality"정신력</param>
    /// <returns>코인 리롤 성공 횟수</returns>
    public int CoinRoll(int rerollCount, float mentality);

    /// <summary>
    /// 코인을 더함
    /// </summary>
    /// <param name="amount">더할 코인 수</param>
    public void AddCoin(int amount);
}
