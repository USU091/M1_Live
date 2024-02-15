using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SpellIndicator : BaseObject
{
    //인게임에서 방향성을 갖고 움직이는 오브젝트처럼 다룰 예정임. 그러므로 BaseObject를 상속받음
    private Creature _owner;
    private SkillData _skillData;
    private EIndicatorType _indicatorType = EIndicatorType.Cone;

    private SpriteRenderer _coneSprite;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _coneSprite = Util.FindChild<SpriteRenderer>(gameObject, "Cone", true);
        _coneSprite.sortingOrder = SortingLayers.SPELL_INDICATOR;

        return true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Cancel();
    }

    public void SetInfo(Creature owner, SkillData skillData, EIndicatorType type)
    {
        _skillData = skillData;
        _indicatorType = type;
        _owner = owner;

        _coneSprite.gameObject.SetActive(true);
        _coneSprite.material.SetFloat("_Angle", 0);
        _coneSprite.material.SetFloat("_Duration", 0);
    }

    public void ShowCone(Vector3 startPos, Vector3 dir, float angleRange)
    {
        _coneSprite.gameObject.SetActive(true);
        transform.position = startPos;
        _coneSprite.material.SetFloat("_Angle", angleRange);
        _coneSprite.transform.localScale = Vector3.one * _skillData.SkillRange;
        transform.eulerAngles = GetLookRotation(dir);
        StartCoroutine(SetConeFill());
    }

    private IEnumerator SetConeFill()
    {
        float elapsedTime = 0;
        float value = 0;

        while(elapsedTime < _skillData.AnimImpactDuration)
        {
            value = Mathf.Lerp(0f, 1f, elapsedTime / _skillData.AnimImpactDuration);
            _coneSprite.material.SetFloat("_Duration", value);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _coneSprite.gameObject.SetActive(false);
    }

    public void Cancel()
    {
        StopAllCoroutines();
        _coneSprite.gameObject.SetActive(false);
    }
}
