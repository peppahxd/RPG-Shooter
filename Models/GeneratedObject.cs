using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RPG_Shooter.Models
{
    public enum OBJECT
    {
        TANK,
        SPEED
    }
    public class GeneratedObject
    {
        public GeneratedObject(List<Player> players, Platform platform, Bounds bounds, int iDuration, OBJECT _object)
        {
            this.Bounds = bounds;
            this.Origin = new Origin(new Random().Next(platform.iX, platform.iX + platform.iWidth), platform.iY - (players[0].Bounds.iHeight + bounds.iHeight));
            this.iDuration = iDuration;
            this.Players = players;
            this.Object = _object;
            this.shouldSpawn = true;
        }

        public void Spawn()
        {
            if (!this.shouldSpawn) 
                return;

            this.shouldSpawn = false;


            int timer = 0;
            Task.Run(() =>
            {;
                while (timer < iDuration)
                {
                    Thread.Sleep(5);
                    timer++;
                    Players.ForEach(x =>
                    {
                        if (this.Origin.iX > x.Origin.iX &&
                            this.Origin.iX + this.Bounds.iWidth < x.Origin.iX + x.Bounds.iWidth)
                        {
                            this.shouldSpawn = false;
                            x.HandleGeneratedObject(this);
                            this.Platform.DeleteObject();
                        }
                    });
                }
            });
        }

        public List<Player> Players { get; set; }
        public Platform Platform { get; set; }
        public Bounds Bounds { get; set; }
        public Origin Origin { get; set; }

        public bool shouldSpawn { get; set; }
        public int iDuration { get; set; }
        public OBJECT Object { get; set; }
    }
}
