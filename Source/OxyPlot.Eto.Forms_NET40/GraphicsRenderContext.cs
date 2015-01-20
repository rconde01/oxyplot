// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphicsRenderContext.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;

namespace OxyPlot.Eto.Forms
{
   /// <summary>
   /// The Eto graphics render context.
   /// </summary>
   public class GraphicsRenderContext : RenderContextBase, IDisposable
   {
      /// <summary>
      /// The image cache.
      /// </summary>
      readonly Dictionary<OxyImage, Image> _ImageCache = new Dictionary<OxyImage, Image>();

      /// <summary>
      /// The images in use.
      /// </summary>
      readonly HashSet<OxyImage> _ImagesInUse = new HashSet<OxyImage>();

      /// <summary>
      /// The fonts cache.
      /// </summary>
      readonly Dictionary<string, Font> _FontCache = new Dictionary<string, Font>();

      /// <summary>
      /// The pen cache.
      /// </summary>
      readonly Dictionary<string, Pen> _PenCache = new Dictionary<string, Pen>();

      /// <summary>
      /// The brush cache.
      /// </summary>
      readonly Dictionary<string, Brush> _BrushCache = new Dictionary<string, Brush>();

      public Graphics Graphics
      {
         get;
         set;
      }

      /// <summary>
      /// Draws an ellipse.
      /// </summary>
      /// <param name="rect">The rectangle defining the extents of the ellipse.</param>
      /// <param name="fill">The fill color. If set to <c>OxyColors.Undefined</c>, the extents will not be filled.</param>
      /// <param name="stroke">The stroke color. If set to <c>OxyColors.Undefined</c>, the extents will not be stroked.</param>
      /// <param name="thickness">The thickness (in device independent units, 1/96 inch).</param>
      public override void DrawEllipse(OxyRect rect,
                                       OxyColor fill,
                                       OxyColor stroke,
                                       double thickness)
      {
         var ex = (float)(rect.Left + (rect.Width / 2.0));
         var ey = (float)(rect.Top + (rect.Height / 2.0));


         if (fill.IsVisible())
         {
            var sb = GetCachedBrush(fill.ToEtoColor());

            Graphics.FillEllipse(sb, rect.ToEtoRect());
         }

         if (stroke.IsVisible() && thickness > 0)
         {
            var p = GetCachedPen(stroke.ToEtoColor(), thickness, null);

            Graphics.DrawEllipse(p, rect.ToEtoRect());
         }
      }

      /// <summary>
      /// Draws a polyline.
      /// </summary>
      /// <param name="points">The points defining the polyline.</param>
      /// <param name="stroke">The stroke color.</param>
      /// <param name="thickness">The stroke thickness (in device independent units, 1/96 inch).</param>
      /// <param name="dashArray">The dash array (in device independent units, 1/96 inch). Use <c>null</c> to get a solid line.</param>
      /// <param name="lineJoin">The line join type.</param>
      /// <param name="aliased">if set to <c>true</c> the shape will be aliased.</param>
      public override void DrawLine(IList<ScreenPoint> points,
                                    OxyColor stroke,
                                    double thickness,
                                    double[] dashArray,
                                    LineJoin lineJoin,
                                    bool aliased)
      {
         if (stroke.IsVisible() && thickness > 0 && points.Count >= 2)
         {
            PointF[] fPoints = new PointF[points.Count];

            for(int i = 0; i < points.Count; i++)
            {
               fPoints[i] = new PointF((float)points[i].X, (float)points[i].Y);
            }

            bool aliasedOrig = Graphics.AntiAlias;

            Graphics.AntiAlias = !aliased;

            var p = GetCachedPen(stroke.ToEtoColor(), thickness, dashArray);

            using (GraphicsPath gp = new GraphicsPath())
            {
               gp.AddLines(fPoints);

               Graphics.DrawPath(p, gp);
            }

            Graphics.AntiAlias = aliasedOrig;
         }
      }

