﻿//-----------------------------------------------------------------------
// <copyright file="TreeViewItemExtensions.cs" company="">
//     Author: Zhu Lei
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Windows.Controls;
using System.Windows.Media;

namespace IRW.Themes
{
    public static class TreeViewItemExtensions
    {
        public static int GetDepth(this TreeViewItem item)
        {
            TreeViewItem parent;
            while((parent = GetParent(item)) != null)
            {
                return GetDepth(parent) + 1;
            }
            return 0;
        }

        private static TreeViewItem GetParent(TreeViewItem item)
        {
            System.Windows.DependencyObject parent = VisualTreeHelper.GetParent(item);

            while(!(parent is TreeViewItem || parent is TreeView))
            {
                if(parent == null)
                    return null;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as TreeViewItem;
        }
    }
}
