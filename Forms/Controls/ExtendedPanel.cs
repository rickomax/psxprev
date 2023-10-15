using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using PSXPrev.Forms.Utils;

namespace PSXPrev.Forms.Controls
{
    public class ExtendedPanel : Panel
    {
        // Mouse Dragging state
        // Currently _draggingReady is ignored, since it looks more correct to show the cursor immediately upon clicking
        private bool _draggingReady; // True if the mouse has been pressed down, but hasn't been moved yet
        private bool _dragging;      // True if the mouse has been pressed down and moved
        private Point _dragStartPosition; // Initial position when mouse was pressed down


        public bool IsMouseDragging => _dragging;


        private bool _allowFocus;
        [Category("Behavior")]
        [Description("Allows this panel to receive focus.")]
        [DefaultValue(false)]
        public bool AllowFocus
        {
            get => _allowFocus;
            set
            {
                if (_allowFocus != value)
                {
                    _allowFocus = value;
                    SetStyle(ControlStyles.Selectable, value);
                }
            }
        }

        private bool _allowMouseDragging;
        [Category("Behavior")]
        [Description("Allows scrolling by dragging the mouse.")]
        [DefaultValue(false)]
        public bool AllowMouseDragging
        {
            get => _allowMouseDragging;
            set
            {
                if (_allowMouseDragging != value)
                {
                    EndMouseDragging(null); // Make sure to end the dragging event
                    _allowMouseDragging = value;
                }
            }
        }

        private bool _allowMouseWheelScrolling = true;
        [Category("Behavior")]
        [Description("Allows scrolling with the mouse wheel. False disables the " + nameof(MouseWheel) + " event and enables the " + nameof(MouseWheelEx) + " event.")]
        [DefaultValue(true)]
        public bool AllowMouseWheelScrolling
        {
            get => _allowMouseWheelScrolling;
            set => _allowMouseWheelScrolling = value;
        }

        [Category("Mouse")]
        [Description("Event triggered instead of " + nameof(MouseWheel) + " when " + nameof(AllowMouseWheelScrolling) + " is false.")]
        public event MouseEventHandler MouseWheelEx;


        public ExtendedPanel()
        {
        }


        protected override void OnControlAdded(ControlEventArgs e)
        {
            // Hook events used to process mouse behavior over-top of this control
            e.Control.MouseDown += OnChildMouseDown;
            e.Control.MouseMove += OnChildMouseMove;
            e.Control.MouseUp   += OnChildMouseUp;
            base.OnControlAdded(e);
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            // Unhook events used to process mouse behavior over-top of this control
            e.Control.MouseDown -= OnChildMouseDown;
            e.Control.MouseMove -= OnChildMouseMove;
            e.Control.MouseUp   -= OnChildMouseUp;
            base.OnControlRemoved(e);
        }

        private void OnChildMouseDown(object sender, MouseEventArgs e)
        {
            if (_allowFocus && sender is Control control)
            {
                // If none of the child controls can receive focus, then give it to this control
                var parent = control;
                while (parent != null && parent != this && !parent.CanSelect)
                {
                    parent = parent.Parent;
                }
                if (parent == this)
                {
                    Focus();
                }
            }
            BeginMouseDragging(e.Button);
        }

        private void OnChildMouseMove(object sender, MouseEventArgs e)
        {
            UpdateMouseDragging(e.Button);
        }

        private void OnChildMouseUp(object sender, MouseEventArgs e)
        {
            EndMouseDragging(e.Button);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (_allowFocus)
            {
                Focus();
            }
            BeginMouseDragging(e.Button);
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            UpdateMouseDragging(e.Button);
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            EndMouseDragging(e.Button);
            base.OnMouseUp(e);
        }

        private void BeginMouseDragging(MouseButtons? button)
        {
            if (_allowMouseDragging && (!button.HasValue || button.Value == MouseButtons.Left))
            {
                // We should never be dragging during this event, but avoid getting our state messed up if we are
                EndMouseDragging(button);

                var mousePos = PointToClient(MousePosition);
                _dragStartPosition = new Point(mousePos.X - AutoScrollPosition.X,
                                               mousePos.Y - AutoScrollPosition.Y);
                _draggingReady = true;

                // For now, it looks more correct to immediately show the cursor upon clicking, so do that in Update
                UpdateMouseDragging(button);
            }
        }

        private void UpdateMouseDragging(MouseButtons? button)
        {
            if ((_draggingReady || _dragging) && (!button.HasValue || button.Value == MouseButtons.Left))
            {
                if (_draggingReady)
                {
                    _draggingReady = false;
                    _dragging = true;
                    Cursor = Cursors.SizeAll; // Only change cursor once we've started dragging for real
                }
                var mousePos = PointToClient(MousePosition);
                AutoScrollPosition = new Point(_dragStartPosition.X - mousePos.X,
                                               _dragStartPosition.Y - mousePos.Y);
            }
        }

        private void EndMouseDragging(MouseButtons? button)
        {
            if ((_draggingReady || _dragging) && (!button.HasValue || button.Value == MouseButtons.Left))
            {
                if (_dragging)
                {
                    Cursor = Cursors.Default; // Restore cursor
                }
                _draggingReady = false;
                _dragging = false;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (_allowFocus)
            {
                if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right)
                {
                    return true;
                }
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!_allowMouseWheelScrolling)
            {
                MouseWheelEx?.Invoke(this, e);
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            if (_allowFocus)
            {
                //Invalidate();
            }
            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            if (_allowFocus)
            {
                //Invalidate();
            }
            base.OnLeave(e);
        }
    }
}
