namespace FunctionZero.Maui.Controls
{
    public class MaskViewZero : GraphicsView, IDrawable
    {
        private const int CurveSubdivisionCount = 16;

        private float _x, _y, _w, _h;
        private float _radius;
        private float _alpha;
        private Color _fillColor;
        private Color _strokeColor;
        private float _strokeThickness;
        private float _alphaMultiplier;
        private readonly Dictionary<string, PreparedShape> _shapeCache = new(StringComparer.Ordinal);
        private PreparedShape _currentShape;
        private PreparedShape _destinationShape;
        private MorphShape _activeMorph;
        private float _shapeProgress = 1;
        private string _targetPathData;
        private bool _targetPathDataInitialized;
        private bool _destinationIsDefaultShape;

        public MaskViewZero()
        {
            _x = 100; _y = 100; _w = 200; _h = 70; _radius = 0.0F;
        }
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = _strokeColor;
            canvas.StrokeSize = _strokeThickness;

            PathF path = new PathF();
            var customShape = GetCurrentShape();
            PathF customOutline = null;

            if (customShape == null)
            {
                path.AppendRoundedRectangle(_x, _y, _w, _h, _radius);
            }
            else
            {
                AppendFittedShape(path, customShape);
                customOutline = new PathF();
                AppendFittedShape(customOutline, customShape);
            }

            var width = dirtyRect.Width;
            var height = dirtyRect.Height;

            // Draw a box around the edges of the control.
            path.MoveTo(0, 0);
            path.LineTo(width, 0);
            path.LineTo(width, height);
            path.LineTo(0, height);
            path.LineTo(0, 0);

            path.Close();

            canvas.StrokeSize = _strokeThickness;
            canvas.StrokeLineJoin = LineJoin.Round;
            canvas.StrokeColor = _strokeColor;
            canvas.FillColor = _fillColor;
            canvas.Alpha = _alpha * _alphaMultiplier;
            canvas.FillPath(path, WindingMode.EvenOdd);
            canvas.Alpha = _alphaMultiplier;

            if (customOutline == null)
                canvas.DrawRoundedRectangle(_x - _strokeThickness/2, _y - _strokeThickness/2, _w + _strokeThickness/2 + _strokeThickness/2, _h + _strokeThickness/2 + _strokeThickness/2, _radius);
            else
                canvas.DrawPath(customOutline);
        }

        public void Update(double x, double y, double w, double h, double roundness, double backgroundAlpha, Color fillColor, Color strokeColor, double strokeThickness, double alphaMultiplier)
        {
            _x = (float)x;
            _y = (float)y;
            _w = (float)w;
            _h = (float)h;
            _radius = (float)roundness * Math.Min(_w, _h) / 2.0F;
            _alpha = (float)backgroundAlpha;
            _fillColor = fillColor;
            _strokeColor = strokeColor;
            _strokeThickness = (float)strokeThickness;
            _alphaMultiplier = (float)alphaMultiplier;
        }

        public bool BeginShapeTransition(string pathData, double roundness)
        {
            if (_targetPathDataInitialized && string.Equals(pathData, _targetPathData, StringComparison.Ordinal))
                return false;

            var source = GetCurrentShape() ?? CreateRoundedRectangleShape(roundness);
            var destination = PrepareShape(pathData);
            _destinationIsDefaultShape = destination == null;
            destination ??= CreateRoundedRectangleShape(roundness);
            _targetPathData = pathData;
            _targetPathDataInitialized = true;

            _currentShape = source;
            _destinationShape = destination;
            _activeMorph = CreateMorph(source, destination);
            _shapeProgress = 0;
            return true;
        }

        public void SetShapeProgress(float progress)
        {
            _shapeProgress = Math.Clamp(progress, 0, 1);

            if (_shapeProgress >= 1 && _activeMorph != null)
            {
                _currentShape = _destinationIsDefaultShape ? null : _destinationShape;
                _activeMorph = null;
            }
        }

        private static PreparedShape CreateRoundedRectangleShape(double roundness)
        {
            const int subdivisionsPerCorner = 8;
            var radius = (float)Math.Clamp(roundness, 0, 1) / 2;
            var contour = new List<PointF>(subdivisionsPerCorner * 4);

            AddCorner(contour, 1 - radius, radius, radius, -90, 0, subdivisionsPerCorner);
            AddCorner(contour, 1 - radius, 1 - radius, radius, 0, 90, subdivisionsPerCorner);
            AddCorner(contour, radius, 1 - radius, radius, 90, 180, subdivisionsPerCorner);
            AddCorner(contour, radius, radius, radius, 180, 270, subdivisionsPerCorner);

            if (radius == 0)
                contour = [new PointF(0, 0), new PointF(1, 0), new PointF(1, 1), new PointF(0, 1)];

            return new PreparedShape([contour]);
        }

