using UnityEngine;
using DG.Tweening;

public class DOTweenInitializer : MonoBehaviour
{
    void Awake()
    {
        // DOTween 초기화 및 용량 설정
        DOTween.SetTweensCapacity(3000, 300);
        
        // 전역 DOTween 설정
        DOTween.defaultAutoPlay = AutoPlay.All;
        DOTween.defaultUpdateType = UpdateType.Normal;
        DOTween.defaultTimeScaleIndependent = false;
        
        // 로그 설정
        DOTween.logBehaviour = LogBehaviour.ErrorsOnly;
    }
} 