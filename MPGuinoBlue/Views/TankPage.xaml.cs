using MPGuinoBlue.ViewModels;
using Shiny.BluetoothLE.Central;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Collections.Generic;
using Xamarin.Forms;


namespace MPGuinoBlue.Views
{
    public partial class TankPage : ContentPage
    {

        public static float num { get; set; }
        public static int previous;
        public static decimal lasttt;
        public static int tick;
        public static decimal highest;
        public static decimal multt = 0;


        public static Queue<decimal> mystack = new Queue<decimal>();

        public TankPage(IPeripheral peripheral)
        {

            // mystack.Enqueue(10);

            InitializeComponent();
            BindingContext = new InfoPageViewModel(peripheral);
            //PrintPageViewModel numberz = new InfoPageViewModel(peripheral);
            // num = numberz.Tinstant;




        }


        public static void OnPainting(object sender, SKPaintSurfaceEventArgs e)
        {
            var surface = e.Surface;
            // then we get the canvas that we can draw on
            var canvas = surface.Canvas;
            // clear the canvas / view
            //var scale = e.Info.Width /(float)Width;
            //canvas.Scale(scale);
            canvas.Clear(SKColors.Black);
            var circleFill = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = SKColors.Blue
            };
            var pathStroke = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Green,
                StrokeWidth = 6
            };
            var maxStroke = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                StrokeWidth = 2
            };
            var backStroke = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.DarkGray,
                StrokeWidth = 2
            };
            var textPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = SKColors.DarkGray,
                TextSize = 26
            };

            var pathStrokeRed = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                StrokeWidth = 6
            };
            decimal max = InfoPageViewModel.Ttrip1001;
            if (max <= 1) max = 10;
            if (max >= 100) max = 100;

            //max = max ;
            decimal mult = 500 / (max + (max / 2));
            if (highest >= (max + (max / 2)))
            {
                mult = 500 / highest;
                if (highest >= max * 3) mult = 500 / (max * 3);
            }
            decimal deviation = 0.1M;
            if (multt >= mult) multt = multt - deviation;
            if (multt <= mult) multt = multt + deviation;
            if (multt >= (500 / (max + (max / 2)))) multt = 500 / (max + (max / 2));
            if (multt <= (500 / (max * 3))) multt = 500 / (max * 3);
            int graph = 0;

            //if ((int)(InfoPageViewModel.Tinstant1 * 20) != (int)(lasttt * 20))

            if (tick == 1)
            {
                mystack.Enqueue(InfoPageViewModel.Tinstant1);
                tick = 0;
            }


            for (int i = 0; i <= (500 / (10 * multt)); i++)
            {
                string stringg = (i * 10).ToString();
                if ((500 - (i * 10 * multt)) <= 490) canvas.DrawText(stringg, 20, (int)(500 - (i * 10 * multt)) + 25, textPaint);
                //poop
                canvas.DrawLine(0, (int)(500 - (i * 10 * multt)), 1080, (int)(500 - (i * 10 * multt)), backStroke);
            }

            canvas.DrawLine(0, (int)(500 - (max * multt)), 1080, (int)(500 - (max * multt)), maxStroke);

            highest = 0;

            //make max and multi scale to largest graph point or trip100
            foreach (decimal dot in mystack)
            {
                int draw = 500 - (int)(dot * multt);
                if (dot >= highest) highest = dot;
                graph = graph + 10;
                if (dot >= (max + 2)) if (graph >= 20) canvas.DrawLine(graph, draw, graph - 10, previous, pathStrokeRed);
                if (dot <= (max + 2)) if (graph >= 20) canvas.DrawLine(graph, draw, graph - 10, previous, pathStroke);
                previous = draw;
                decimal lasttt = dot;
            }
            if (graph >= 1050) mystack.Dequeue();

            ((SKCanvasView)sender).InvalidateSurface();




        }

    }
}
