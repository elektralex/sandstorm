﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sandstorm.ParticleSystem.structs;

using System.Threading;
using System.Threading.Tasks;

namespace Sandstorm.ParticleSystem.draw
{
    class Instancing
    {
        Texture2D _particlePositions;
        public enum INSTANCE_MODE
        {
            NORMAL,
            DEBUG
        }

        private GraphicsDevice _graphicsDevice = null;
        private ContentManager _contentManager = null;
        private SharedList _sharedList = null;
        
        private DynamicVertexBuffer instanceVertexBuffer = null;
        private Vector3[] _instanceTransforms;        
        private Effect _effect;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;           
        private Texture2D _billboardTexture;

        private static float _BBSize = 25.0f;
        
        private static float _DebugBBSize = 1.0f;
        private static INSTANCE_MODE _state = INSTANCE_MODE.NORMAL;
        private static INSTANCE_MODE _internalstate = INSTANCE_MODE.NORMAL;

        private static VertexDeclaration _instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 1)
        );  
      
        public static void nextMode()
        {
            switch (_state)
            {
                case INSTANCE_MODE.NORMAL:
                    _state = INSTANCE_MODE.DEBUG;
                    break;
                case INSTANCE_MODE.DEBUG:
                    _state = INSTANCE_MODE.NORMAL;
                    break;
            }
        }

        private void LoadParticleInstance()
        {            
            float pSize=0;
            switch (_state)
            {
                case INSTANCE_MODE.DEBUG:
                    pSize = _DebugBBSize;
                    break;
                default:
                    pSize= _BBSize;
                    break;
            }

            List<BillboardVertex> vertices = new List<BillboardVertex>();
            List<short> indices = new List<short>();
            short baseIndex = (short)0;

            BillboardVertex vertex = new BillboardVertex();

            vertex.Position = new Vector4(1f, 1f, 1f, 1f);
            vertex.TexCoord = new Vector4(0.0f, 0.0f, -pSize, pSize);
            vertices.Add(vertex);

            vertex.Position = new Vector4(1, 1, 1, 1);
            vertex.TexCoord = new Vector4(1.0f, 0.0f, pSize, pSize);
            vertices.Add(vertex);

            vertex.Position = new Vector4(1, 1, 1, 0);
            vertex.TexCoord = new Vector4(0.0f, 1.0f, -pSize, -pSize);
            vertices.Add(vertex);

            vertex.Position = new Vector4(1, 1, 1, 0);
            vertex.TexCoord = new Vector4(1.0f, 1.0f, pSize, -pSize);
            vertices.Add(vertex);

            indices.Add((short)(0 + baseIndex));
            indices.Add((short)(1 + baseIndex));
            indices.Add((short)(2 + baseIndex));
            indices.Add((short)(2 + baseIndex));
            indices.Add((short)(1 + baseIndex));
            indices.Add((short)(3 + baseIndex));

            _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(BillboardVertex), vertices.Count, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(vertices.ToArray());

            _indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indices.ToArray());
        }

        public Instancing(GraphicsDevice pGraphicsDevice, ContentManager pContentManager, SharedList pList)
        {
            this._graphicsDevice = pGraphicsDevice;
            this._contentManager = pContentManager;
            this._sharedList = pList;
            _effect = _contentManager.Load<Effect>("fx/particleDrawer");
            _billboardTexture = _contentManager.Load<Texture2D>("tex/smoke"); 

            LoadParticleInstance();
            _particlePositions = new Texture2D(_graphicsDevice, 10, 10, false, SurfaceFormat.Vector4);
        }


        void DrawInstances(Camera pCamera, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, Texture2D pTexture, Vector3[] pInstances)
        {
            if (pInstances.Length == 0)
                return;

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            if ((instanceVertexBuffer == null) || (pInstances.Length > instanceVertexBuffer.VertexCount))
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(_graphicsDevice, _instanceVertexDeclaration,
                                                               10*10, BufferUsage.None);
            }
            
            Vector2[] iVertex = new Vector2[10 * 10]; //Position auf 

            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                {
                    iVertex[x * 10 + y].X = x;
                    iVertex[x * 10 + y].Y = y;
                }

            instanceVertexBuffer.SetData(iVertex, 0, iVertex.Length, SetDataOptions.Discard);

            // Tell the GPU to read from both the model vertex buffer plus our instanceVertexBuffer.
             _graphicsDevice.SetVertexBuffers(
                        new VertexBufferBinding(vertexBuffer,0,0),
                        new VertexBufferBinding(instanceVertexBuffer, 0, 2)
            );

            _graphicsDevice.Indices = indexBuffer;

            // Set up the instance rendering effect.
            _effect.CurrentTechnique = _effect.Techniques["InstancingBB"];



            Vector4[] myVector = new Vector4[10 * 10];

            Random r = new Random();
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                {
                    if (x == 9 && y == 9)
                    {
                        myVector[x * 10 + y].X = (float)r.NextDouble() * 100f;
                        myVector[x * 10 + y].Y = (float)r.NextDouble() * 100f;
                        // myVector[x * 100 + y].Z = 1.0f;
                    }
                    else
                    {
                        myVector[x * 10 + y].X = x*10f;
                        myVector[x * 10 + y].Y = y*10f;
                        //myVector[x * 100 + y].Z = 1.0f;
                    }
                }
            _particlePositions.SetData(myVector);

            _effect.Parameters["world"].SetValue(Matrix.Identity);
            _effect.Parameters["view"].SetValue(pCamera.ViewMatrix);
            _effect.Parameters["projection"].SetValue(pCamera.ProjMatrix);
            _effect.Parameters["Texture"].SetValue(_billboardTexture);
            _effect.Parameters["positionMap"].SetValue(_particlePositions);
            /*_effect.Parameters["alphaTestDirection"].SetValue(1.0f);
            _effect.Parameters["alphaTestThreshold"].SetValue(0.3f);*/
            _effect.Parameters["BBSize"].SetValue((_state == INSTANCE_MODE.DEBUG) ? _DebugBBSize : _BBSize);
            _effect.Parameters["debug"].SetValue((_state == INSTANCE_MODE.DEBUG) ? true : false);
            

            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;


            // Draw all the instance copies in a single call.
            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                               vertexBuffer.VertexCount, 0,
                                                               indexBuffer.IndexCount / 3, 10*10);
            }


        }


        public void Draw(Camera pCamera)
        {
            Array.Resize(ref _instanceTransforms, _sharedList.Count*2);

            if (_internalstate != _state)
            {
                _internalstate = _state;
                LoadParticleInstance();
            }
            //TODO: Parallelisieren?!
            //Parallel.For(0, _sharedList.Particles.Length, i =>
            int k=0;
            for(int i=0; i < _sharedList.Particles.Length;i++)
            {
                Particle p = _sharedList.Particles[i];
                if (p != null)
                {
                    _instanceTransforms[k++] = p.Pos;
                    _instanceTransforms[k++] = p.Force;
                }
            }//);

            DrawInstances(pCamera,_vertexBuffer, _indexBuffer, _billboardTexture, _instanceTransforms);
        }
    }
}
