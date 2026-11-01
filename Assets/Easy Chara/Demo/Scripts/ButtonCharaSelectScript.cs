using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HisaGames.EasyChara
{
    public class ButtonCharaSelectScript : MonoBehaviour
    {
        [SerializeField] int charaID;
        [SerializeField] Image imagePreview;
        [SerializeField] string charaName;
        [SerializeField] Text charaNameText;

        public void InitData(int charaID, Sprite charaPreview, string charaName)
        {
            this.charaID = charaID;
            this.imagePreview.sprite = charaPreview;
            this.charaName = charaName;
            this.charaNameText.text = this.charaName;
        }

        public void ChangeCharacter()
        {
            ContainerCharacterManager.instance.ChangeCharacter(charaID, charaName);
        }
    }
}