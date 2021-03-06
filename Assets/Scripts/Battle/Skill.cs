﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    SingltonSkillManager.SkillInfo skill;
    public SingltonSkillManager.SkillInfo getSkillInfo
    {
        get
        {
            return skill;
        }
    }

    public Skill()
    {
        skill = new SingltonSkillManager.SkillInfo();
        skill.ADV = 1.0f;
        skill.MDV = 0.0f;
        skill.MP = 0;
        skill.myTarget = SingltonSkillManager.Target.Enemy;
        skill.myScope = SingltonSkillManager.Scope.Simplex;
        skill.myCategory = SingltonSkillManager.Category.Damage;
    }

    public Skill(SingltonSkillManager.SkillInfo info)
    {
        skill = info;
    }

    public void DefenceAction()
    {
        skill = new SingltonSkillManager.SkillInfo();
        skill.ID = "";
        skill.MP = 0;
        skill.myTarget = SingltonSkillManager.Target.MySelf;
        skill.myScope = SingltonSkillManager.Scope.Simplex;
        skill.myCategory = SingltonSkillManager.Category.Buff;
        skill.DT = 1;
        skill.influence = BattleParam.Def;
    }


    public List<BattleAction> use(BattleCharacter from, BattleCharacter[] targets)
    {
        var ret = new List<BattleAction>();
        // 仮で攻撃力分のHPを減らす処理を作成
        // 防御力などが入った場合ここかアクションを処理するところで行う

        switch (skill.myCategory) {
            case SingltonSkillManager.Category.Damage:
                    ret.Add(new BattleAction()
                    {
                        targets = targets,
                        effects = new Dictionary<BattleParam, int>
                        {
                            { BattleParam.HP, (int)(-(from.Atk * skill.ADV + from.MAtk * skill.MDV)) }
                        }
                    });
                break;

            case SingltonSkillManager.Category.Buff:
                ret.Add(new BattleAction()
                {
                    targets = targets,
                    effects = new Dictionary<BattleParam, int>
                    {
                        {skill.influence, 5}
                    }
                });

                foreach(var c in targets) {
                    BattleCharacter.BuffManager tempManager = new BattleCharacter.BuffManager(skill);

                    if (!c.buffList.Contains(tempManager)) {
                        c.buffList.Add(tempManager);
                    }
                    else {
                        foreach(var s in c.buffList) {
                            if(s == tempManager) {
                                s.ResetBuff();
                            }
                        }
                    }
                }
                break;
            default:
                break;
        }

        ret.Add(new BattleAction()
        {
            targets = new BattleCharacter[] { from },
            effects = new Dictionary<BattleParam, int>
            {
                {BattleParam.MP, -skill.MP}
            }
        });
        return ret;
    }

    public List<BattleAction> ItemUse(BattleCharacter from, BattleCharacter[] targets, SingltonItemManager.ItemList item)
    {
        var ret = new List<BattleAction>();
        switch (item.category) {
            case SingltonSkillManager.Category.Damage:
                ret.Add(new BattleAction()
                {
                    targets = targets,
                    effects = new Dictionary<BattleParam, int>
                        {
                            { item.param, -item.value }
                        }
                });
                break;

            case SingltonSkillManager.Category.Buff:
                ret.Add(new BattleAction()
                {
                    targets = targets,
                    effects = new Dictionary<BattleParam, int>
                    {
                        {item.param, 5 }
                    }
                });

                foreach (var c in targets) {
                    BattleCharacter.BuffManager tempManager = new BattleCharacter.BuffManager(item);

                    if (!c.buffList.Contains(tempManager)) {
                        c.buffList.Add(tempManager);
                    }
                    else {
                        foreach (var s in c.buffList) {
                            if (s == tempManager) {
                                s.ResetBuff();
                            }
                        }
                    }
                }
                break;
            default:
                break;
        }

        return ret;
    }
}
