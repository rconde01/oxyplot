﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlotView.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Forms;
using Eto.Drawing;

namespace OxyPlot.Eto.Forms
{

   /// <summary>
   /// A Widget that displays a <see cref="PlotModel" />.
   /// </summary>
   [Serializable]
   public class PlotView : Drawable, IPlotView
   {

      /// <summary>
      /// The mouse hit tolerance.
      /// </summary>
      const double MouseHitTolerance = 10;

      /// <summary>
      /// The invalidate lock.
      /// </summary>
      readonly object invalidateLock = new object();

      /// <summary>
      /// The model lock.
      /// </summary>
      readonly object modelLock = new object();

      /// <summary>
      /// The render context.
      /// </summary>
      readonly GraphicsRenderContext renderContext;

      /// <summary>
      /// The current model (holding a reference to this plot view).
      /// </summary>
      [NonSerialized]
      PlotModel currentModel;

      /// <summary>
      /// The is model invalidated.
      /// </summary>
      bool isModelInvalidated;

      /// <summary>
      /// The model.
      /// </summary>
      PlotModel model;

      /// <summary>
      /// The update data flag.
      /// </summary>
      bool updateDataFlag = true;

      /// <summary>
      /// The zoom rectangle.
      /// </summary>
      OxyRect? zoomRectangle;

      /// <summary>
      /// The default controller
      /// </summary>
      IPlotController defaultController;

      /// <summary>
      /// The tracker definitions.
      /// </summary>
      Dictionary<string, TrackerSettings> trackerDefinitions;

      /// <summary>
      /// The tracker lock.
      /// </summary>
      readonly object trackerLock = new object();

      /// <summary>
      /// The tracker hit result.
      /// </summary>
      TrackerHitResult actualTrackerHitResult;

      /// <summary>
      /// The tracker popover.
      /// </summary>
      //Popover trackerPopover;

      /// <summary>
      /// The tracker label.
      /// </summary>
      Label lblTrackerText = new Label();

      /// <summary>
      /// Initializes a new instance of the <see cref="OxyPlot.Eto.Forms.PlotView"/> class.
      /// </summary>
      public PlotView()
      {
         renderContext = new GraphicsRenderContext();
         trackerDefinitions = new Dictionary<string, TrackerSettings>();
         DefaultTrackerSettings = new TrackerSettings();
         ZoomRectangleColor = OxyColor.FromArgb(0x40, 0xFF, 0xFF, 0x00);
         ZoomRectangleBorderColor = OxyColors.Transparent;
         ZoomRectangleBorderWidth = 1.0;
      }

      /// <summary>
      /// Gets or sets the model.
      /// </summary>
      public PlotModel Model
      {
         get
         {
            return model;
         }

         set
         {
            if (model != value)
            {
               model = value;
               OnModelChanged();
            }
         }
      }

      /// <summary>
      /// Gets the actual <see cref="PlotModel" /> of the control.
      /// </summary>
      Model IView.ActualModel
      {
         get
         {
            if (model == null)
               model = new PlotModel();
            return model;
         }
      }

      /// <summary>
      /// Gets the actual <see cref="OxyPlot.Model" /> of the control.
      /// </summary>
      public PlotModel ActualModel
      {
         get
         {
            return Model;
         }
      }

      /// <summary>
      /// Gets the actual controller.
      /// </summary>
      /// <value>
      /// The actual <see cref="IController" />.
      /// </value>
      IController IView.ActualController
      {
         get
         {
            return ActualController;
         }
      }

      /// <summary>
      /// Gets the coordinates of the client area of the view.
      /// </summary>
      public OxyRect ClientArea
      {
         get
         {
            return new OxyRect(0, 0, Bounds.Width, Bounds.Height);
         }
      }

      /// <summary>
      /// Gets the actual controller.
      /// </summary>
      /// <value>
      /// The actual <see cref="IController" />.
      /// </value>
      public IController ActualController
      {
         get
         {
            return Controller ?? (defaultController ?? (defaultController = new PlotController()));
         }
      }

      /// <summary>
      /// Gets or sets the plot controller.
      /// </summary>
      /// <value>The controller.</value>
      public IPlotController Controller { get; set; }

