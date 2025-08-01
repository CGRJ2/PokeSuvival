using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_PlayerStatus : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_Name;
    [SerializeField] TMP_Text tmp_Level;
    [SerializeField] TMP_Text tmp_CurHP;
    [SerializeField] TMP_Text tmp_MaxHP;
    [SerializeField] Image image_HPBar;
    [SerializeField] Image image_EXPBar;

    public void Init()
    {

    }

    void Update()
    {
        if (NetworkManager.Instance.CurServer.type != (int)ServerType.InGame) return;


        PlayerManager pm = PlayerManager.Instance;
        if (pm == null) return;
        PlayerModel model = pm.LocalPlayerController.Model;

        tmp_Name.text = model.PokeData.PokeName;
        tmp_Level.text = model.PokeLevel.ToString();
        tmp_CurHP.text = model.CurrentHp.ToString();
        tmp_MaxHP.text = model.MaxHp.ToString();

        float curHp = model.CurrentHp;
        float maxHp = model.MaxHp;

        float curExp = model.PokeExp;
        float maxExp = model.NextExp;

        image_HPBar.fillAmount = Mathf.Clamp01(curHp / maxHp);
        image_EXPBar.fillAmount = Mathf.Clamp01(curExp / maxExp);
    }
}
