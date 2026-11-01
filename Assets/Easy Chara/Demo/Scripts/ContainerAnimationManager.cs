using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HisaGames.EasyChara
{
    public class ContainerAnimationManager : MonoBehaviour
    {
        [System.Serializable]
        public class AnimDisplayData
        {
            public string animClipName;
            public int animPlayButtonID;
            public int environmentAreaID;
        }

        [SerializeField] GameObject[] animPlayButton;
        [SerializeField] GameObject[] environmentArea;
        [SerializeField] AnimDisplayData[] animDisplayData;

        [SerializeField] Sprite[] animPlayButtonSprite;
        List<GameObject> listAnimButton = new List<GameObject>();

        public static ContainerAnimationManager instance;
        public Animator characterAnimator;
        public string activeAnimationName;
        public int activeEnvironmentAreaID;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            InitAnimPlayButton();
            transform.parent.gameObject.SetActive(false);
        }

        public void SetCharacterAnimator(Animator animator)
        {
            characterAnimator = animator;
        }

        void InitAnimPlayButton()
        {
            for (int i = 0; i < animDisplayData.Length; i++)
            {
                GameObject temp = Instantiate(animPlayButton[animDisplayData[i].animPlayButtonID], transform);

                temp.GetComponent<ButtonPlayAnimScript>().InitData(
                    animDisplayData[i].animClipName,
                    animDisplayData[i].environmentAreaID,
                    characterAnimator
                    );

                if (i == 0)
                {
                    temp.GetComponent<Image>().sprite = animPlayButtonSprite[1];
                    activeAnimationName = animDisplayData[i].animClipName;
                    activeEnvironmentAreaID = animDisplayData[i].environmentAreaID;
                }
                
                listAnimButton.Add(temp);
            }
        }

        public void SetEnvironment(int environmentAreaID)
        {
            for(int i = 0; i < environmentArea.Length; i++)
            {
                if(i == environmentAreaID)
                {
                    environmentArea[i].SetActive(true);
                }
                else
                {
                    environmentArea[i].SetActive(false);
                }
            }
        }
        public void ChangeAnimation(string animationName, int environmentAreaID)
        {
            if (characterAnimator != null)
            {
                activeAnimationName = animationName;
                activeEnvironmentAreaID = environmentAreaID;
                characterAnimator.Play(activeAnimationName);
                SetEnvironment(environmentAreaID);

                for (int i = 0; i < listAnimButton.Count; i++)
                {
                    if (animDisplayData[i].animClipName != animationName)
                        listAnimButton[i].GetComponent<Image>().sprite = animPlayButtonSprite[0];
                    else
                        listAnimButton[i].GetComponent<Image>().sprite = animPlayButtonSprite[1];
                }
            }
            else
            {
                Debug.LogError("Animator is not assigned!");
            }
        }
    }
}
