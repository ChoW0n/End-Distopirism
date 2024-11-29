public class StatusEffect
{
    public string effectName;
    public int duration;  // 남은 지속 시간 (전투 횟수)
    public float mentalityModifier;  // 정신력 수정치
    public float healthPercentDamage;  // 체력 비례 피해
    public float incomingDamageModifier = 0f;  // 받는 피해 수정치 (%)
    public float outgoingDamageModifier = 0f;  // 주는 피해 수정치 (%)
    public float defenseModifier = 1f;  // 방어력 수정치 (1 = 100%, 0.5 = 50%)

    public StatusEffect(string name, int duration, float mentalityMod = 0, float healthDamage = 0, 
                       float incomingDamageMod = 0, float outgoingDamageMod = 0, float defenseMod = 1f)
    {
        this.effectName = name;
        this.duration = duration;
        this.mentalityModifier = mentalityMod;
        this.healthPercentDamage = healthDamage;
        this.incomingDamageModifier = incomingDamageMod;
        this.outgoingDamageModifier = outgoingDamageMod;
        this.defenseModifier = defenseMod;
    }
} 