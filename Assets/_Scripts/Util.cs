using System;
using UnityEngine;

namespace Minesweeper
{
    public static class Util
    {
        
        public static object Get(this object[,] array, Vector2Int coords)
        {
           return array[coords.x, coords.y];
        }
        
        public static T Get<T>(this T[,] array, Vector2Int coords)
        {
            return array[coords.x, coords.y];
        }

    }
}
