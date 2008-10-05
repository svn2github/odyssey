using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

#region Licence
// Copyright (c) 2008 Thomas Gerber
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion
namespace Odyssey.Controls
{
    public class AnimationDecorator : Decorator
    {
        static AnimationDecorator()
        {
          
        }

        public AnimationDecorator()
            : base()
        {
            ClipToBounds = true;
        }

        
        /// <summary>
        /// Specify whether to apply opactiy animation.
        /// </summary>
        public bool OpacityAnimation
        {
            get { return (bool)GetValue(OpacityAnimationProperty); }
            set { SetValue(OpacityAnimationProperty, value); }
        }

        public static readonly DependencyProperty OpacityAnimationProperty =
            DependencyProperty.Register("OpacityAnimation", 
            typeof(bool), 
            typeof(AnimationDecorator),
            new UIPropertyMetadata(true));



        /// <summary>
        /// Gets or sets whether the decorator is expanded or collapsed.
        /// </summary>
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", 
            typeof(bool), 
            typeof(AnimationDecorator),
            new PropertyMetadata(true,IsExpandedChanged));


        public static void IsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimationDecorator expander = d as AnimationDecorator;
            bool expanded = (bool)e.NewValue;
            expander.DoAnimate(expanded);
        }


        /// <summary>
        /// Specify whether to apply animation when IsExpanded is changed.
        /// </summary>
        public DoubleAnimation HeightAnimation
        {
            get { return (DoubleAnimation)GetValue(HeightAnimationProperty); }
            set { SetValue(HeightAnimationProperty, value); }
        }

        public static readonly DependencyProperty HeightAnimationProperty =
            DependencyProperty.Register("HeightAnimation", 
            typeof(DoubleAnimation), 
            typeof(AnimationDecorator), 
            new UIPropertyMetadata(null));



        /// <summary>
        /// Gets or sets the duration for the animation.
        /// </summary>
        public Duration Duration
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Duration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(Duration), typeof(AnimationDecorator), new UIPropertyMetadata(new Duration(new TimeSpan(0,0,0,400))));



        /// <summary>
        /// Perform the animation.
        /// </summary>
        /// <param name="expanded"></param>
        private void DoAnimate(bool expanded)
        {
            if (Child != null)
            {
                if (YOffset > 0) YOffset = 0;
                if (-YOffset > Child.DesiredSize.Height) YOffset = -Child.DesiredSize.Height;
                DoubleAnimation animation = HeightAnimation;
                if (animation == null)
                {
                    animation = new DoubleAnimation();
                    animation.DecelerationRatio = 0.9;
                    animation.Duration = Duration;
                }
                animation.From = null;
                animation.To = expanded ? 0 : -Child.DesiredSize.Height;
                this.BeginAnimation(AnimationDecorator.YOffsetProperty, animation);

                if (OpacityAnimation)
                {
                    animation.From = null;
                    animation.To = expanded ? 1 : 0;
                    this.BeginAnimation(Control.OpacityProperty, animation);
                }
            }
            else
            {
                YOffset = int.MinValue;
            }
        }


        protected void SetYOffset(bool expanded)
        {
            YOffset = expanded ? 0 : -Child.DesiredSize.Height;
        }

        /// <summary>
        /// A helper value for the current state while in animation.
        /// </summary>
        internal Double YOffset
        {
            get { return (Double)GetValue(YOffsetProperty); }
            set { SetValue(YOffsetProperty, value); }
        }

        public static readonly DependencyProperty YOffsetProperty =
            DependencyProperty.Register("YOffset", typeof(Double), typeof(AnimationDecorator),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange
                | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Measures the child element of a <see cref="T:System.Windows.Controls.Decorator"/> to prepare for arranging it during the <see cref="M:System.Windows.Controls.Decorator.ArrangeOverride(System.Windows.Size)"/> pass.
        /// </summary>
        /// <param name="constraint">An upper limit <see cref="T:System.Windows.Size"/> that should not be exceeded.</param>
        /// <returns>
        /// The target <see cref="T:System.Windows.Size"/> of the element.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            if (Child == null) return new Size(0, 0);

            Child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Size size = new Size();
            size.Width = DesiredSize.Width;
            size.Height = Child.DesiredSize.Height;
            Double h = size.Height + YOffset;
            if (h < 0) h = 0;
            size.Height = h;
            if (Child != null) Child.IsEnabled = h > 0;
            return size;
        }


        /// <summary>
        /// Arranges the content of a <see cref="T:System.Windows.Controls.Decorator"/> element.
        /// </summary>
        /// <param name="arrangeSize">The <see cref="T:System.Windows.Size"/> this element uses to arrange its child content.</param>
        /// <returns>
        /// The <see cref="T:System.Windows.Size"/> that represents the arranged size of this <see cref="T:System.Windows.Controls.Decorator"/> element and its child.
        /// </returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Child == null) return arrangeSize;
            Size size = new Size();
            size.Width = arrangeSize.Width;
            size.Height = Child.DesiredSize.Height;

            Point p = new Point(0, YOffset);

            Child.Arrange(new Rect(p, size));

            Double h = Child.DesiredSize.Height + YOffset;
            if (h < 0) h = 0;
            size.Height = h;
            return size;
        }
    }
}
