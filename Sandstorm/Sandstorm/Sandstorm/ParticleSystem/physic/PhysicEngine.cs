﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Sandstorm.ParticleSystem.structs;
using Sandstorm.Terrain;

namespace Sandstorm.ParticleSystem.physic
{
    class PhysicEngine
    {
        private SharedList _sharedList = null;
        private FPSCounter _fpsCounter = new FPSCounter();

        private List<Vector3> _forces = new List<Vector3>();

        private HeightMap _heightMap;

        private CollisionDetector _collisionDetector;

        public PhysicEngine(SharedList pList, HeightMap heightMap)
        {
            this._sharedList = pList;
            this._heightMap = heightMap;
            _collisionDetector = new CollisionDetector(pList, _heightMap);

            this._forces.Add(new Vector3(0f, -0.03f, 0f));
            this._forces.Add(new Vector3(0f, -0.0f, 0.05f));
        }

        public void Update(GameTime pGameTime) //Update physic
        {

            Parallel.ForEach(_sharedList.getParticles(), p =>
            {
                
                if (p != null)
                {

                    foreach (Vector3 f in _forces)//apply external forces (Gravitation etc)
                    {
                        p.applyExternalForce(f);
                    }
                    p.move();//move the Particle
                    //check colision
                    Vector3 nextPosition = p.Pos;// +p.getForce();
                    float pHeight = _heightMap.getHeightData(nextPosition.X, nextPosition.Z);

                    if (nextPosition.Y < pHeight + p.Radius)
                    {
                        Vector3 normal = _heightMap.getNormal(nextPosition.X, nextPosition.Z);
                        p.reflect(normal);
                        Vector3 f= p.Force;
                        f.Y = 0;
                        p.Force = f;
                        p.Pos = new Vector3(p.Pos.X, pHeight, p.Pos.Z) + (/*p.getRadius() **/ 1.005f * normal);
                    
                    
                        p.applyFriction(0.1f);
                    }


                    //p.collide(_sharedList.getParticles().ToArray());
                }
            });
            _collisionDetector.checkCollisions();
        }

        public void Draw(Camera pCamera) //Nothing to draw.. normally
        {
            _fpsCounter.Update();
        }

        public int getFPS()
        {
            return _fpsCounter.getFrames();
        }
    }
}
