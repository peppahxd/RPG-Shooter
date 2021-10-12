using Microsoft.AspNetCore.Http;
using Microsoft.Web.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RPG_Shooter.Models
{

    public class MessageEventArgs : EventArgs
    {
        public Player Player { get; set; }
        public string sMessage { get; set; }
        public MessageEventArgs(Player player, string sMessage)
        {
            this.Player = player;
            this.sMessage = sMessage;
        }
    }

    public class SocketEventArgs : EventArgs
    {
        public WebSocket socket { get; set; }
        public SocketEventArgs(WebSocket socket)
        {
            this.socket = socket;
        }
    }
    public class Server
    {
        public List<Player> players { get; set; }
        public List<Platform> platforms { get; set; }
        public int idCounter { get; set; }
        public Server()
        {
            this.players = new List<Player>();
            this.platforms = new List<Platform>();
            this.idCounter = 0;



            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(5);
                    this.players.ForEach(x =>
                    {
                        x.Run(platforms);
                    });
                }
            });

        }

        public async Task AddSocket(WebSocket socket)
        {
            Player player = new Player(idCounter, socket);
            this.players.Add(player);

            this.idCounter++;
            await Handle(player);

        }

        public async Task CreatePlatforms(string msg)
        {
            platforms = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Platform>>(msg);
            platforms.ForEach(async x =>
            {
                x.Server = this;
                
                GeneratedObject obj =  x.GenerateObject(players);
                if (obj != null)
                    await SendMessage(null, "NEWOBJECT&" + obj.Origin.iX + "&" + obj.Origin.iY + "&" + obj.Bounds.iWidth + "&" + obj.Bounds.iHeight, true);
            });
        }


        public async Task OnMessage(MessageEventArgs args)
        {
            var msg = args.sMessage.Split('&');
            switch (msg[0])
            {
                case "REQ":
                    {
                        await SendMessage(args.Player, JsonSerializer.Serialize(this.players));
                        break;
                    }
                case "FIXPOS":
                    {
                        var player = this.players.FirstOrDefault(x => x.id == Convert.ToInt32(msg[1]));
                        player.Origin = new Origin(int.Parse(msg[2]), int.Parse(msg[3]));
                        player.OriginalOrigin = player.Origin;
                        break;
                    };
                case "MOVE":
                    {
                        Console.WriteLine(args.Player.Velocity.xSpeed);
                        switch (msg[1])
                        {
                            case "RIGHT":
                                {
                                    args.Player.isMovingLeft = false;
                                    args.Player.isMovingRight = true;
                                    break;
                                }

                            case "LEFT":
                                {
                                    args.Player.isMovingLeft = true;
                                    args.Player.isMovingRight = false;
                                    break;
                                }
                            case "DOWN":
                                {
                                    args.Player.isCrouching = true;
                                    break;
                                }
                            case "JUMP":  args.Player.Jump.StartJump(); break;
                            case "STOP":  args.Player.isStopping = true; break;
                        }
                        Console.WriteLine(args.Player.Velocity.ySpeed);

                        break;
                    }
                case "RESPAWN":
                    {
                        Console.WriteLine("respawned");

                        args.Player.Origin = args.Player.OriginalOrigin;
                        args.Player.iHealth = 100;
                        args.Player.isDead = false;
                        break;
                    }
                case "RPG":
                    {
                        Bullet bullet = new Bullet(args.Player);
                        args.Player.Bullets.Add(bullet);

                        _ = Task.Run(() =>
                          {
                              while (bullet.Origin.iX > -1000 && bullet.Origin.iX < 2000)
                              {
                                  Thread.Sleep(1);
                                  bullet.Fire(args.Player);

                                  IMPACT impact = bullet.HandleImpact(players);
                                  if (impact == IMPACT.HIT)
                                  {
                                      args.Player.Bullets.Remove(bullet);
                                      break;
                                  }

                                  if (impact == IMPACT.FATALHIT)
                                  {
                                      players.ForEach(async x =>
                                      {
                                          if (!x.Equals(args.Player))
                                              await SendMessage(x, "DEAD");
                                      });
                                      
                                      break;
                                  }
                              }

                          });
                        break;
                    }
                case "ID":
                    {
                        await SendMessage(args.Player, "ID:" + args.Player.id.ToString());
                        break;
                    }

            }
        }


        public async Task SendMessage(Player player, string msg, bool broadcast = false)
        {
            var buffer = new byte[1024];
            buffer = Encoding.ASCII.GetBytes(msg);
            
            if (!broadcast)
                await player.socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            else
            {
                players.ForEach(async x =>
                {
                    await x.socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                });
            }
        }


        public async Task Handle(Player player)
        {
            var buffer = new byte[1024];
            WebSocketReceiveResult result = await player.socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            var _platforms = Encoding.ASCII.GetString(buffer);
            if (_platforms.Contains("canvas"))
                await this.CreatePlatforms(_platforms);

            while (!result.CloseStatus.HasValue)
            {
                await player.socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                buffer = new byte[1024];
                result = await player.socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                await this.OnMessage(new MessageEventArgs(player, Encoding.ASCII.GetString(buffer)));


            }
            await player.socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
