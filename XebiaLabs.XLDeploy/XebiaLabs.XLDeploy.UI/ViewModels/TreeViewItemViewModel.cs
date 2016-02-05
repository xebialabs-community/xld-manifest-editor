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
using System.Collections.Generic;
using Microsoft.Practices.Prism.ViewModel;
using System.Collections.ObjectModel;

namespace XebiaLabs.Deployit.UI.ViewModels
{
	public class TreeViewItemViewModel : NotificationObject
	{
		#region Data

        private static readonly TreeViewItemViewModel DUMMY_CHILD = new TreeViewItemViewModel();

		private readonly ObservableCollection<TreeViewItemViewModel> _children = new ObservableCollection<TreeViewItemViewModel>();
		private readonly TreeViewItemViewModel _parent;

        private bool _isExpanded;
        private bool _isSelected;
        private readonly bool _lazyLoadChildren;
        private bool _childrenInitialized;
        private bool _childrenLoaded;
	    private string _treeItemLabel;

	    #endregion // Data

		#region Constructors

		protected TreeViewItemViewModel(TreeViewItemViewModel parent)
		{
			_parent = parent;

			_lazyLoadChildren = true;
			_childrenInitialized = false;
			_childrenLoaded = false;
		}

		// This is used to create the DummyChild instance.
		private TreeViewItemViewModel()
		{
		}

		#endregion // Constructors

		#region Presentation Members

	    public string TreeItemLabel
	    {
	        get { return _treeItemLabel; }
	        protected set
	        {
	            if (value == _treeItemLabel) return;
	            _treeItemLabel = value;
	            RaisePropertyChanged(() => TreeItemLabel);
	        }
	    }

	    #region Children

		/// <summary>
		/// Returns the logical child items of this object.
		/// </summary>
		public ObservableCollection<TreeViewItemViewModel> Children
		{
			get
			{
			    if (_childrenInitialized)
			    {
			        return _children;
			    }
			    _childrenInitialized = true;
			    if (_lazyLoadChildren && HasChildren == true)
			    {
			        _children.Add(DUMMY_CHILD);
			    }
			    return _children;
			}
		}

		#endregion // Children

		#region HasLoadedChildren

		/// <summary>
		/// Returns true if this object's Children have not yet been populated.
		/// </summary>
		public bool HasDummyChild
		{
			get { return Children.Count == 1 && Children[0] == DUMMY_CHILD; }
		}

		#endregion // HasLoadedChildren

		#region IsExpanded

		/// <summary>
		/// Gets/sets whether the TreeViewItem
		/// associated with this object is expanded.
		/// </summary>
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				if (value != _isExpanded)
				{
					_isExpanded = value;
					RaisePropertyChanged("IsExpanded");
				}

				// Expand all the way up to the root.
				if (_isExpanded && _parent != null)
					_parent.IsExpanded = true;

				// Lazy load the child items, if necessary.
			    if (!_lazyLoadChildren || _childrenLoaded) { return; }
			    _childrenLoaded = true;
			    Children.Clear();
			    LoadChildren();
			}
		}

		#endregion // IsExpanded

		#region IsSelected

		/// <summary>
		/// Gets/sets whether the TreeViewItem
		/// associated with this object is selected.
		/// </summary>
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
			    if (value == _isSelected) {return;}
			    _isSelected = value;
			    RaisePropertyChanged("IsSelected");
			}
		}

		#endregion // IsSelected

		#region LoadChildren

		/// <summary>
		/// Invoked when the child items need to be loaded on demand.
		/// Subclasses can override this to populate the Children collection.
		/// </summary>
		protected virtual void LoadChildren()
		{
		}

	    public virtual bool? HasChildren
	    {
	        get { return null; }
	    }

	    #endregion // LoadChildren

		#region Parent

		public TreeViewItemViewModel Parent
		{
			get { return _parent; }
		}

		#endregion // Parent

        public List<MenuItemViewModel> MenuItems { get; protected set; }

		#endregion // Presentation Members

	}
}