        private static void AddCorner(List<PointF> contour, float centerX, float centerY, float radius, float startAngle, float endAngle, int subdivisions)
        {
            for (var index = 0; index < subdivisions; index++)
            {
                var progress = index / (float)(subdivisions - 1);
                var angle = (startAngle + (endAngle - startAngle) * progress) * MathF.PI / 180;
                AddPoint(contour, new PointF(centerX + MathF.Cos(angle) * radius, centerY + MathF.Sin(angle) * radius));
            }
        }

        private PreparedShape GetCurrentShape()
        {
            if (_activeMorph == null)
                return _currentShape;

            var contours = _activeMorph.Contours
                .Select(contour => contour.Source
                    .Select((point, index) => Lerp(point, contour.Destination[index], _shapeProgress))
                    .ToList())
                .ToList();

            return new PreparedShape(contours);
        }

        private void AppendFittedShape(PathF path, PreparedShape shape)
        {
            var minX = shape.Contours.Min(contour => contour.Min(point => point.X));
            var minY = shape.Contours.Min(contour => contour.Min(point => point.Y));
            var maxX = shape.Contours.Max(contour => contour.Max(point => point.X));
            var maxY = shape.Contours.Max(contour => contour.Max(point => point.Y));
            var shapeWidth = maxX - minX;
            var shapeHeight = maxY - minY;

            if (shapeWidth <= 0 || shapeHeight <= 0 || _w <= 0 || _h <= 0)
                return;

            var scale = Math.Min(_w / shapeWidth, _h / shapeHeight);
            var offsetX = _x + (_w - shapeWidth * scale) / 2 - minX * scale;
            var offsetY = _y + (_h - shapeHeight * scale) / 2 - minY * scale;

            foreach (var contour in shape.Contours)
            {
                if (contour.Count < 3)
                    continue;

                path.MoveTo(offsetX + contour[0].X * scale, offsetY + contour[0].Y * scale);
                for (var index = 1; index < contour.Count; index++)
                    path.LineTo(offsetX + contour[index].X * scale, offsetY + contour[index].Y * scale);
                path.Close();
            }
        }

        private PreparedShape PrepareShape(string pathData)
        {
            if (string.IsNullOrWhiteSpace(pathData))
                return null;

            if (_shapeCache.TryGetValue(pathData, out var cachedShape))
                return cachedShape;

            PreparedShape preparedShape = null;

            try
            {
                var path = Microsoft.Maui.Graphics.PathBuilder.Build(pathData);
                var contours = Flatten(path);
                preparedShape = Normalize(contours);
            }
            catch
            {
            }

            _shapeCache[pathData] = preparedShape;
            return preparedShape;
        }

        private static List<List<PointF>> Flatten(PathF path)
        {
            var contours = new List<List<PointF>>();
            List<PointF> currentContour = null;
            var currentPoint = default(PointF);
            var segmentTypes = path.SegmentTypes.ToArray();

            for (var segmentIndex = 0; segmentIndex < path.OperationCount; segmentIndex++)
            {
                var operation = segmentTypes[segmentIndex];
                var points = path.GetPointsForSegment(segmentIndex) ?? [];

                switch (operation)
                {
                    case PathOperation.Move:
                        AddContour(contours, currentContour);
                        currentContour = new List<PointF>();
                        if (points.Length > 0)
                        {
                            currentPoint = points[^1];
                            AddPoint(currentContour, currentPoint);
                        }
                        break;

                    case PathOperation.Line:
                        if (currentContour != null && points.Length > 0)
                        {
                            currentPoint = points[^1];
                            AddPoint(currentContour, currentPoint);
                        }
                        break;

                    case PathOperation.Quad:
                        if (currentContour != null && points.Length >= 2)
                        {
                            var start = currentPoint;
                            var control = points[^2];
                            var end = points[^1];
                            for (var index = 1; index <= CurveSubdivisionCount; index++)
                                AddPoint(currentContour, EvaluateQuadratic(start, control, end, index / (float)CurveSubdivisionCount));
                            currentPoint = end;
                        }
                        break;

                    case PathOperation.Cubic:
                        if (currentContour != null && points.Length >= 3)
                        {
                            var start = currentPoint;
                            var control1 = points[^3];
                            var control2 = points[^2];
                            var end = points[^1];
                            for (var index = 1; index <= CurveSubdivisionCount; index++)
                                AddPoint(currentContour, EvaluateCubic(start, control1, control2, end, index / (float)CurveSubdivisionCount));
                            currentPoint = end;
                        }
                        break;

                    case PathOperation.Arc:
                        if (currentContour != null && points.Length > 0)
                        {
                            currentPoint = points[^1];
                            AddPoint(currentContour, currentPoint);
                        }
                        break;

                    case PathOperation.Close:
                        AddContour(contours, currentContour);
                        currentContour = null;
                        break;
                }
            }

            AddContour(contours, currentContour);
            return contours;
        }

