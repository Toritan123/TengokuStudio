using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace HeavenStudio.Games.Scripts_LoveLab
{
    public class LoveLabHeartMisc : MonoBehaviour
    {
        [SerializeField] GameObject heart;
        [SerializeField] GameObject completeHeart;
        public void destroyObj()
        {            
            Destroy(heart, 0.1f);
        } 

        public void createHeart()
        {
            Debug.Log("heart");
            //LoveLabHearts ch = Instantiate(completeHeart, LoveLab.instance.getHeartHolder()).GetComponent<LoveLabHearts>();
        }
    }
}