      /// <summary>
      /// Draws a polygon.
      /// </summary>
      /// <param name="points">The points defining the polygon.</param>
      /// <param name="fill">The fill color. If set to <c>OxyColors.Undefined</c>, the polygon will not be filled.</param>
      /// <param name="stroke">The stroke color. If set to <c>OxyColors.Undefined</c>, the polygon will not be stroked.</param>
      /// <param name="thickness">The stroke thickness (in device independent units, 1/96 inch).</param>
      /// <param name="dashArray">The dash array (in device independent units, 1/96 inch).</param>
      /// <param name="lineJoin">The line join type.</param>
      /// <param name="aliased">If set to <c>true</c> the polygon will be aliased.</param>
      public override void DrawPolygon(IList<ScreenPoint> points,
                                       OxyColor fill,
                                       OxyColor stroke,
                                       double thickness,
                                       double[] dashArray,
                                       LineJoin lineJoin,
                                       bool aliased)
      {
         bool drawFill = fill.IsVisible() && points.Count >= 2;
         bool drawStroke = stroke.IsVisible() && thickness > 0 && points.Count >= 2;

         if(!drawFill && !drawStroke)
            return;

         PointF[] fPoints = new PointF[points.Count + 1];

         for(int i = 0; i < points.Count; i++)
         {
            fPoints[i] = new PointF((float)points[i % points.Count].X, (float)points[i % points.Count].Y);
         }

         fPoints[points.Count] = fPoints[0];

         bool aliasedOrig = Graphics.AntiAlias;

         Graphics.AntiAlias = !aliased;

         if (drawFill)
         {
            var sb = GetCachedBrush(fill.ToEtoColor());

            Graphics.FillPolygon(sb, new ArraySegment<PointF>(fPoints,0,points.Count).Array);
         }

         if (drawStroke)
         {
            var p = GetCachedPen(stroke.ToEtoColor(), thickness, dashArray, lineJoin.ToEtoLineJoin());

            Graphics.DrawPolygon(p, fPoints);
         }

         Graphics.AntiAlias = aliasedOrig;
      }

      /// <summary>
      /// Draws a rectangle.
      /// </summary>
      /// <param name="rect">The rectangle to draw.</param>
      /// <param name="fill">The fill color. If set to <c>OxyColors.Undefined</c>, the rectangle will not be filled.</param>
      /// <param name="stroke">The stroke color. If set to <c>OxyColors.Undefined</c>, the rectangle will not be stroked.</param>
      /// <param name="thickness">The stroke thickness (in device independent units, 1/96 inch).</param>
      public override void DrawRectangle(OxyRect rect,
                                         OxyColor fill,
                                         OxyColor stroke,
                                         double thickness)
      {
         var ex = (float)(rect.Left + (rect.Width / 2.0));
         var ey = (float)(rect.Top + (rect.Height / 2.0));


         if (fill.IsVisible())
         {
            var sb = GetCachedBrush(fill.ToEtoColor());

            Graphics.FillRectangle(sb, rect.ToEtoRect());
         }

         if (stroke.IsVisible() && thickness > 0)
         {
            var p = GetCachedPen(stroke.ToEtoColor(), thickness, null);

            Graphics.DrawRectangle(p, rect.ToEtoRect());
         }
      }