        private static void AddContour(List<List<PointF>> contours, List<PointF> contour)
        {
            if (contour == null || contour.Count < 3)
                return;

            if (DistanceSquared(contour[0], contour[^1]) < 0.000001F)
                contour.RemoveAt(contour.Count - 1);

            if (contour.Count >= 3)
                contours.Add(contour);
        }

        private static void AddPoint(List<PointF> contour, PointF point)
        {
            if (contour.Count == 0 || DistanceSquared(contour[^1], point) >= 0.000001F)
                contour.Add(point);
        }

        private static PreparedShape Normalize(List<List<PointF>> contours)
        {
            if (contours.Count == 0)
                return null;

            var minX = contours.Min(contour => contour.Min(point => point.X));
            var minY = contours.Min(contour => contour.Min(point => point.Y));
            var maxX = contours.Max(contour => contour.Max(point => point.X));
            var maxY = contours.Max(contour => contour.Max(point => point.Y));
            var width = maxX - minX;
            var height = maxY - minY;
            var scale = Math.Max(width, height);

            if (scale <= 0 || !float.IsFinite(scale))
                return null;

            var normalizedContours = contours
                .Select(contour => contour.Select(point => new PointF((point.X - minX) / scale, (point.Y - minY) / scale)).ToList())
                .ToList();

            return new PreparedShape(normalizedContours);
        }

        private static MorphShape CreateMorph(PreparedShape source, PreparedShape destination)
        {
            var morphContours = new List<MorphContour>();
            var unusedSourceIndices = Enumerable.Range(0, source.Contours.Count).ToList();

            foreach (var destinationContour in destination.Contours)
            {
                var destinationCenter = GetCentroid(destinationContour);
                var sourceIndex = FindNearestContour(source.Contours, unusedSourceIndices, destinationCenter);
                List<PointF> sourceContour;

                if (sourceIndex >= 0)
                {
                    sourceContour = source.Contours[sourceIndex];
                    unusedSourceIndices.Remove(sourceIndex);
                }
                else
                {
                    sourceContour = CreateCollapsedContour(destinationCenter, destinationContour.Count);
                }

                morphContours.Add(CreateMorphContour(sourceContour, destinationContour));
            }

            foreach (var sourceIndex in unusedSourceIndices)
            {
                var sourceContour = source.Contours[sourceIndex];
                var collapsedDestination = CreateCollapsedContour(GetCentroid(sourceContour), sourceContour.Count);
                morphContours.Add(CreateMorphContour(sourceContour, collapsedDestination));
            }

            return new MorphShape(morphContours);
        }

        private static MorphContour CreateMorphContour(List<PointF> source, List<PointF> destination)
        {
            var pointCount = Math.Clamp(Math.Max(32, Math.Max(source.Count, destination.Count)), 32, 256);
            var resampledSource = ResampleClosedContour(source, pointCount);
            var resampledDestination = ResampleClosedContour(destination, pointCount);
            resampledDestination = AlignContour(resampledSource, resampledDestination);
            return new MorphContour(resampledSource, resampledDestination);
        }

        private static int FindNearestContour(List<List<PointF>> contours, List<int> candidates, PointF center)
        {
            var bestIndex = -1;
            var bestDistance = float.MaxValue;

            foreach (var candidate in candidates)
            {
                var distance = DistanceSquared(GetCentroid(contours[candidate]), center);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = candidate;
                }
            }

