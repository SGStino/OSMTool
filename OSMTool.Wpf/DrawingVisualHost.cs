using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace OSMTool.Wpf
{
    public enum DrawingLayer
    {
        Outlines = 0,
        Ways = 1,
        Nodes = 2,
        Markers = 3
    }

    public class DrawingVisualHost : FrameworkElement
    { 
        public DrawingContext GetLayer(DrawingLayer layer) => contexts[(int)layer];

        private DrawingContext[] contexts;
        private VisualCollection children;

        public DrawingVisualHost()
        {
            children = new VisualCollection(this);

            int count = Enum.GetValues(typeof(DrawingLayer)).Cast<int>().Max()+1;
            for (int i = 0; i < count; i++)
                children.Add(new DrawingVisual()
                { 
                   // CacheMode = new BitmapCache()
                }); 
            contexts = new DrawingContext[count];
        }


        protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
        {
            return base.HitTestCore(hitTestParameters);
        }
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return base.HitTestCore(hitTestParameters);
        }


        public void OpenLayers()
        {
            for (int i = 0; i < contexts.Length; i++)
                contexts[i] = (children[i] as DrawingVisual).RenderOpen();
        }

        public void CloseLayers()
        {
            for (int i = 0; i < contexts.Length; i++)
            {
                contexts[i]?.Close();
                contexts[i] = null; 
            }


        }

        protected override int VisualChildrenCount
        {
            get { return children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return children[index];
        }

    }
}