      /// <summary>
      /// Gets the tracker definitions.
      /// </summary>
      /// <value>The tracker definitions mapping.</value>
      /// <remarks>The tracker definitions make it possible to show different trackers for different series.
      /// The dictionary key must match the <see cref="OxyPlot.Series.Series.TrackerKey" /> property. If
      /// the dictionary does not contain a matching key, the <see cref="DefaultTrackerSettings"/> tracker
      /// configuration is used.</remarks>
      public Dictionary<string, TrackerSettings> TrackerDefinitions
      {
         get { return trackerDefinitions; }
      }

      /// <summary>
      /// Gets or sets the default tracker settings.
      /// </summary>
      /// <value>The default tracker settings.</value>
      /// <remarks>The default thracker settings to be used, if <see cref="TrackerDefinitions"/> does not
      /// contain a definition for the matching <see cref="OxyPlot.Series.Series.TrackerKey" />.</remarks>
      public TrackerSettings DefaultTrackerSettings { get; set; }

      /// <summary>
      /// Gets or sets the color of the zoom rectangle.
      /// </summary>
      /// <value>The color of the zoom rectangle.</value>
      public OxyColor ZoomRectangleColor { get; set; }

      /// <summary>
      /// Gets or sets the color of the zoom rectangle border.
      /// </summary>
      /// <value>The color of the zoom rectangle border.</value>
      public OxyColor ZoomRectangleBorderColor { get; set; }

      /// <summary>
      /// Gets or sets the width of the zoom rectangle border.
      /// </summary>
      /// <value>The width of the zoom rectangle border.</value>
      public double ZoomRectangleBorderWidth { get; set; }

      bool showDynamicTooltips = true;

      /// <summary>
      /// Gets or sets a value indicating whether this <see cref="OxyPlot.Eto.Forms.PlotView"/> shows dynamic tooltips
      /// for plot elements with <see cref="OxyPlot.PlotElement.ToolTip"/> property set.
      /// </summary>
      /// <value><c>true</c> to show dynamic tooltips; otherwise, <c>false</c>.</value>
      public bool ShowDynamicTooltips
      {
         get
         {
            return showDynamicTooltips;
         }
         set
         {
            showDynamicTooltips = value;
         }
      }

      /// <summary>
      /// Invalidates the plot (not blocking the UI thread)
      /// </summary>
      /// <param name="updateData">if set to <c>true</c>, all data collections will be updated.</param>
      public void InvalidatePlot(bool updateData = true)
      {
         lock (invalidateLock)
         {
            isModelInvalidated = true;
            updateDataFlag = updateDataFlag || updateData;
         }

         this.Invalidate();
      }

      /// <summary>
      /// Called when the Model property has been changed.
      /// </summary>
      void OnModelChanged()
      {
         lock (modelLock)
         {
            if (currentModel != null)
            {
               ((IPlotModel)currentModel).AttachPlotView(null);
            }

            if (Model != null)
            {
               if (Model.PlotView != null)
               {
                  throw new InvalidOperationException(
                      "This PlotModel is already in use by some other plot view.");
               }

               ((IPlotModel)Model).AttachPlotView(this);
               currentModel = Model;
            }
         }

         InvalidatePlot();
      }

      /// <summary>
      /// Sets the cursor type.
      /// </summary>
      /// <param name="cursorType">The cursor type.</param>
      public void SetCursorType(CursorType cursorType)
      {
         Cursor = new global::Eto.Forms.Cursor(cursorType.ToEtoCursorType());
      }


      /// <summary>
      /// Shows the tracker.
      /// </summary>
      /// <param name="trackerHitResult">The data.</param>
      public virtual void ShowTracker(TrackerHitResult trackerHitResult)
      {
         // TODO Implement this for Eto...not sure what to use for popover yet

         //if (trackerPopover != null)
         //   HideTracker();

         //if (trackerHitResult == null)
         //   return;

         //var trackerSettings = DefaultTrackerSettings;
         //if (trackerHitResult.Series != null && !string.IsNullOrEmpty(trackerHitResult.Series.TrackerKey))
         //   trackerSettings = trackerDefinitions[trackerHitResult.Series.TrackerKey];

         //if (trackerSettings.Enabled)
         //{
         //   lock (trackerLock)
         //   {
         //      actualTrackerHitResult = trackerHitResult;
         //      trackerPopover = new Popover(lblTrackerText);
         //      // TODO: background color, when supported by xwt
         //      //trackerPopover.Background = trackerSettings.Background;
         //      lblTrackerText.Text = trackerHitResult.Text;

         //      var position = new Rectangle(trackerHitResult.Position.X, trackerHitResult.Position.Y, 1, 1);
         //      //TODO: horizontal flip, needs xwt fix (https://github.com/mono/xwt/pull/362)
         //      //if (trackerHitResult.Position.Y <= Bounds.Height / 2)
         //      trackerPopover.Show(Popover.Position.Top, this, position);
         //      //else
         //      //	trackerPopover.Show (Popover.Position.Bottom, this, position);
         //   }
         //}

         Invalidate();
      }

