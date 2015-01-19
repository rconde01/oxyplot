using System;
using Eto.Forms;
using Eto.Drawing;

namespace OxyPlot.Eto.Forms
{
   // TODO Finish xml comments

   public static class Conversions
   {
      public static RectangleF ToEtoRect(this OxyRect r, bool aliased = false)
      {
         if (aliased)
         {
            var x = 0.5f + (int)r.Left;
            var y = 0.5f + (int)r.Top;
            var ri = 0.5f + (int)r.Right;
            var bo = 0.5f + (int)r.Bottom;

            return new RectangleF(x, y, ri - x, bo - y);
         }

         return new RectangleF((float)r.Left, 
                               (float)r.Top, 
                               (float)r.Width, 
                               (float)r.Height);
      }

      public static OxyRect ToOxyRect(this Rectangle r)
      {
         return new OxyRect(r.Left, r.Top, r.Width, r.Height);
      }

      /// <summary>
      /// Converts a <see cref="OxyColor" /> to a <see cref="Color" />.
      /// </summary>
      /// <returns>The Eto color.</returns>
      /// <param name="c">The Oxy color.</param>
      public static Color ToEtoColor(this OxyColor c)
      {
         return Color.FromArgb(c.R, c.G, c.B, c.A);
      }

      public static PenLineJoin ToEtoLineJoin(this LineJoin lineJoin)
      {
         switch (lineJoin)
         {
            case LineJoin.Miter:
               return PenLineJoin.Miter;
            case LineJoin.Round:
               return PenLineJoin.Round;
            case LineJoin.Bevel:
               return PenLineJoin.Bevel;

            default:
               return PenLineJoin.Round;
         }
      }

      public static global::Eto.Forms.CursorType ToEtoCursorType(this CursorType cursorType)
      {
         switch (cursorType)
         {
            case OxyPlot.CursorType.Pan:
               return global::Eto.Forms.CursorType.Move;
            case OxyPlot.CursorType.ZoomRectangle:
               return global::Eto.Forms.CursorType.Crosshair;
            case OxyPlot.CursorType.ZoomHorizontal:
               return global::Eto.Forms.CursorType.HorizontalSplit;
            case OxyPlot.CursorType.ZoomVertical:
               return global::Eto.Forms.CursorType.VerticalSplit;
            default:
               return global::Eto.Forms.CursorType.Arrow;
         }
      }

      /// <summary>
      /// Creates the mouse down event arguments.
      /// </summary>
      /// <param name="args">The instance containing the event data.</param>
      /// <returns>Mouse event arguments.</returns>
      public static OxyMouseDownEventArgs ToOxyMouseDownEventArgs(this MouseEventArgs args)
      {
         return new OxyMouseDownEventArgs
         {
            ChangedButton = args.Buttons.ToOxyMouseButton(),
            ClickCount = 1,
            Position = new ScreenPoint(args.Location.X, args.Location.Y),
            ModifierKeys = OxyModifierKeys.None
         };
      }

      /// <summary>
      /// Creates the mouse up event arguments.
      /// </summary>
      /// <param name="args">The instance containing the event data.</param>
      /// <returns>Mouse event arguments.</returns>
      public static OxyMouseEventArgs ToOxyMouseUpEventArgs(this MouseEventArgs args)
      {
         return new OxyMouseEventArgs
         {
            Position = new ScreenPoint(args.Location.X, args.Location.Y),
            ModifierKeys = OxyModifierKeys.None
         };
      }

      /// <summary>
      /// Creates the mouse event arguments.
      /// </summary>
      /// <param name="args">The motion event args.</param>
      /// <returns>Mouse event arguments.</returns>
      public static OxyMouseEventArgs ToOxyMouseEventArgs(this MouseEventArgs args)
      {
         return new OxyMouseEventArgs
         {
            Position = new ScreenPoint(args.Location.X, args.Location.Y),
            ModifierKeys = OxyModifierKeys.None
         };
      }

      /// <summary>
      /// Creates the mouse wheel event arguments.
      /// </summary>
      /// <param name="args">The scroll event args.</param>
      /// <returns>Mouse event arguments.</returns>
      public static OxyMouseWheelEventArgs ToOxyMouseWheelEventArgs(this MouseEventArgs args)
      {
         return new OxyMouseWheelEventArgs
         {
            Delta = args.Delta.Height > 0 ? -120 : 120,
            Position = new ScreenPoint(args.Location.X, args.Location.Y),
            ModifierKeys = OxyModifierKeys.None
         };
      }

      /// <summary>
      /// Creates the key event arguments.
      /// </summary>
      /// <param name="e">The key event args.</param>
      /// <returns>Key event arguments.</returns>
      public static OxyKeyEventArgs ToOxyKeyEventArgs(this KeyEventArgs e)
      {
         return new OxyKeyEventArgs
         {
            ModifierKeys = ToOxyModifierKeys(e.Modifiers),
            Key = e.Key.ToOxyKey()
         };
      }

