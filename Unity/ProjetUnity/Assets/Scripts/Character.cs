﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Character : MonoBehaviour
{
    //public Items[] items;
    public Stats healthPoint;
    public Stats attackPoint;
    public Stats movePoint;
    public Stats minDistAttack;
    public Stats maxDistAttack;

    public Stats naPasJouer;

    public Equipement equipement;
    public GameObject canvas;
    public Sprite ally;
    public Sprite enemy;

    public bool tower = false;

    private TextMeshProUGUI healthTextPerUnit;

    public float moveTime = .1f;
    private float inverseMoveTime;
    private Rigidbody2D rb2D;
    private Image image;

    void Start()
    {
        inverseMoveTime = 0.8f / moveTime;
        rb2D = GetComponent<Rigidbody2D>();
    }
    void Awake()
    {
        GameObject canvasGO = Instantiate(canvas, transform.position, Quaternion.identity);
        canvasGO.transform.SetParent(transform);
        image = canvasGO.transform.GetChild(0).gameObject.GetComponent<Image>();

        healthTextPerUnit = canvasGO.transform.Find("ImageHealth").transform.Find("HealthTextPerUnit").GetComponent<TextMeshProUGUI>();
    }

    public void GetHealed(int pdv)
    {
        healthPoint.currentStat = pdv;
        healthTextPerUnit.SetText(healthPoint.currentStat.ToString());
    }

    public bool GetAttacked(int loss)
    {        
        healthPoint.DecreaseCurrent(loss);
        healthTextPerUnit.SetText(healthPoint.currentStat.ToString());
        if (GameManager.instance.GetHealthShown() != "")
        {
            GameManager.instance.ChangeHealth(healthPoint.currentStat);
        }
        if (healthPoint.currentStat <= 0)
        {
            Destroy(gameObject);
            if(image.sprite == ally)
            {
                if (tower)
                {
                    GameManager.instance.YouLost();
                    GameManager.instance.network.SendString("YouWin", new List<object>());
                }
                GameManager.instance.RemoveFromAllies(gameObject);
            }
            else
            {
                if (tower)
                {
                    GameManager.instance.YouWin();
                    GameManager.instance.network.SendString("YouLost", new List<object>());
                }
                GameManager.instance.RemoveFromEnemies(gameObject);
            }
            
            return true;
        }
        return false;
    }

    public int GethealthPointwithEquipement()
    {
        if (equipement != null)
            return equipement.healthPoint.currentStat + healthPoint.currentStat;
        else
            return healthPoint.currentStat;
    }

    public int GetAttackPointwithEquipement()
    {
        if (equipement != null)
            return equipement.attackPoint.currentStat + attackPoint.currentStat;
        else
            return attackPoint.currentStat;
    }
    public int GetMovePointwithEquipement()
    {
        if (equipement != null)
            return equipement.movePoint.currentStat + movePoint.currentStat;
        else
            return movePoint.currentStat;
    }

    public void RoundEnded()
    {
        movePoint.ResetCurrent();
    }


    protected IEnumerator SmoothMovement(Vector3 end)
    {
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }

    }


    public void SetState(bool isAlly)
    {
        if (isAlly)
        {
            image.sprite = ally;
        }
        else
        {
            image.sprite = enemy;
        }
        healthTextPerUnit.SetText(healthPoint.currentStat.ToString());
    }


    public void resetTurn()
    {
        movePoint.ResetCurrent();
    }



    public void Move(int used)
    {
        movePoint.DecreaseCurrent(used);
        if (GameManager.instance.GetMoveShown() != "")
        {
            GameManager.instance.ChangeMove(movePoint.currentStat);
        }
    }
    public void Move(List<Vector3> follow)
    {
        foreach (Vector3 pos in follow)
        {
            StartCoroutine(SmoothMovement(pos));
        }
    }
    public void Move(Vector3 pos)
    {
        if (GameManager.instance.GetPhase())
        {
            transform.position = pos;
        }
    }

}
