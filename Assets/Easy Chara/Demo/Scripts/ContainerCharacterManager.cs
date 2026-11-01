using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HisaGames.EasyChara.ContainerAnimationManager;

namespace HisaGames.EasyChara
{
    public class ContainerCharacterManager : MonoBehaviour
    {
        [System.Serializable]
        public class CharacterBankData
        {
            public GameObject charaPrefab;
            [HideInInspector] public string charaName;
            public Sprite spritePreview;
            public int charaButtonID;
        }

        public Transform charaTrunkObject;
        [SerializeField] GameObject[] charaSelectButton;
        public CharacterBankData[] characterBankData;

        public static ContainerCharacterManager instance;

        public GameObject currentCharaObject;
        public string activeCharaName;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            InitCharaPlayButton();
        }

        void InitCharaPlayButton()
        {
            for (int i = 0; i < characterBankData.Length; i++)
            {
                GameObject temp = Instantiate(charaSelectButton[characterBankData[i].charaButtonID], transform);
                //set chara name based on chara prefab name
                characterBankData[i].charaName = characterBankData[i].charaPrefab.name.Replace(" Character", "");
                temp.GetComponent<ButtonCharaSelectScript>().InitData(
                    i, characterBankData[i].spritePreview, characterBankData[i].charaName);

                if (i == 0)
                {
                    ChangeCharacter(i, characterBankData[i].charaName);
                }
            }
        }

        public void ChangeCharacter(int charaID , string charaName)
        {
            foreach (Transform child in charaTrunkObject)
            {
                GameObject.Destroy(child.gameObject);
            }
            currentCharaObject = Instantiate(characterBankData[charaID].charaPrefab,
                charaTrunkObject);
            activeCharaName = charaName;

            ContainerAnimationManager.instance.characterAnimator = currentCharaObject.GetComponent<Animator>();
            ContainerAnimationManager.instance.ChangeAnimation(
                ContainerAnimationManager.instance.activeAnimationName,
                ContainerAnimationManager.instance.activeEnvironmentAreaID
            );
        }
    }
}