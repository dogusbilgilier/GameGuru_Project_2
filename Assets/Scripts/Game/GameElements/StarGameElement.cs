using System;
using Game.GameElements;
using UnityEngine;

namespace Game
{
    public class StarGameElement : GameElementBase
    {
        private void Awake()
        {
            DoHover(1f, 1.5f);
        }
    }
}