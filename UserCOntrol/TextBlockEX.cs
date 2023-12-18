//-----------------------------------------------------------------------
// <copyright file="TextBlockEX.cs" company="">
//     Author: Zhu Lei
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace IRW.UserControls
{
    public class TextBlockEX : TextBlock
    {
        TextPointer StartSelectPosition;
        TextPointer EndSelectPosition;
        public String SelectedText = string.Empty;
        public delegate void TextSelectedHandler(string SelectedText);

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            StartSelectPosition = GetPositionFromPoint(e.GetPosition(this), true);
            TextRange tr = new TextRange(ContentStart, ContentEnd);
            tr.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Transparent));
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            try
            {
                base.OnMouseUp(e);
                EndSelectPosition = GetPositionFromPoint(e.GetPosition(this), true);
                //TextRange ctr = new TextRange(this.ContentStart, this.ContentEnd);
                TextRange str = new TextRange(StartSelectPosition, EndSelectPosition);
                str.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Gray));

                if(!string.IsNullOrEmpty(str.Text))
                {
                    SelectedText = str.Text;
                    Clipboard.SetText(SelectedText);
                }
            }
            catch
            {
            }
        }
    }
}