      /// <summary>
      /// Hides the tracker.
      /// </summary>
      public virtual void HideTracker()
      {
         // TODO Implement this for Eto...not sure what to use for popover yet

         //lock (trackerLock)
         //{
         //   actualTrackerHitResult = null;
         //   if (trackerPopover != null)
         //   {
         //      trackerPopover.Hide();
         //      trackerPopover.Content = null;
         //      trackerPopover.Dispose();
         //      trackerPopover = null;
         //   }
         //}

         Invalidate();
      }

      /// <summary>
      /// Shows the zoom rectangle.
      /// </summary>
      /// <param name="rectangle">The rectangle.</param>
      public virtual void ShowZoomRectangle(OxyRect rectangle)
      {
         zoomRectangle = rectangle;
         Invalidate();
      }

      /// <summary>
      /// Hides the zoom rectangle.
      /// </summary>
      public virtual void HideZoomRectangle()
      {
         zoomRectangle = null;
         Invalidate();
      }

      /// <summary>
      /// Sets the clipboard text.
      /// </summary>
      /// <param name="text">The text.</param>
      void IPlotView.SetClipboardText(string text)
      {
         // TODO Implement this for Eto...not sure how to do this in Eto yet

         //Clipboard.SetText(text);
      }

      /// <summary>
      /// Pans all axes.
      /// </summary>
      /// <param name="dx">Dx.</param>
      /// <param name="dy">Dy.</param>
      public void PanAllAxes(double dx, double dy)
      {
         if (ActualModel != null)
            ActualModel.PanAllAxes(dx, dy);

         InvalidatePlot(false);
      }

      /// <summary>
      /// Zooms all axes.
      /// </summary>
      /// <param name="factor">The zoom factor.</param>
      public void ZoomAllAxes(double factor)
      {
         if (ActualModel != null)
            ActualModel.ZoomAllAxes(factor);

         InvalidatePlot(false);
      }

      /// <summary>
      /// Resets all axes.
      /// </summary>
      public void ResetAllAxes()
      {
         if (ActualModel != null)
            ActualModel.ResetAllAxes();

         InvalidatePlot(false);
      }

      protected override void OnMouseDown(MouseEventArgs e)
      {
         base.OnMouseDown(e);

         if (e.Handled)
            return;

         e.Handled = ActualController.HandleMouseDown(this,
                                                      e.ToOxyMouseDownEventArgs());
      }

      protected override void OnMouseMove(MouseEventArgs e)
      {
         base.OnMouseMove(e);

         if (e.Handled)
            return;

         if (ShowDynamicTooltips && ActualModel != null)
         {
            string tooltip = null;
            var hitArgs = new HitTestArguments(new ScreenPoint(e.Location.X, e.Location.Y), MouseHitTolerance);

            foreach (var result in ActualModel.HitTest(hitArgs))
            {
               var plotElement = result.Element as PlotElement;
               if (plotElement != null && !String.IsNullOrEmpty(plotElement.ToolTip))
               {
                  tooltip = String.IsNullOrEmpty(tooltip) ? plotElement.ToolTip : tooltip + Environment.NewLine + plotElement.ToolTip;
               }
            }

            ToolTip = tooltip;
         }

         e.Handled = ActualController.HandleMouseMove(this,
                                                      e.ToOxyMouseEventArgs());
      }

      protected override void OnMouseUp(MouseEventArgs e)
      {
         base.OnMouseUp(e);

         if (e.Handled)
            return;

         e.Handled = ActualController.HandleMouseUp(this,
                                                    e.ToOxyMouseUpEventArgs());
      }

