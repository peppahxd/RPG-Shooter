using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_Shooter.Models
{
    public class Platform
    {
        public Platform(int iX, int iY, int iWidth, int iHeight)
        {
            this.iX = iX;
            this.iY = iY;
            this.iWidth = iWidth;
            this.iHeight = iHeight;
        }
        public Server Server { get; set; }
        public GeneratedObject genObject { get; set; }
        public int iX { get; set; }
        public int iY { get; set; }
        public int iWidth { get; set; }
        public int iHeight { get; set; }


        public GeneratedObject GenerateObject(List<Player> players)
        {
            genObject = null;

            if (this.iWidth > 300)
            {
                genObject = new GeneratedObject(players, this, new Bounds(20, 20), 3000, OBJECT.TANK);
                genObject.Spawn();
                return genObject;

            }

            return null;
        }

        public void DeleteObject()
        {
            Console.WriteLine("enje??");
            genObject = null;
            Console.WriteLine("enjdzde??");
            Task.Run(async () =>
            {
                Console.WriteLine("enje??");
                await Server.SendMessage(null, "DELETEOBJECT", true);
            });
            
        }


    }
}