      /// <summary>
      /// Converts the specified key.
      /// </summary>
      /// <param name="k">The key to convert.</param>
      /// <returns>The converted key.</returns>
      static OxyKey ToOxyKey(this Keys k)
      {
         // TODO Finish implementing this...was unsure about some of the 
         // shift + key combinations

         Keys nonModifierKey = k & ~Keys.ModifierMask;
         Keys modifierKeys = k & Keys.ModifierMask;

         switch (nonModifierKey)
         {
            case Keys.A:
               return OxyKey.A;
            case Keys.B:
               return OxyKey.B;
            case Keys.C:
               return OxyKey.C;
            case Keys.D:
               return OxyKey.D;
            case Keys.E:
               return OxyKey.E;
            case Keys.F:
               return OxyKey.F;
            case Keys.G:
               return OxyKey.G;
            case Keys.H:
               return OxyKey.H;
            case Keys.I:
               return OxyKey.I;
            case Keys.J:
               return OxyKey.J;
            case Keys.K:
               return OxyKey.K;
            case Keys.L:
               return OxyKey.L;
            case Keys.M:
               return OxyKey.M;
            case Keys.N:
               return OxyKey.N;
            case Keys.O:
               return OxyKey.O;
            case Keys.P:
               return OxyKey.P;
            case Keys.Q:
               return OxyKey.Q;
            case Keys.R:
               return OxyKey.R;
            case Keys.S:
               return OxyKey.S;
            case Keys.T:
               return OxyKey.T;
            case Keys.U:
               return OxyKey.U;
            case Keys.V:
               return OxyKey.V;
            case Keys.W:
               return OxyKey.W;
            case Keys.X:
               return OxyKey.X;
            case Keys.Y:
               return OxyKey.Y;
            case Keys.Z:
               return OxyKey.Z;

            case Keys.Backspace:
               return OxyKey.Backspace;
            case Keys.Down:
               return OxyKey.Down;
            case Keys.End:
               return OxyKey.End;
            case Keys.Enter:
               return OxyKey.Enter;
            case Keys.Escape:
               return OxyKey.Escape;
            case Keys.F1:
               return OxyKey.F1;
            case Keys.F10:
               return OxyKey.F10;
            case Keys.F2:
               return OxyKey.F2;
            case Keys.F3:
               return OxyKey.F3;
            case Keys.F4:
               return OxyKey.F4;
            case Keys.F5:
               return OxyKey.F5;
            case Keys.F6:
               return OxyKey.F6;
            case Keys.F7:
               return OxyKey.F7;
            case Keys.F8:
               return OxyKey.F8;
            case Keys.F9:
               return OxyKey.F9;
            case Keys.Home:
               return OxyKey.Home;
            case Keys.Insert:
               return OxyKey.Insert;
            case Keys.Left:
               return OxyKey.Left;
            case Keys.PageDown:
               return OxyKey.PageDown;
            case Keys.PageUp:
               return OxyKey.PageUp;
            case Keys.Plus:
               return OxyKey.Add;
            case Keys.Right:
               return OxyKey.Right;
            case Keys.Space:
               return OxyKey.Space;
            case Keys.Tab:
               return OxyKey.Tab;
            case Keys.Up:
               return OxyKey.Up;
         }


         switch (nonModifierKey)
         {
            case Keys.D0:
               return OxyKey.D0;
            case Keys.D1:
               return OxyKey.D1;
            case Keys.D2:
               return OxyKey.D2;
            case Keys.D3:
               return OxyKey.D3;
            case Keys.D4:
               return OxyKey.D4;
            case Keys.D5:
               return OxyKey.D5;
            case Keys.D6:
               return OxyKey.D6;
            case Keys.D7:
               return OxyKey.D7;
            case Keys.D8:
               return OxyKey.D8;
            case Keys.D9:
               return OxyKey.D9;
            case Keys.Decimal:
               return OxyKey.Decimal;
            case Keys.Delete:
               return OxyKey.Delete;
            case Keys.Divide:
               return OxyKey.Divide;
            case Keys.Minus:
               return OxyKey.Subtract;
            default:
               return OxyKey.Unknown;
         }
      }

      static OxyModifierKeys ToOxyModifierKeys(Keys state)
      {
         var result = OxyModifierKeys.None;

         if ((state & Keys.Shift) != 0)
            result |= OxyModifierKeys.Shift;

         if ((state & Keys.Control) != 0)
            result |= OxyModifierKeys.Control;

         if ((state & Keys.Alt) != 0)
            result |= OxyModifierKeys.Alt;

         if ((state & Keys.Application) != 0)
            result |= OxyModifierKeys.Windows;

         return result;
      }

      static OxyMouseButton ToOxyMouseButton(this MouseButtons button)
      {
         switch (button)
         {
            case MouseButtons.Primary:
               return OxyMouseButton.Left;
            case MouseButtons.Middle:
               return OxyMouseButton.Middle;
            case MouseButtons.Alternate:
               return OxyMouseButton.Right;
         }

         return OxyMouseButton.Left;
      }
   }
}
