﻿using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using RcisSchoolBell.lib.MaterialSkin;

namespace RcisSchoolBell.Controls
{
	public class MaterialListView : ListView, IMaterialControl
	{
		[Browsable(false)]
		public int Depth { get; set; }
		[Browsable(false)]
		public MaterialSkinManager SkinManager { get { return MaterialSkinManager.Instance; } }
		[Browsable(false)]
		public MouseState MouseState { get; set; }
		[Browsable(false)]
		public Point MouseLocation { get; set; }

		public MaterialListView()
		{
			GridLines = false;
			FullRowSelect = true;
			HeaderStyle = ColumnHeaderStyle.Nonclickable;
			View = View.Details;
			OwnerDraw = true;
			ResizeRedraw = true;
			BorderStyle = BorderStyle.None;
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);

			//Fix for hovers, by default it doesn't redraw
			//TODO: should only redraw when the hovered line changed, this to reduce unnecessary redraws
			MouseLocation = new Point(-1, -1);
			MouseState = MouseState.Out;
			MouseEnter += delegate
			{
				MouseState = MouseState.Hover;
			}; 
			MouseLeave += delegate
			{
				MouseState = MouseState.Out; 
				MouseLocation = new Point(-1, -1);
				Invalidate();
			};
			MouseDown += delegate { MouseState = MouseState.Down; };
			MouseUp += delegate{ MouseState = MouseState.Hover; };
			MouseMove += delegate(object sender, MouseEventArgs args)
			{
				MouseLocation = args.Location;
				Invalidate();
			};
		}

		protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
		{
			e.Graphics.FillRectangle(new SolidBrush(SkinManager.GetApplicationBackgroundColor()), new Rectangle(e.Bounds.X, e.Bounds.Y, Width, e.Bounds.Height));
			e.Graphics.DrawString(e.Header.Text, 
				SkinManager.Ui, 
				SkinManager.GetSecondaryTextBrush(),
				new Rectangle(e.Bounds.X + ItemPadding, e.Bounds.Y + ItemPadding, e.Bounds.Width - ItemPadding * 2, e.Bounds.Height - ItemPadding * 2), 
				GetStringFormat());
		}

		private const int ItemPadding = 3;
		protected override void OnDrawItem(DrawListViewItemEventArgs e)
		{
			//We draw the current line of items (= item with subitems) on a temp bitmap, then draw the bitmap at once. This is to reduce flickering.
			var b = new Bitmap(e.Item.Bounds.Width, e.Item.Bounds.Height);
			var g = Graphics.FromImage(b);

			//always draw default background
			g.FillRectangle(new SolidBrush(SkinManager.GetApplicationBackgroundColor()), new Rectangle(new Point(e.Bounds.X, 0), e.Bounds.Size));
			
			if (e.State.HasFlag(ListViewItemStates.Selected))
			{
				//selected background
				g.FillRectangle(SkinManager.GetFlatButtonPressedBackgroundBrush(), new Rectangle(new Point(e.Bounds.X, 0), e.Bounds.Size));
			}
			else if (e.Bounds.Contains(MouseLocation) && MouseState == MouseState.Hover)
			{
				//hover background
				g.FillRectangle(SkinManager.GetFlatButtonHoverBackgroundBrush(), new Rectangle(new Point(e.Bounds.X, 0), e.Bounds.Size));
			}


			//Draw seperator
			g.DrawLine(new Pen(SkinManager.GetDividersColor()), e.Bounds.Left, 0, e.Bounds.Right, 0);
			
			foreach (ListViewItem.ListViewSubItem subItem in e.Item.SubItems)
			{
				//Draw text
				g.DrawString(subItem.Text, SkinManager.Ui, SkinManager.GetPrimaryTextBrush(),
								 new Rectangle(subItem.Bounds.Location.X + ItemPadding, ItemPadding, subItem.Bounds.Width - 2 * ItemPadding, subItem.Bounds.Height - 2 * ItemPadding),
								 GetStringFormat());
			}

			e.Graphics.DrawImage((Image) b.Clone(), e.Item.Bounds.Location);
			g.Dispose();
			b.Dispose();
		}

		private StringFormat GetStringFormat()
		{
			return new StringFormat
			{
				FormatFlags = StringFormatFlags.LineLimit,
				Trimming = StringTrimming.EllipsisCharacter,
				Alignment = StringAlignment.Near,
				LineAlignment = StringAlignment.Center
			};
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			//This is a hax for the needed padding.
			//Another way would be intercepting all ListViewItems and changing the sizes, but really, that will be a lot of work
			//This will do for now.
			Font = new Font(SkinManager.Ui.FontFamily, 8.5f);
		}
	}
}
