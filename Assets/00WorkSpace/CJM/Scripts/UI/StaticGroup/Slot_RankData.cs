using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Slot_RankData : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_RankAndName;
    [SerializeField] TMP_Text tmp_Score;

    public void UpdateView(string rankAndName, string score)
    {
        tmp_RankAndName.text = rankAndName;
        tmp_Score.text = score;
    }
}
