﻿using HuuugeGame.Behaviour.Hive;
using HuuugeGame.Content.Behaviour;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HuuugeGame
{
    class ComponentGame : StateTemplate, Behaviour.IComponent
    {
        private int counter1, counter2;
        private bool isLoaded = false;
        public List<IEntity> DrawList { get; set; } = new List<IEntity>();
        public List<IEntity> UpdateList { get; set; } = new List<IEntity>();
        public List<Obstacle> ObstacleList { get; set; } = new List<Obstacle>();
        Random rnd = new Random();

        //LOAD OBJECTS AND OTHER STUFF IMPORTANT FOR THE GAMESTATE
        public void OnLoad()
        {
            var hive = new Hive(this, new Vector2(500, 500), 13);

            ObstacleList = GenerateListOfRandomObstacles();

            foreach (IEntity obstacle in ObstacleList)
            {
                DrawList.Add(obstacle);
            }

            DrawList.Add(new Spider(this, new Vector2(32, 32), new Vector2(100, 100), 2));
            DrawList.Add(new Hive(this, new Vector2(500, 500), 10));
            //DrawList.Insert(0, new Flower(this, new Vector2(300,300)));

            //HACK: Do rozróżnienia?
            UpdateList = DrawList;
        }

        //OBLICZENIA
        public void Update()
        {
            Globals.newKeyState = Keyboard.GetState();

            if (!isLoaded)
            {
                OnLoad();
                isLoaded = true;
            }
            if (KeypressTest(Keys.Escape))
            {
                Globals.activeState = Globals.enGameStates.PAUSE;
            }


            for (int i = 0; i < UpdateList.Count(); i++)
            {
                UpdateList[i].Update();
            }
            Draw();


            if (getCountOfChildrenFlies() == 0)

            {
                ResetState();

                Globals.winner = "SPIDER!";
            }
            else if (getCountOfChildrenFlies() > 150)
            {

                ResetState();


                Globals.winner = "BUTTERFLY!";
            }


            //FLOWER
            if (!IfDrawListHasFlower())
            {
                counter2 = 0;
                if (counter1 < 60 * 3)
                    counter1++;
                else
                {
                    DrawList.Insert(20,new Flower(this, GenerateNewPositionForFlower()));

                }
            }
            else
            {
                counter1 = 0;
                if (counter2 < 60 * 8)
                    counter2++;
                else
                {
                    counter2 = 0;
                    DrawList.Remove(DrawList.Find(x => x is Flower));
                    DrawList.Insert(20,new Flower(this, GenerateNewPositionForFlower()));
                }
            }
            Globals.oldKeyState = Globals.newKeyState;

        }
        private bool KeypressTest(Keys theKey)
        {
            if (Globals.oldKeyState.IsKeyUp(theKey) && Globals.newKeyState.IsKeyDown(theKey))
                return true;
            return false;
        }
        private void ResetState()
        {
            isLoaded = false;
            counter1 = 0;
            counter2 = 0;
            DrawList = new List<IEntity>();
            UpdateList = new List<IEntity>();
            ObstacleList = new List<Obstacle>();


            Globals.activeState = Globals.enGameStates.WINSTATE;
        }

        //RYSOWANIE NA EKRANIE
        public void Draw()
        {
            Globals.spriteBatch.Begin();

            Globals.spriteBatch.Draw(Globals.backgroundTexture, new Vector2(0, 0), Color.White);







            foreach (IEntity entity in DrawList)
            {
                entity.Draw();
            }




            List<ChildrenFly> ListOfChildrenFlies = ((Hive)DrawList.Find(x => x is Hive)).ChildrenFlies;


            Globals.spriteBatch.Draw(Globals.hpBar, new Rectangle(30, 2, 100, 22), new Color(0, 0, 0, 150));
            Globals.spriteBatch.DrawString(Globals.defaultFont, "My Hive: " + ListOfChildrenFlies.Count.ToString(), new Vector2(36, 4), Color.White);
            Globals.spriteBatch.End();
        }

        private List<Obstacle> GenerateListOfRandomObstacles()
        {
            List<Obstacle> listOfRandomizedObstacles = new List<Obstacle>();

            Random rnd = new Random();
            int x = 0;
            int y = 0;
            int tex = 0;
            Texture2D texture = null;

            for (int i = 0; i < 20; i++)
            {
                tex = rnd.Next(0, 7);

                switch (tex)
                {
                    case 0:
                        texture = Globals.stone1Texture;
                        break;
                    case 1:
                        texture = Globals.stone2Texture;
                        break;
                    case 2:
                        texture = Globals.stone3Texture;
                        break;
                    case 3:
                        texture = Globals.stone4Texture;
                        break;
                    case 4:
                        texture = Globals.stumpTexture;
                        break;
                    case 5:
                        texture = Globals.holeTexture;
                        break;
                    case 6:
                        texture = Globals.bushTexture;
                        break;
                }

                bool escapeWhileFlag;

                do
                {
                    escapeWhileFlag = false;

                    x = rnd.Next(64, 636 - texture.Width);
                    y = rnd.Next(64, 536 - texture.Height);

                    foreach (var obstacle in listOfRandomizedObstacles)
                    {
                        if (new Rectangle(x + 5, y + 5, texture.Width - 10, texture.Height - 10).Intersects(obstacle.BoundingBox))
                        {
                            escapeWhileFlag = true;
                        }
                    }
                } while (escapeWhileFlag);


                listOfRandomizedObstacles.Add(new Obstacle(this, new Vector2(x, y), texture));
            }

            return listOfRandomizedObstacles;
        }

        private Vector2 GenerateNewPositionForFlower()
        {
            var entities = DrawList.FindAll(x => x is Obstacle).ToList();
            int posX = 0, posY = 0;
            var flowerRectangle = new Rectangle(posX, posY, Globals.flowerTexture.Width, Globals.flowerTexture.Height);



            while (true)
            {
                posX = rnd.Next(24 + Globals.flowerTexture.Width, (int)Globals.screenSize.X - 24 - Globals.flowerTexture.Width);
                posY = rnd.Next(24 + Globals.flowerTexture.Height, (int)Globals.screenSize.Y - 24 - Globals.flowerTexture.Height);

                flowerRectangle = new Rectangle(new Point(posX, posY), flowerRectangle.Size);

                if (entities.All(x => !x.BoundingBox.Intersects(flowerRectangle)))
                    return new Vector2(posX, posY);
            }
        }
        private bool IfDrawListHasFlower()
        {
            return DrawList.Find(x => x is Flower) != null;
        }
        int getCountOfChildrenFlies()
        {
            List<ChildrenFly> ListOfChildrenFlies = ((Hive)DrawList.Find(x => x is Hive)).ChildrenFlies;

            return ListOfChildrenFlies.Count;
        }
    }
}
