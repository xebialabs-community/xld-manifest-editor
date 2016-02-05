/// Copyright (c) 2015, XebiaLabs B.V., All rights reserved.
///
///
/// The Manifest Editor for XL Deploy is licensed under the terms of the GPLv2
/// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most XebiaLabs Libraries.
/// There are special exceptions to the terms and conditions of the GPLv2 as it is applied to
/// this software, see the FLOSS License Exception
/// <https://github.com/jenkinsci/deployit-plugin/blob/master/LICENSE>.
///
/// This program is free software; you can redistribute it and/or modify it under the terms
/// of the GNU General Public License as published by the Free Software Foundation; version 2
/// of the License.
///
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
/// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
/// See the GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License along with this
/// program; if not, write to the Free Software Foundation, Inc., 51 Franklin St, Fifth
/// Floor, Boston, MA 02110-1301  USA
///
using System.Windows;
using System.Windows.Controls;
using XebiaLabs.Deployit.UI.ViewModels;

namespace XebiaLabs.Deployit.UI.Controls
{
	public class TreeViewEx : TreeView
	{
		public static readonly DependencyProperty SelectedTreeViewItemProperty = DependencyProperty.Register("SelectedTreeViewItem", typeof(TreeViewItemViewModel), typeof(TreeViewEx), new UIPropertyMetadata(null, OnSelectedTreeViewItemChanged));

		private static void OnSelectedTreeViewItemChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			var treeViewEx = o as TreeViewEx;
			if (treeViewEx != null)
				treeViewEx.OnSelectedTreeViewItemChanged((TreeViewItemViewModel)e.OldValue, (TreeViewItemViewModel)e.NewValue);
		}

		protected virtual TreeViewItem OnCoerceSelectedTreeViewItem(TreeViewItem value)
		{
			// TODO: Keep the proposed value within the desired range.
			return value;
		}

		protected virtual void OnSelectedTreeViewItemChanged(TreeViewItemViewModel oldValue, TreeViewItemViewModel newValue)
		{

		}

		public TreeViewItemViewModel SelectedTreeViewItem
		{
			get{return (TreeViewItemViewModel)GetValue(SelectedTreeViewItemProperty);}
			set{SetValue(SelectedTreeViewItemProperty, value);}
		}


		/// <summary>
		/// Initializes a new instance of the TreeViewEx class.
		/// </summary>
		public TreeViewEx()
		{
			SelectedItemChanged += TreeViewEx_SelectedItemChanged;
		}

		private void TreeViewEx_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
		    SelectedTreeViewItem = e.NewValue as TreeViewItemViewModel;
		}
	}
}
