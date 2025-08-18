using System;
using PlayerControllers;
using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using TMPro;
using UnityEngine;

public class DebugSpeedShowController : Singleton<DebugSpeedShowController>
{
    [SerializeField]private TextMeshProUGUI speedShowTMP;
    [SerializeField]private TextMeshProUGUI parentStateNameTMP;
    [SerializeField]private TextMeshProUGUI subStateNameTMP;
    private PlayerInputRouter _inputRouter;
    private HierarchicalBaseState _currentSubState;

    public void UpdateSpeedTMP(float speed)
    {
        //保留俩小数
        speed= Mathf.Round(speed * 100f) / 100f;
        speedShowTMP.text = "Speed:" + speed;
    }

    public void FixedUpdate()
    {
        parentStateNameTMP.text = "Parent State: " + _inputRouter?.StatusStrategy.GetType().Name;
        subStateNameTMP.text = "Sub State: " + _currentSubState?.GetType().Name;
    }

    public void SetRouter(PlayerInputRouter inputRouter)
    {
        _inputRouter = inputRouter;
    }

    public void SetParentState(HierarchicalBaseState state)
    {
        _currentSubState=state;
    }
    
}
