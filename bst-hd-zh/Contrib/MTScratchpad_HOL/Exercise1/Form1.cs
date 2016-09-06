// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.

// MTScratchpadWMTouch application.
// Description:
//  Inside the application window, user can draw using multiple fingers
//  at the same time. The trace of each finger is drawn using different
//  color. The primary finger trace is always drawn in black, and the
//  remaining traces are drawn using rotating colors: red, blue,
//  green, magenta, cyan and yellow.
//
// Purpose:
//  This sample demonstrates handling of the multi-touch input inside
//  a C# application using WM_TOUCH* group of window messages:
//  - Registering a window for multi-touch using RegisterTouchWindow,
//    IsTouchWindow.
//  - Handling WM_TOUCHDOWN, WM_TOUCHUP and WM_TOUCHMOVE messages and
//    unpacking their parameters using GetTouchInputInfo and
//    CloseTouchInputHandle; reading contact info from the TOUCHINPUT
//    structure.
//  In addition, the sample also shows how to store and draw strokes
//  entered by user, using the helper classes Stroke and
//  CollectionOfStrokes.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Microsoft.Samples.TabletPC.MTScratchpad.WMTouch
{
    // Main app, WMToouchForm-derived, multi-touch aware form.
//Exercise2-Task1-Step6 
    public partial class Form1 : WMTouchForm
    {
        // Constructor
        public Form1()
        {
            InitializeComponent();

            // Create data members, color generator and stroke collections.
            touchColor = new TouchColor ();
            ActiveStrokes = new CollectionOfStrokes ();
            FinishedStrokes = new CollectionOfStrokes ();
//Exercise3-Task1-Step3
/*
            // Setup handlers
            Touchdown += OnTouchDownHandler;
            Touchup += OnTouchUpHandler;
            TouchMove += OnTouchMoveHandler;
*/
            Paint += new PaintEventHandler(this.OnPaintHandler);
        }

        // Touch down event handler.
        // in:
        //      sender      object that has sent the event
        //      e           touch event arguments

//Exercise3-Task1-Step5 
/*
        private void OnTouchDownHandler(object sender, WMTouchEventArgs e)
        {
            // If there exist stroke with this ID, finish it.
            Stroke oldStroke = ActiveStrokes.Get(e.Id);
            if(oldStroke != null)
            {
                // Add finished stroke to the collection of finished strokes.
                FinishedStrokes.Add(oldStroke);

                // Remove finished stroke from the collection of strokes in drawing.
                ActiveStrokes.Remove(e.Id);

                // Request redraw of the window.
                Invalidate();

                return;
            }

            // Create new stroke, add point and assign a color to it.
            Stroke newStroke = new Stroke ();
            newStroke.Color = touchColor.GetColor(e.IsPrimaryContact);
            newStroke.Id = e.Id;

            // Add new stroke to the collection of strokes in drawing.
            ActiveStrokes.Add(newStroke);
        }
*/
        // Touch up event handler.
        // in:
        //      sender      object that has sent the event
        //      e           touch event arguments
//Exercise3-Task3-Step1
/*
        private void OnTouchUpHandler(object sender, WMTouchEventArgs e)
        {
            // Find the stroke in the collection of the strokes in drawing.
            Stroke stroke = ActiveStrokes.Get(e.Id);
            if(stroke != null)
            {
                // Add finished stroke to the collection of finished strokes.
                FinishedStrokes.Add(stroke);

                // Remove finished stroke from the collection of strokes in drawing.
                ActiveStrokes.Remove(e.Id);

                // Request full redraw.
                Invalidate();
            }
        }
*/
        // Touch move event handler.
        // in:
        //      sender      object that has sent the event
        //      e           touch event arguments
//Exercise3-Task2-Step1
/*
        private void OnTouchMoveHandler(object sender, WMTouchEventArgs e)
        {
            // Find the stroke in the collection of the strokes in drawing.
            Stroke stroke = ActiveStrokes.Get(e.Id);
            if(stroke != null)
            {
                // Add contact point to the stroke
                stroke.Add(new Point(e.LocationX, e.LocationY));

                // Partial redraw: only the last line segment
                Graphics g = this.CreateGraphics();
                stroke.DrawLast(g);
            }
        }
*/
        // OnPaint event handler.
        // in:
        //      sender      object that has sent the event
        //      e           paint event arguments
//Exercise3-Task1-Step9 
        private void OnPaintHandler(object sender, PaintEventArgs e)
        {
            // Erase the background
            Brush brush = new SolidBrush(SystemColors.Window);
            e.Graphics.FillRectangle(brush, ClientRectangle);

            // Full redraw: draw complete collection of finished strokes and
            // also all the strokes that are currently in drawing.
            FinishedStrokes.Draw(e.Graphics);
            ActiveStrokes.Draw(e.Graphics);
        }

        // Attributes
        private TouchColor touchColor;                  // Color generator
        private CollectionOfStrokes FinishedStrokes;    // Collection of finished strokes
        private CollectionOfStrokes ActiveStrokes;      // Collection of active strokes, currently being drawn by the user
    }

    // Color generator: assigns a color to the new stroke.
    public class TouchColor
    {
        // Constructor
        public TouchColor()
        {
        }

        // Returns color for the newly started stroke.
        // in:
        //      primary         flag, whether the contact is the primary contact
        // returns:
        //      color of the stroke
        public Color GetColor(bool primary)
        {
            if (primary)
            {
                // The primary contact is drawn in black.
                return Color.Black;
            }
            else
            {
                // Take current secondary color.
                Color color = secondaryColors[cnt];

                // Move to the next color in the array.
                cnt = (cnt + 1) % secondaryColors.Length;

                return color;
            }
        }

        // Attributes
        static private Color[] secondaryColors =    // Secondary colors
        {
            Color.Red,
            Color.LawnGreen,
            Color.Blue,
            Color.Cyan,
            Color.Magenta,
            Color.Yellow
        };
        private int cnt = 0;                // Rotating secondary color index
    }
}