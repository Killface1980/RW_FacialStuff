using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    public static class GraphicDatabaseModded
    {
        private static Dictionary<GraphicRequest, GraphicModded> allGraphics = new Dictionary<GraphicRequest, GraphicModded>();

        public static GraphicModded Get<T>(string path, Shader shader, Vector2 drawSize, Color color) where T : GraphicModded, new()
        {
            GraphicRequest req = new GraphicRequest(typeof(T), path, shader, drawSize, color, Color.white, null);
            return GetInner<T>(req);
        }

        private static T GetInner<T>(GraphicRequest req) where T : GraphicModded, new()
        {
            GraphicModded graphic;
            if (!allGraphics.TryGetValue(req, out graphic))
            {
                graphic = Activator.CreateInstance<T>();
                graphic.Init(req);
                allGraphics.Add(req, graphic);
            }
            return (T)((object)graphic);
        }
    }
}