            return bestIndex;
        }

        private static List<PointF> ResampleClosedContour(List<PointF> contour, int pointCount)
        {
            if (contour.Count == 1)
                return Enumerable.Repeat(contour[0], pointCount).ToList();

            var cumulativeLengths = new float[contour.Count + 1];
            for (var index = 0; index < contour.Count; index++)
                cumulativeLengths[index + 1] = cumulativeLengths[index] + MathF.Sqrt(DistanceSquared(contour[index], contour[(index + 1) % contour.Count]));

            var perimeter = cumulativeLengths[^1];
            if (perimeter <= 0)
                return Enumerable.Repeat(contour[0], pointCount).ToList();

            var result = new List<PointF>(pointCount);
            var segmentIndex = 0;

            for (var pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                var targetLength = perimeter * pointIndex / pointCount;
                while (segmentIndex < contour.Count - 1 && cumulativeLengths[segmentIndex + 1] < targetLength)
                    segmentIndex++;

                var segmentStartLength = cumulativeLengths[segmentIndex];
                var segmentLength = cumulativeLengths[segmentIndex + 1] - segmentStartLength;
                var progress = segmentLength <= 0 ? 0 : (targetLength - segmentStartLength) / segmentLength;
                result.Add(Lerp(contour[segmentIndex], contour[(segmentIndex + 1) % contour.Count], progress));
            }

            return result;
        }

        private static List<PointF> AlignContour(List<PointF> source, List<PointF> destination)
        {
            var bestCost = double.MaxValue;
            var bestOffset = 0;
            var reverse = false;

            for (var direction = 0; direction < 2; direction++)
            {
                for (var offset = 0; offset < destination.Count; offset++)
                {
                    double cost = 0;
                    for (var index = 0; index < source.Count; index++)
                    {
                        var destinationIndex = direction == 0
                            ? (index + offset) % destination.Count
                            : (offset - index + destination.Count) % destination.Count;
                        cost += DistanceSquared(source[index], destination[destinationIndex]);
                    }

                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        bestOffset = offset;
                        reverse = direction == 1;
                    }
                }
            }

            var result = new List<PointF>(destination.Count);
            for (var index = 0; index < destination.Count; index++)
            {
                var destinationIndex = reverse
                    ? (bestOffset - index + destination.Count) % destination.Count
                    : (index + bestOffset) % destination.Count;
                result.Add(destination[destinationIndex]);
            }

            return result;
        }

        private static List<PointF> CreateCollapsedContour(PointF center, int pointCount)
        {
            return Enumerable.Repeat(center, Math.Max(1, pointCount)).ToList();
        }

        private static PointF GetCentroid(List<PointF> contour)
        {
            return new PointF(contour.Average(point => point.X), contour.Average(point => point.Y));
        }

        private static PointF Lerp(PointF start, PointF end, float progress)
        {
            return new PointF(start.X + (end.X - start.X) * progress, start.Y + (end.Y - start.Y) * progress);
        }

        private static PointF EvaluateQuadratic(PointF start, PointF control, PointF end, float progress)
        {
            var inverse = 1 - progress;
            return new PointF(
                inverse * inverse * start.X + 2 * inverse * progress * control.X + progress * progress * end.X,
                inverse * inverse * start.Y + 2 * inverse * progress * control.Y + progress * progress * end.Y);
        }

        private static PointF EvaluateCubic(PointF start, PointF control1, PointF control2, PointF end, float progress)
        {
            var inverse = 1 - progress;
            return new PointF(
                inverse * inverse * inverse * start.X + 3 * inverse * inverse * progress * control1.X + 3 * inverse * progress * progress * control2.X + progress * progress * progress * end.X,
                inverse * inverse * inverse * start.Y + 3 * inverse * inverse * progress * control1.Y + 3 * inverse * progress * progress * control2.Y + progress * progress * progress * end.Y);
        }

        private static float DistanceSquared(PointF first, PointF second)
        {
            var deltaX = second.X - first.X;
            var deltaY = second.Y - first.Y;
            return deltaX * deltaX + deltaY * deltaY;
        }

        private sealed class PreparedShape
        {
            public PreparedShape(List<List<PointF>> contours)
            {
                Contours = contours;
            }

            public List<List<PointF>> Contours { get; }
        }

        private sealed class MorphShape
        {
            public MorphShape(List<MorphContour> contours)
            {
                Contours = contours;
            }

            public List<MorphContour> Contours { get; }
        }

        private sealed class MorphContour
        {
            public MorphContour(List<PointF> source, List<PointF> destination)
            {
                Source = source;
                Destination = destination;
            }

            public List<PointF> Source { get; }
            public List<PointF> Destination { get; }
        }
    }
}
