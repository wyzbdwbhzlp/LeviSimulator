using System.Collections.Generic;
using UnityEngine;

namespace GlobalManager
{
    public class MainGameManager : Singleton<MainGameManager>
    {


        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);


            InitComponents();
        }

        private void InitComponents()
        {

        }



    }
}