      /// <summary>
      /// Draws text.
      /// </summary>
      /// <param name="p">The position.</param>
      /// <param name="text">The text.</param>
      /// <param name="fill">The text color.</param>
      /// <param name="fontFamily">The font family.</param>
      /// <param name="fontSize">Size of the font (in device independent units, 1/96 inch).</param>
      /// <param name="fontWeight">The font weight.</param>
      /// <param name="rotate">The rotation angle.</param>
      /// <param name="halign">The horizontal alignment.</param>
      /// <param name="valign">The vertical alignment.</param>
      /// <param name="maxSize">The maximum size of the text (in device independent units, 1/96 inch).</param>
      public override void DrawText(ScreenPoint p,
                                    string text,
                                    OxyColor fill,
                                    string fontFamily,
                                    double fontSize,
                                    double fontWeight,
                                    double rotate,
                                    HorizontalAlignment halign,
                                    VerticalAlignment valign,
                                    OxySize? maxSize)
      {
         Graphics.SaveTransform();

         Font font = GetCachedFont(fontFamily, fontSize, fontWeight);

         SizeF size = Graphics.MeasureString(font, text);
         
         if (maxSize != null)
         {
            size.Width = Math.Min(size.Width, (float)maxSize.Value.Width);
            size.Height = Math.Min(size.Height, (float)maxSize.Value.Height);
         }

         double dx = 0;
         if (halign == HorizontalAlignment.Center)
         {
            dx = -size.Width / 2;
         }

         if (halign == HorizontalAlignment.Right)
         {
            dx = -size.Width;
         }

         double dy = 0;
         if (valign == VerticalAlignment.Middle)
         {
            dy = -size.Height / 2;
         }

         if (valign == VerticalAlignment.Bottom)
         {
            dy = -size.Height;
         }

         Graphics.TranslateTransform((float)p.X, (float)p.Y);

         if (Math.Abs(rotate) > double.Epsilon)
         {
            Graphics.RotateTransform((float)rotate);
         }

         Graphics.TranslateTransform((float)dx, (float)dy);

         Graphics.DrawText(font, fill.ToEtoColor(), PointF.Empty, text);

         Graphics.RestoreTransform();
      }

      /// <summary>
      /// Measures the size of the specified text.
      /// </summary>
      /// <param name="text">The text.</param>
      /// <param name="fontFamily">The font family.</param>
      /// <param name="fontSize">Size of the font (in device independent units, 1/96 inch).</param>
      /// <param name="fontWeight">The font weight.</param>
      /// <returns>The size of the text (in device independent units, 1/96 inch).</returns>
      public override OxySize MeasureText(string text,
                                          string fontFamily,
                                          double fontSize,
                                          double fontWeight)
      {
         if (text == null)
            return OxySize.Empty;

         Font font = GetCachedFont(fontFamily, fontSize, fontWeight);

         SizeF size = Graphics.MeasureString(font,text);

         return new OxySize(size.Width, size.Height);
      }

      /// <summary>
      /// Cleans up resources not in use.
      /// </summary>
      /// <remarks>This method is called at the end of each rendering.</remarks>
      public override void CleanUp()
      {
         var imagesToRelease = _ImageCache.Keys.Where(i => !_ImagesInUse.Contains(i)).ToList();
         foreach (var i in imagesToRelease)
         {
            var image = GetImage(i);
            image.Dispose();
            _ImageCache.Remove(i);
         }

         _ImagesInUse.Clear();
      }

      /// <summary>
      /// Draws a portion of the specified <see cref="OxyImage" />.
      /// </summary>
      /// <param name="source">The source.</param>
      /// <param name="srcX">The x-coordinate of the upper-left corner of the portion of the source image to draw.</param>
      /// <param name="srcY">The y-coordinate of the upper-left corner of the portion of the source image to draw.</param>
      /// <param name="srcWidth">Width of the portion of the source image to draw.</param>
      /// <param name="srcHeight">Height of the portion of the source image to draw.</param>
      /// <param name="destX">The x-coordinate of the upper-left corner of drawn image.</param>
      /// <param name="destY">The y-coordinate of the upper-left corner of drawn image.</param>
      /// <param name="destWidth">The width of the drawn image.</param>
      /// <param name="destHeight">The height of the drawn image.</param>
      /// <param name="opacity">The opacity.</param>
      /// <param name="interpolate">interpolate if set to <c>true</c>.</param>
      public override void DrawImage(OxyImage source,
                                     double srcX,
                                     double srcY,
                                     double srcWidth,
                                     double srcHeight,
                                     double destX,
                                     double destY,
                                     double destWidth,
                                     double destHeight,
                                     double opacity,
                                     bool interpolate)
      {
         var image = GetImage(source);
         if (image != null)
         {
            var sourceRect = new RectangleF((float)srcX, (float)srcY, (float)srcWidth, (float)srcHeight);
            var destRect = new RectangleF((float)destX, (float)destY, (float)destWidth, (float)destHeight);

            Graphics.DrawImage(image, sourceRect, destRect);
         }
      }

