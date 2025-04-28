using System;
using Game.GameElements;
using UnityEngine;

namespace Game
{
    public class StarGameElement : GameElementBase
    {
        private void Awake()
        {
            DoHover();
            DoTurn();
        }
        
    }
}