using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowcaseZero.Mvvm.PageViewModels.MaskView
{
    public class MaskViewPageVm : BasePageVm
    {
        string message = "We all use open-source software. Sponsoring is a way to show a little appreciation, " + // 14
            "and has many benefits, including getting to brag about it at dinner parties. Please consider " + // 15, 29
            "picking one of the libraries you use, or a project you like, and sponsor them. " + // 15, 44
            "It doesn't matter who, and you'll get that warm fuzzy feeling of giving something back. " + // 15, 59
            "And please do brag about it, because that's how we can encourage others to sponsor too. " + // 16, 70
            "Buy a developer a coffee each month, or lunch; if you use their work, they've probably earned it."; // 18

        string[] _allText;

        int _nextIndex = 0;

        private string _targetName;
        private Color _maskColor;
        private Color _maskEdgeColor;
        private float _backgroundAlpha;
        private float _maskRoundness;
        private float _maskEdgeThickness;

        public string TargetName
        {
            get => _targetName;
            set => SetProperty(ref _targetName, value);
        }
        public float BackgroundAlpha
        {
            get => _backgroundAlpha;
            set => SetProperty(ref _backgroundAlpha, value);
        }
        public float MaskRoundness
        {
            get => _maskRoundness;
            set => SetProperty(ref _maskRoundness, value);
        }

        public float MaskEdgeThickness
        {
            get => _maskEdgeThickness;
            set => SetProperty(ref _maskEdgeThickness, value);
        }

        public Color MaskColor
        {
            get => _maskColor;
            set => SetProperty(ref _maskColor, value);
        }
        public Color MaskEdgeColor
        {
            get => _maskEdgeColor;
            set => SetProperty(ref _maskEdgeColor, value);
        }

        public MaskViewPageVm()
        {
            _allText = message.Split(' ');
            _ = DoTheThingAsync();
        }

        public string NextText => GetNextText();

        string GetNextText()
        {
            var retval = _allText[_nextIndex];

            _nextIndex = (_nextIndex + 1) % _allText.Length;

            return retval;
        }

        private async Task DoTheThingAsync()
        {
            int hb2y1 = 800;
            int hb2y2 = 500;

            while (true)
            {
                await Task.Delay(5000);

                await WhoDoYouSponsorAsync(hb2y1, Colors.Black);
                TargetName = "";
                await Task.Delay(1500);

                //await WhoDoYouSponsorAsync(hb2y1, Colors.Blue);
                //TargetName = "";
                //await HappyBirthdayDearJay(hb2y1, Colors.Red);
                //TargetName = "";
                //await Task.Delay(1100);
                //await WhoDoYouSponsorAsync(hb2y2, Colors.Green);
                //TargetName = "";

            }
        }

        private async Task HappyBirthdayDearJay(int hb2y, Color edgeColor)
        {
            await Task.Delay(hb2y);
            TargetName = "Happy";
            BackgroundAlpha = 0.5F;
            MaskRoundness = 1.0f;
            MaskEdgeColor = edgeColor;
            MaskColor = Colors.Yellow;
            MaskEdgeThickness = 5f;
            await Task.Delay(hb2y);

            TargetName = "Birthday";
            BackgroundAlpha = 0.5F;
            MaskRoundness = 1.0f;
            MaskEdgeColor = edgeColor;
            MaskColor = Colors.Yellow;
            MaskEdgeThickness = 5f;
            await Task.Delay(hb2y);

            TargetName = "Dear";
            BackgroundAlpha = 0.5F;
            MaskRoundness = 1.0f;
            MaskEdgeColor = edgeColor;
            MaskColor = Colors.Yellow;
            MaskEdgeThickness = 5f;
            await Task.Delay(hb2y);

            TargetName = "Jay";
            BackgroundAlpha = 0.5F;
            MaskRoundness = 1.0f;
            MaskEdgeColor = edgeColor;
            MaskColor = Colors.Yellow;
            MaskEdgeThickness = 5f;
            await Task.Delay(hb2y);








            //BackgroundAlpha = 0.8F;
            //await Task.Delay(1000);

            BackgroundAlpha = 1F;
            MaskRoundness = 0.0f;
            await Task.Delay(1000);

            MaskEdgeThickness = 15f;
            await Task.Delay(1000);
            MaskEdgeThickness = 5f;



            BackgroundAlpha = 0.5F;
            MaskRoundness = 1.0f;
            await Task.Delay(1000);




        }

        private async Task WhoDoYouSponsorAsync(int hb2y, Color edgeColor)
        {
            TargetName = "who";
            BackgroundAlpha = 0.5F;
            MaskRoundness = 1.0f;
            MaskEdgeColor = edgeColor;
            MaskColor = Colors.Yellow;
            MaskEdgeThickness = 5f;
            await Task.Delay(hb2y);

            TargetName = "do";
            BackgroundAlpha = 0.5F;
            MaskRoundness = 1.0f;
            MaskEdgeColor = edgeColor;
            MaskColor = Colors.Yellow;
            MaskEdgeThickness = 5f;
            await Task.Delay(hb2y);

            TargetName = "you";
            BackgroundAlpha = 0.5F;
            MaskRoundness = 1.0f;
            //MaskEdgeColor = edgeColor;
            MaskColor = Colors.Yellow;
            MaskEdgeThickness = 5f;
            await Task.Delay(800);

            //
            BackgroundAlpha = 1F;
            MaskRoundness = 0.0f;
            await Task.Delay(800);

            MaskEdgeThickness = 15f;
            await Task.Delay(800);
            MaskEdgeColor = Colors.Red;
            await Task.Delay(800);
            MaskEdgeThickness = 5f;



            BackgroundAlpha = 0.5F;
            MaskRoundness = 1.0f;
            MaskEdgeColor = edgeColor;
            await Task.Delay(800);

            //

            TargetName = "sponsor";
            BackgroundAlpha = 0.5F;
            MaskRoundness = 1.0f;
            MaskEdgeColor = edgeColor;
            MaskColor = Colors.Yellow;
            MaskEdgeThickness = 5f;
            await Task.Delay(hb2y);
            await Task.Delay(hb2y);
        }
    }
}