      /// <summary>
      /// Sets the clipping rectangle.
      /// </summary>
      /// <param name="clippingRectangle">The clipping rectangle.</param>
      /// <returns><c>true</c> if the clip rectangle was set.</returns>
      public override bool SetClip(OxyRect rect)
      {
         Graphics.SetClip(rect.ToEtoRect());

         return true;
      }

      /// <summary>
      /// Resets the clipping rectangle.
      /// </summary>
      public override void ResetClip()
      {
         Graphics.ResetClip();
      }

      private Pen GetCachedPen(Color color, double thickness, double[] dashArray)
      {
         return GetCachedPen(color, thickness, dashArray, PenLineJoin.Miter);
      }

      private Pen GetCachedPen(Color color, double thickness, double[] dashArray, PenLineJoin lineJoin)
      {
         if (dashArray == null)
            dashArray = new double[0];

         string key = color.ToHex() + thickness.ToString() + dashArray.Length.ToString() + lineJoin.ToString();

         if (_PenCache.ContainsKey(key))
            return _PenCache[key];

         Pen newPen = new Pen(color, (float)thickness);

         if (dashArray.Length != 0)
         {
            float[] fdashArray = new float[dashArray.Length];

            for (int i = 0; i < dashArray.Length; i++)
            {
               fdashArray[i] = (float)dashArray[i];
            }

            newPen.DashStyle = new DashStyle(0.0f, fdashArray);
         }

         newPen.LineJoin = lineJoin;

         _PenCache.Add(key, newPen);

         return newPen;
      }

      private Brush GetCachedBrush(Color color)
      {
         string key = color.ToHex();

         if (_BrushCache.ContainsKey(key))
            return _BrushCache[key];

         SolidBrush newBrush = new SolidBrush(color);

         _BrushCache.Add(key, newBrush);

         return newBrush;
      }

      /// <summary>
      /// Gets the specified font from cache.
      /// </summary>
      /// <returns>The font.</returns>
      /// <param name="fontFamily">Font family name.</param>
      /// <param name="fontSize">Font size.</param>
      /// <param name="fontWeight">Font weight.</param>
      Font GetCachedFont(string fontFamily, double fontSize, double fontWeight)
      {
         if(string.IsNullOrEmpty(fontFamily))
         {
            fontFamily = FontFamilies.Sans.ToString();
         }

         var fs = (fontWeight >= 700) ? FontStyle.Bold : FontStyle.None;
         var key = fontFamily + ' ' + fs + ' ' + fontSize.ToString("0.###");
         Font font;

         return _FontCache.TryGetValue(key, out font) ? font : _FontCache[key] = new Font(fontFamily,(float)fontSize,fs);
      }


      /// <summary>
      /// Gets the cached <see cref="Image" /> of the specified <see cref="OxyImage" />.
      /// </summary>
      /// <param name="source">The source image.</param>
      /// <returns>The <see cref="Image" />.</returns>
      private Image GetImage(OxyImage source)
      {
         if (source == null)
            return null;

         if (!this._ImagesInUse.Contains(source))
            this._ImagesInUse.Add(source);

         Image src;
         if (this._ImageCache.TryGetValue(source, out src))
            return src;

         Image btm = new Bitmap(source.GetData());

         _ImageCache.Add(source, btm);
         return btm;
      }

      /// <summary>
      /// Releases all resource used by the <see cref="OxyPlot.Eto.GraphicsRenderContext"/> object.
      /// </summary>
      public void Dispose()
      {
         foreach (var image in this._ImageCache.Values)
         {
            image.Dispose();
         }

         foreach (var pen in this._PenCache.Values)
         {
            pen.Dispose();
         }

         _PenCache.Clear();

         foreach (var brush in this._BrushCache.Values)
         {
            brush.Dispose();
         }

         _BrushCache.Clear();

         foreach(var font in this._FontCache.Values)
         {
            font.Dispose();
         }

         _FontCache.Clear();
      }
   }
}

