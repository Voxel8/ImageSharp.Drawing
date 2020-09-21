using System;
using System.Linq;
using SixLabors.ImageSharp.Drawing.Shapes.Scan;
using SixLabors.ImageSharp.Memory;
using Xunit;

namespace SixLabors.ImageSharp.Drawing.Tests.Shapes.Scan
{
    public class TessellatedComplexPolygonTests
    {
        private static MemoryAllocator MemoryAllocator => Configuration.Default.MemoryAllocator;
        
        private static readonly TolerantComparer Comparer = new TolerantComparer(0.001f);
        
        private static void VerifyRing(TessellatedMultipolygon.Ring ring, PointF[] originalPoints, bool originalPositive, bool isHole)
        {
            ReadOnlySpan<PointF> points = ring.Points;
            
            Assert.Equal(originalPoints.Length + 1, points.Length);
            Assert.Equal(points[0], points[points.Length - 1]);
            
            originalPoints = originalPoints.CloneArray();

            
            if (originalPositive && isHole || !originalPositive && !isHole)
            {
                originalPoints.AsSpan().Reverse();
                points = points.Slice(1);
            }
            else
            {
                points = points.Slice(0, points.Length - 1);
            }
            
            Assert.True(originalPoints.AsSpan().SequenceEqual(points));
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Create_FromPolygon_Case1(bool reverseOriginal)
        {
            PointF[] points = TestData.CreatePoints((0, 3), (3, 3), (3, 0), (1, 2), (1, 1), (0, 0));
            if (reverseOriginal)
            {
                points.AsSpan().Reverse();
            }
            
            Polygon polygon = new Polygon(new LinearLineSegment(points));

            using var multipolygon = TessellatedMultipolygon.Create(polygon, MemoryAllocator, Comparer);
            VerifyRing(multipolygon[0], points, reverseOriginal, false);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Create_FromPolygon_Case2(bool reverseOriginal)
        {
            PointF[] points = TestData.CreatePoints((0, 0), (2, 0), (3, 1), (3, 0), (6, 0), (6, 2), (5, 2), (5, 1), (4, 1), (4, 2), (2, 2), (1, 1), (0, 2));
            if (reverseOriginal)
            {
                points.AsSpan().Reverse();
            }
            
            Polygon polygon = new Polygon(new LinearLineSegment(points));

            using var multipolygon = TessellatedMultipolygon.Create(polygon, MemoryAllocator, Comparer);

            VerifyRing(multipolygon[0], points, !reverseOriginal, false);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Create_FromComplexPolygon(bool reverseContour, bool reverseHole)
        {
            PointF[] contour = TestData.CreatePoints((0, 0), (30, 0), (30, 30), (0, 30));
            PointF[] hole = TestData.CreatePoints((10, 10), (20, 10), (20, 20), (10, 20));
            
            if (reverseContour)
            {
                contour.AsSpan().Reverse();
            }

            if (reverseHole)
            {
                hole.AsSpan().Reverse();
            }

            ComplexPolygon polygon = new ComplexPolygon(
                new Path(new LinearLineSegment(contour)),
                new Path(new LinearLineSegment(hole)));
            
            using var multipolygon = TessellatedMultipolygon.Create(polygon, MemoryAllocator, Comparer);

            VerifyRing(multipolygon[0], contour, !reverseContour, false);
            VerifyRing(multipolygon[1], hole, !reverseHole, true);
        }

        [Fact]
        public void Create_FromRecangle()
        {
            RectangularPolygon polygon = new RectangularPolygon( 10, 20, 100, 50);

            PointF[] points = polygon.Flatten().Single().Points.Span.ToArray();
            
            using var multipolygon = TessellatedMultipolygon.Create(polygon, MemoryAllocator, Comparer);
            VerifyRing(multipolygon[0], points, true, false);
        }
        
        [Fact]
        public void Create_FromStar()
        {
            Star polygon = new Star(100, 100, 5, 30, 60);
            PointF[] points = polygon.Flatten().Single().Points.Span.ToArray();
            
            using var multipolygon = TessellatedMultipolygon.Create(polygon, MemoryAllocator, Comparer);
            VerifyRing(multipolygon[0], points, true, false);
        }
    }
}