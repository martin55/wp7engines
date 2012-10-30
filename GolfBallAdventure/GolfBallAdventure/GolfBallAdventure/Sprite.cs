using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Xna.Framework.Graphics;

// noxo 2011
namespace GolfBallAdventure
{
    public class Sprite
    {
        float x, y;
        Texture2D texture;
        bool drown;

        public Sprite(Texture2D texture) {
            this.texture = texture;
        } 

        public float X
        {
            get
            {
                return x;
            }

            set
            {
                x = value;
            }
        }

        public bool Drown
        {
            get
            {
                return drown;
            }

            set
            {
                drown = value;
            }
        }

        public float Y
        {
            get
            {
                return y;
            }

            set
            {
                y = value;
            }
        }

        public Texture2D Texture2D
        {
            get
            {
                return texture;
            }

            set
            {
                texture = value;
            }        
        }


    }
}
