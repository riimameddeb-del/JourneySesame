using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HisaGames.EasyChara
{
    public class ButtonPlayAnimScript : MonoBehaviour
    {
        [SerializeField] string animationName;
        [SerializeField] int environmentAreaID;

        public void InitData(string animationName, int environmentAreaID, Animator characterAnimator)
        {
            this.animationName = animationName;
            this.environmentAreaID = environmentAreaID;
            GetComponentInChildren<Text>().text = animationName;
        }

        public void ChangeAnimation()
        {
            ContainerAnimationManager.instance.ChangeAnimation(animationName, environmentAreaID);
        }
    }
}