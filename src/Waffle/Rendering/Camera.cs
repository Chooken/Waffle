using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine
{
    public struct Camera
    {
        private Camera3D _camera;

        public Camera(float x, float y, float rotation)
        {
            _camera = new Camera3D(new Vector3(x, y, 10), new Vector3(x,y,0), new Vector3(0,1,0), 8f, CameraProjection.Orthographic);
        }

        public void SetPosition(float x, float y)
        {
            _camera.Position.X = x;
            

            _camera.Target.X = x;
            _camera.Target.Y = y;
        }

        public void Move(float x, float y)
        {
            _camera.Position.X += x;
            _camera.Position.Y += y;

            //_camera.Target.X = _camera.Position.X;
            //_camera.Target.Y = _camera.Position.Y;
        }

        public static implicit operator Camera3D(Camera camera) => camera._camera;
    }
}