      protected override void OnMouseWheel(MouseEventArgs e)
      {
         base.OnMouseWheel(e);

         if (e.Handled)
            return;

         e.Handled = ActualController.HandleMouseWheel(this,
                                                       e.ToOxyMouseWheelEventArgs());
      }

      protected override void OnMouseEnter(MouseEventArgs e)
      {
         base.OnMouseEnter(e);

         ActualController.HandleMouseEnter(this, new OxyMouseEventArgs());
      }

      protected override void OnMouseLeave(MouseEventArgs e)
      {
         base.OnMouseLeave(e);

         ActualController.HandleMouseLeave(this, new OxyMouseEventArgs());
      }

      protected override void OnKeyDown(KeyEventArgs e)
      {
         base.OnKeyDown(e);

         if (e.Handled)
            return;

         e.Handled = ActualController.HandleKeyDown(this,
                                                    e.ToOxyKeyEventArgs());
      }

      protected override void OnPaint(PaintEventArgs e)
      {
         try
         {
            lock (invalidateLock)
            {
               if (isModelInvalidated)
               {
                  if (model != null)
                  {
                     ((IPlotModel)model).Update(updateDataFlag);
                     updateDataFlag = false;
                  }
                  isModelInvalidated = false;
               }
            }

            renderContext.Graphics = e.Graphics;

            if (model != null)
            {
               if (!model.Background.IsUndefined())
                  renderContext.DrawRectangle(new OxyRect(0,0,Bounds.Width,Bounds.Height),
                                              model.Background,
                                              OxyColors.Undefined,
                                              0);

               ((IPlotModel)model).Render(renderContext, Bounds.Width, Bounds.Height);
            }

            if (zoomRectangle.HasValue)
            {
               renderContext.DrawRectangle(zoomRectangle.Value,
                                       ZoomRectangleColor,
                                       ZoomRectangleBorderColor,
                                       ZoomRectangleBorderWidth);
            }

            if (actualTrackerHitResult != null)
            {
               var trackerSettings = DefaultTrackerSettings;
               if (actualTrackerHitResult.Series != null && !string.IsNullOrEmpty(actualTrackerHitResult.Series.TrackerKey))
                  trackerSettings = trackerDefinitions[actualTrackerHitResult.Series.TrackerKey];

               if (trackerSettings.Enabled)
               {
                  var extents = actualTrackerHitResult.LineExtents;
                  if (Math.Abs(extents.Width) < double.Epsilon)
                  {
                     extents.Left = actualTrackerHitResult.XAxis.ScreenMin.X;
                     extents.Right = actualTrackerHitResult.XAxis.ScreenMax.X;
                  }
                  if (Math.Abs(extents.Height) < double.Epsilon)
                  {
                     extents.Top = actualTrackerHitResult.YAxis.ScreenMin.Y;
                     extents.Bottom = actualTrackerHitResult.YAxis.ScreenMax.Y;
                  }

                  var pos = actualTrackerHitResult.Position;

                  if (trackerSettings.HorizontalLineVisible)
                  {

                     renderContext.DrawLine(
                        new[] { new ScreenPoint(extents.Left, pos.Y), new ScreenPoint(extents.Right, pos.Y) },
                        trackerSettings.HorizontalLineColor,
                        trackerSettings.HorizontalLineWidth,
                        trackerSettings.HorizontalLineActualDashArray,
                        LineJoin.Miter,
                        true);
                  }
                  if (trackerSettings.VerticalLineVisible)
                  {
                     renderContext.DrawLine(
                        new[] { new ScreenPoint(pos.X, extents.Top), new ScreenPoint(pos.X, extents.Bottom) },
                        trackerSettings.VerticalLineColor,
                        trackerSettings.VerticalLineWidth,
                        trackerSettings.VerticalLineActualDashArray,
                        LineJoin.Miter,
                        true);
                  }
               }
            }
         }
         catch (Exception paintException)
         {
            var trace = new StackTrace(paintException);
            Debug.WriteLine(paintException);
            Debug.WriteLine(trace);
         }
      }

      protected override void Dispose(bool disposing)
      {

         // TODO Implement this for Eto...not sure what to use for popover yet

         if (disposing)
         {
            renderContext.Dispose();
            //if (trackerPopover != null)
            //   trackerPopover.Dispose();
         }
         base.Dispose(disposing);
      }
   }
}

