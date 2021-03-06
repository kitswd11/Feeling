﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class BattlePlayer : BattleCharacter
{
    /// <summary>
    /// 現在プレイヤーの行動選択か
    /// </summary>
    bool isPlayerAction;

    /// <summary>
    /// UI コントローラー的なの実装する
    /// </summary>
    public Button[] combatButtons;

    [SerializeField]
    Text playerName;

    [SerializeField]
    public PlayerSelect playerSelect;

    [SerializeField]
    private Image feelingStone;

    public override void battleStart()
    {
        base.battleStart();

        playerName.text = this.param.Name;

        combatButtons = new Button[5];
        attachButton();
        combatButtons[0].OnClickAsObservable()
            .Where(_ => isPlayerAction)
            .Subscribe(_ => {
                battleUI.beforeSelect.Add(combatButtons[0]);
                BattleUI.NotActiveButton(battleController.combatGrid);
                BattleUI.ActiveButton(battleController.monsterZone);
                BattleUI.DontSelectableDeadCharacter(battleController.monsterZone);

                battleUI.beforeGrid = battleController.combatGrid;

                battleController.combatGrid.SetActive(false);

                battleUI.Mode = BattleUI.SelectMode.Monster;
            })
            .AddTo(this);

        combatButtons[1].OnClickAsObservable()
            .Where(_ => isPlayerAction)
            .Subscribe(_ => {
                BattleCharacter targets = this;
                var skill = new Skill();
                skill.DefenceAction();

                isPlayerAction = false;
                playAction(skill.use(this, new BattleCharacter[] { targets }));
            })
            .AddTo(this);

        combatButtons[2].OnClickAsObservable()
            .Where(_ => isPlayerAction)
            .Subscribe(_ =>{

                if (battleUI.skillButtonList.Count == 0) {
                    foreach (var i in skillList) {
                        battleUI.SetSkill(i);
                    }
                }

                battleUI.beforeSelect.Add(combatButtons[2]);
                battleUI.SkillWindow.SetActive(true);
                battleUI.windowLine_Skill.SetActive(true);
                battleUI.SkillDetail.SetActive(true);
                battleUI.SkillMode();
                battleController.combatGrid.SetActive(false);

                BattleUI.NotActiveButton(battleController.combatGrid);
                BattleUI.ActiveButton(battleUI.SkillWindow);
                battleUI.Mode = BattleUI.SelectMode.Skill;
            })
            .AddTo(this);

        combatButtons[3].OnClickAsObservable()
            .Where(_ => isPlayerAction)
            .Subscribe(_ => {
                if(battleUI.itemButtonList.Count == 0) {
                    battleUI.SetItem(GV.Instance.GData.Items.itemList);
                }

                battleUI.beforeSelect.Add(combatButtons[3]);
                battleUI.ItemWindow.SetActive(true);
                battleUI.windowLine_Item.SetActive(true);
                battleUI.ItemDetail.SetActive(true);
                battleUI.ItemMode();
                battleController.combatGrid.SetActive(false);

                BattleUI.NotActiveButton(battleController.combatGrid);
                BattleUI.ActiveButton(battleUI.ItemWindow);
                battleUI.Mode = BattleUI.SelectMode.Item;
            })
            .AddTo(this);

        combatButtons[4].OnClickAsObservable()
            .Where(_ => isPlayerAction)
            .Subscribe(_ => {
                SceneController.startFade((fade) => {
                    Destroy(battleController.transform.parent.gameObject);
                }, 1.0f);
            })
            .AddTo(this);
    }

    public void attackAction(BattleCharacter[] targets, string id = "none")
    {
        var skill = new Skill();
        if (id != "none") {
            skill = searchSkill(id);

            if (skill.getSkillInfo.FVC != null) {
                InfluenceFeel(skill.getSkillInfo.FVC);
            }
        }
        isPlayerAction = false;


        playAction(skill.use(this, targets));
    }

    public void useItem(BattleCharacter[] targets, string id = "none")
    {
        SingltonItemManager.ItemList info = SerchItem(id);
        var skill = new Skill();


        isPlayerAction = false;

        battleUI.cutNumber(info);
        playAction(skill.ItemUse(this, targets, info));
    }

    Skill searchSkill(string id)
    {
        SingltonSkillManager.SkillInfo skill = new SingltonSkillManager.SkillInfo();

        foreach(var s in battleController.SkillList) {
            if(s.ID == id) {
                skill = s;
            }
        }
        
        return new Skill(skill);
    }

    SingltonItemManager.ItemList SerchItem(string id)
    {
        SingltonItemManager.ItemList item = new SingltonItemManager.ItemList();

        foreach(var i in battleController.gameManager.ItemManager.CDItem) {
            if(i.id == id) {
                item = i;
            } 
        }

        return item;
    }

    public override void startAction()
    {
        BattleUI.DisplayPlayerTurn(this.name);

        isPlayerAction = true;
        battleController.combatGrid.SetActive(true);
        playerSelect.Select();

        for(int i = 1; i < combatButtons.Length; i++) {
            combatButtons[i].transform.localScale = Vector3.one;
        }

        BattleUI.ActiveButton(battleController.combatGrid, combatButtons[0].gameObject);

        combatButtons[0].transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

        if(battleUI != null)
            battleUI.Mode = BattleUI.SelectMode.Behaviour;
    }

    public override void endAction()
    {
        playerSelect.DeSelect();
        battleUI.ClearWindow();

        base.endAction();
    }

    private void attachButton()
    {
        combatButtons[0] = GameObject.Find("AtkButton").GetComponent<Button>();          //攻撃
        combatButtons[1] = GameObject.Find("DefButton").GetComponent<Button>();          //防御
        combatButtons[2] = GameObject.Find("SkillButton").GetComponent<Button>();        //スキル
        combatButtons[3] = GameObject.Find("ItemButton").GetComponent<Button>();         //アイテム
        combatButtons[4] = GameObject.Find("EscapeButton").GetComponent<Button>();       //逃げる
    }

    public void ChangeFeelingColor(SingltonSkillManager.Feel highestFeel)
    {
        if (feelingStone == null) return;
        feelingStone.color = FeelColor(highestFeel);
    }

    Color FeelColor(SingltonSkillManager.Feel feel)
    {
        Color ret = Color.white;

        switch (feel) {
            case SingltonSkillManager.Feel.Ki:
                ret = Color.green;
                break;
            case SingltonSkillManager.Feel.Do:
                ret = Color.red;
                break;
            case SingltonSkillManager.Feel.Ai:
                ret = Color.blue;
                break;
            case SingltonSkillManager.Feel.Raku:
                ret = Color.yellow;
                break;
            case SingltonSkillManager.Feel.Love:
                ret = Color.magenta;
                break;
            case SingltonSkillManager.Feel.Zou:
                ret = Color.gray;
                break;
        }

        return ret;
    }
}
